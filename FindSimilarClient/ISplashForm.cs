/*
http://codehunter-uk.blogspot.no/2009/04/super-splash-screen-in-c.html
 */

using System.ComponentModel;

namespace FindSimilar
{
    /// <summary>
    ///     Description of ISplashForm.
    /// </summary>
    public interface ISplashForm : ISynchronizeInvoke
    {
        void UpdateStatus(string status);
        void UpdateProgress(int progress);
        void UpdateInfo(string info);
    }
}