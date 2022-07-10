/*
 * Created by SharpDevelop.
 * User: perivar
 * Date: 07.08.2014
 * Time: 20:04
 */

using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace FindSimilar
{
    /// <summary>
    ///     Description of SplashSceenWaitingForm.
    /// </summary>
    public partial class SplashSceenWaitingForm : Form
    {
        /// <summary>
        ///     Delegate for the DoWork event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Contains the event data.</param>
        public delegate void DoWorkEventHandler(SplashSceenWaitingForm sender, DoWorkEventArgs e);

        private int lastPercent;
        private string lastStatus;

        public SplashSceenWaitingForm()
        {
            //
            // The InitializeComponent() call is required for Windows Forms designer support.
            //
            InitializeComponent();

            //
            // TODO: Add constructor code after the InitializeComponent() call.
            //
            DefaultStatusText = "Please wait...";
            CancellingText = "Cancelling operation...";
        }

        /// <summary>
        ///     Gets the progress bar so it is possible to customize it
        ///     before displaying the form.
        ///     Do not use it directly from the background worker function!
        /// </summary>
        public ProgressBar ProgressBar { get; private set; }

        /// <summary>
        ///     Will be passed to the background worker.
        /// </summary>
        public object Argument { get; set; }

        /// <summary>
        ///     Background worker's result.
        ///     You may also check ShowDialog return value
        ///     to know how the background worker finished.
        /// </summary>

        public RunWorkerCompletedEventArgs Result { get; private set; }

        /// <summary>
        ///     True if the user clicked the Cancel button
        ///     and the background worker is still running.
        /// </summary>
        public bool CancellationPending => worker.CancellationPending;

        /// <summary>
        ///     Text displayed once the Cancel button is clicked.
        /// </summary>
        public string CancellingText { get; set; }

        /// <summary>
        ///     Default status text.
        /// </summary>
        public string DefaultStatusText { get; set; }

        /// <summary>
        ///     Occurs when the background worker starts.
        /// </summary>
        public event DoWorkEventHandler DoWork;

        public void UpdateStatus(string status)
        {
            lblStatus.Text = status;
        }

        public void UpdateProgress(int progress)
        {
            ProgressBar.Value = progress;
        }

        /// <summary>
        ///     Changes the status text only.
        /// </summary>
        /// <param name="status">New status text.</param>
        public void SetProgress(string status)
        {
            //do not update the text if it didn't change
            //or if a cancellation request is pending
            if (status != lastStatus && !worker.CancellationPending)
            {
                lastStatus = status;
                worker.ReportProgress(ProgressBar.Minimum - 1, status);
            }
        }

        /// <summary>
        ///     Changes the progress bar value only.
        /// </summary>
        /// <param name="percent">New value for the progress bar.</param>
        public void SetProgress(int percent)
        {
            //do not update the progress bar if the value didn't change
            if (percent != lastPercent)
            {
                lastPercent = percent;
                worker.ReportProgress(percent);
            }
        }

        /// <summary>
        ///     Changes both progress bar value and status text.
        /// </summary>
        /// <param name="percent">New value for the progress bar.</param>
        /// <param name="status">New status text.</param>
        public void SetProgress(int percent, string status)
        {
            //update the form is at least one of the values need to be updated
            if (percent != lastPercent || (status != lastStatus && !worker.CancellationPending))
            {
                lastPercent = percent;
                lastStatus = status;
                worker.ReportProgress(percent, status);
            }
        }

        private void WorkerDoWork(object sender, DoWorkEventArgs e)
        {
            //the background worker started
            //let's call the user's event handler
            if (DoWork != null) DoWork(this, e);
        }

        private void WorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //the background worker completed
            //keep the resul and close the form
            Result = e;

            if (e.Error != null)
                DialogResult = DialogResult.Abort;
            else if (e.Cancelled)
                DialogResult = DialogResult.Cancel;
            else
                DialogResult = DialogResult.OK;

            Close();
        }

        private void WorkerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //make sure the new value is valid for the progress bar and update it
            if (e.ProgressPercentage >= ProgressBar.Minimum &&
                e.ProgressPercentage <= ProgressBar.Maximum)
                UpdateProgress(e.ProgressPercentage);

            //do not update the text if a cancellation request is pending
            if (e.UserState != null && !worker.CancellationPending) UpdateStatus(e.UserState.ToString());
        }

        private void SplashSceenWaitingFormLoad(object sender, EventArgs e)
        {
            // reset to defaults just in case the user wants to reuse the form
            Result = null;
            buttonCancel.Enabled = true;
            ProgressBar.Value = ProgressBar.Minimum;
            UpdateStatus(DefaultStatusText);
            lastStatus = DefaultStatusText;
            lastPercent = ProgressBar.Minimum;

            // start the background worker as soon as the form is loaded
            worker.RunWorkerAsync(Argument);
        }

        private void ButtonCancelClick(object sender, EventArgs e)
        {
            // notify the background worker we want to cancel
            worker.CancelAsync();

            // disable the cancel button and change the status text
            buttonCancel.Enabled = false;
            UpdateStatus(CancellingText);
        }
    }
}