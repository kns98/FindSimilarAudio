﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using CommonUtils;
using ZedGraph;
// for using Parallell tasks (dot product)

// for MathUtils and ColorUtils

// For drawing graph

namespace Comirva.Audio.Util.Maths
{
    /// Matrix
    /// <P>
    ///     The Matrix Class provides the fundamental operations of numerical
    ///     linear algebra.  Various constructors create Matrices from two dimensional
    ///     arrays of double precision floating point numbers.  Various "gets" and
    ///     "sets" provide access to submatrices and matrix elements.  Several methods
    ///     implement basic matrix arithmetic, including matrix addition and
    ///     multiplication, matrix norms, and element-by-element array operations.
    ///     Methods for reading and printing matrices are also included.  All the
    ///     operations in this version of the Matrix Class involve real matrices.
    ///     Complex matrices may be handled in matrixData future version.
    ///     <P>
    ///         Five fundamental matrix decompositions, which consist of pairs or triples
    ///         of matrices, permutation vectors, and the like, produce results in five
    ///         decomposition classes.  These decompositions are accessed by the Matrix
    ///         class to compute solutions of simultaneous linear equations, determinants,
    ///         inverses and other matrix functions.  The five decompositions are:
    ///         <P>
    ///             <UL>
    ///                 <LI>
    ///                     Cholesky Decomposition of symmetric, positive definite matrices.
    ///                     <LI>
    ///                         LU Decomposition of rectangular matrices.
    ///                         <LI>
    ///                             QR Decomposition of rectangular matrices.
    ///                             <LI>
    ///                                 Singular Value Decomposition of rectangular matrices.
    ///                                 <LI>Eigenvalue Decomposition of both symmetric and nonsymmetric square matrices.
    ///             </UL>
    ///             <DL>
    ///                 <DT>
    ///                     <B>Example of use:</B>
    ///                 </DT>
    ///                 <DD>
    ///                     Solve matrixData linear system matrixData x = b and compute the residual norm, ||b - matrixData
    ///                     x||.
    ///                     <P>
    ///                         <PRE>
    ///                             var vals = {{1.,2.,3},{4.,5.,6.},{7.,8.,10.}};
    ///                             Matrix matrixData = new Matrix(vals);
    ///                             Matrix b = Matrix.random(3,1);
    ///                             Matrix x = matrixData.solve(b);
    ///                             Matrix r = matrixData.times(x).minus(b);
    ///                             double rnorm = r.normInf();
    ///                         </PRE>
    ///                     </P>
    ///                 </DD>
    ///             </DL>
    ///             @author The MathWorks, Inc. and the National Institute of Standards and Technology.
    ///             @version 5 August 1998
    ///             @version 2015 - Enhancements by perivar@nerseth.com
    public class Matrix
    {
        // Epsilon, used for comparing if matrices are identical
        private const double EPSILON = 0.00000001;
        // ------------------------
        //   Class variables
        // ------------------------

        // Random to make sure the seed is different and we actually get real random numbers when using it
        private static readonly Random random = new Random();

        //Number of columns.

        // Array for internal storage of elements.

        //Number of rows.

        #region Resize

        public Matrix Resize(int newRows, int newColumns)
        {
            var resizedMatrix = new Matrix(newRows, newColumns);
            var rows = Math.Min(Rows, newRows);
            var columns = Math.Min(Columns, newColumns);
            for (var i = 0; i < rows; i++)
            for (var j = 0; j < columns; j++)
                resizedMatrix.MatrixData[i][j] = MatrixData[i][j];
            return resizedMatrix;
        }

        #endregion

        #region Public Static methods

        /// <summary>
        ///     Compare two matrices
        /// </summary>
        /// <param name="matrixA">Matrix A</param>
        /// <param name="matrixB">Matrix B</param>
        /// <param name="epsilon">Epsilon, e.g. what are the maximum difference accepted</param>
        /// <returns>True if the matrixes are similar</returns>
        /// <remarks>Developed by James McCaffrey</remarks>
        /// <see cref="http://msdn.microsoft.com/en-us/magazine/jj863137.aspx">Matrix Decomposition</see>
        public static bool MatrixAreEqual(double[][] matrixA, double[][] matrixB, double epsilon)
        {
            // true if all values in matrixA == corresponding values in matrixB
            var aRows = matrixA.Length;
            var aCols = matrixA[0].Length;
            var bRows = matrixB.Length;
            var bCols = matrixB[0].Length;
            if (aRows != bRows || aCols != bCols)
                throw new Exception("Non-conformable matrices in MatrixAreEqual");

            for (var i = 0; i < aRows; ++i) // each row of A and B
            for (var j = 0; j < aCols; ++j) // each col of A and B
                if (Math.Abs(matrixA[i][j] - matrixB[i][j]) > epsilon)
                    return false;
            return true;
        }

        #endregion

        #region Setters and Getters

        public int Rows { get; set; }

        public int Columns { get; set; }

        public double[][] MatrixData { get; set; }

        #endregion

        #region Constructors

        // ------------------------
        //   Constructors
        // ------------------------

        /// <summary>Construct an rows-by-columns matrix of zeros.</summary>
        /// <param name="rows">Number of rows.</param>
        /// <param name="columns">Number of colums.</param>
        public Matrix(int rows, int columns)
        {
            Rows = rows;
            Columns = columns;
            MatrixData = new double[rows][];
            for (var i = 0; i < rows; i++)
                MatrixData[i] = new double[columns];
        }

        /// <summary>Construct an rows-by-columns constant matrix.</summary>
        /// <param name="rows">Number of rows.</param>
        /// <param name="columns">Number of colums.</param>
        /// <param name="s">Fill the matrix with this scalar value.</param>
        public Matrix(int rows, int columns, double s)
        {
            Rows = rows;
            Columns = columns;
            MatrixData = new double[rows][];
            for (var i = 0; i < rows; i++)
            {
                MatrixData[i] = new double[columns];
                for (var j = 0; j < columns; j++) MatrixData[i][j] = s;
            }
        }

        /// <summary>Construct matrixData matrix from matrixData 2-D array.</summary>
        /// <param name="matrixData">Two-dimensional array of doubles.</param>
        /// <exception cref="">ArgumentException All rows must have the same length</exception>
        /// <seealso cref="">#constructWithCopy</seealso>
        public Matrix(double[][] matrixData)
        {
            Rows = matrixData.Length;
            Columns = matrixData[0].Length;
            for (var i = 0; i < Rows; i++)
                if (matrixData[i].Length != Columns)
                    throw new ArgumentException("All rows must have the same length.");
            MatrixData = matrixData;
        }

        /// <summary>Construct matrixData matrix quickly without checking arguments.</summary>
        /// <param name="matrixData">Two-dimensional array of doubles.</param>
        /// <param name="rows">Number of rows.</param>
        /// <param name="columns">Number of colums.</param>
        public Matrix(double[][] matrixData, int rows, int columns)
        {
            MatrixData = matrixData;
            Rows = rows;
            Columns = columns;
        }

        /// <summary>Construct matrixData matrix from matrixData one-dimensional packed array</summary>
        /// <param name="vals">One-dimensional array of doubles, packed by columns (ala Fortran).</param>
        /// <param name="rows">Number of rows.</param>
        /// <exception cref="ArgumentException">Array length must be matrixData multiple of rows.</exception>
        public Matrix(double[] vals, int rows)
        {
            Rows = rows;
            Columns = rows != 0 ? vals.Length / rows : 0;
            if (rows * Columns != vals.Length)
                throw new ArgumentException("Array length must be matrixData multiple of rows.");

            MatrixData = new double[rows][];
            for (var i = 0; i < rows; i++)
            {
                MatrixData[i] = new double[Columns];
                for (var j = 0; j < Columns; j++) MatrixData[i][j] = vals[i + j * rows];
            }
        }

        /// <summary>
        ///     Construct matrixData matrix from a two-dimensional array
        /// </summary>
        /// <param name="matrix2DArray">2D array</param>
        public Matrix(double[,] matrix2DArray, int rows, int columns)
        {
            Rows = rows;
            Columns = columns;

            MatrixData = new double[rows][];
            for (var i = 0; i < rows; i++)
            {
                MatrixData[i] = new double[columns];
                for (var j = 0; j < columns; j++) MatrixData[i][j] = matrix2DArray[i, j];
            }
        }

        #endregion

        // ------------------------
        // Public Methods
        // ------------------------

        #region Constructors, Copy and Clone

        /// <summary>Construct matrixData matrix from matrixData copy of matrixData 2-D array.</summary>
        /// <param name="matrixData">Two-dimensional array of doubles.</param>
        /// <exception cref="">ArgumentException All rows must have the same length</exception>
        public static Matrix ConstructWithCopy(double[][] matrixData)
        {
            var rows = matrixData.Length;
            var columns = matrixData[0].Length;
            var X = new Matrix(rows, columns);
            var C = X.GetArray();
            for (var i = 0; i < rows; i++)
            {
                if (matrixData[i].Length != columns) throw new ArgumentException("All rows must have the same length.");
                for (var j = 0; j < columns; j++) C[i][j] = matrixData[i][j];
            }

            return X;
        }

        /// Make matrixData deep copy of matrixData matrix
        public Matrix Copy()
        {
            var X = new Matrix(Rows, Columns);
            var C = X.GetArray();
            for (var i = 0; i < Rows; i++)
            for (var j = 0; j < Columns; j++)
                C[i][j] = MatrixData[i][j];
            return X;
        }

        /// Clone the Matrix object.
        public object Clone()
        {
            return Copy();
        }

        #endregion

        #region Get Methods

        /// <summary>
        ///     Access the internal two-dimensional array.
        /// </summary>
        /// <returns>The two-dimensional array of matrix elements.</returns>
        public double[][] GetArray()
        {
            return MatrixData;
        }

        /// <summary>
        ///     Return the internal two-dimensional jagged array as a two dimensional array.
        /// </summary>
        /// <returns>A two-dimensional array of matrix elements.</returns>
        public double[,] GetTwoDimensionalArray()
        {
            var d = new double[Rows, Columns];

            for (var i = 0; i < Rows; i++)
            for (var j = 0; j < Columns; j++)
                d[i, j] = MatrixData[i][j];
            return d;
        }

        /// <summary>Copy the internal two-dimensional array.</summary>
        /// <returns>Two-dimensional array copy of matrix elements.</returns>
        public double[][] GetArrayCopy()
        {
            var C = new double[Rows][];
            for (var i = 0; i < Rows; i++)
            {
                C[i] = new double[Columns];
                for (var j = 0; j < Columns; j++) C[i][j] = MatrixData[i][j];
            }

            return C;
        }

        /// <summary>Make matrixData one-dimensional column packed copy of the internal array.</summary>
        /// <returns>Matrix elements packed in matrixData one-dimensional array by columns.</returns>
        public double[] GetColumnPackedCopy()
        {
            var vals = new double[Rows * Columns];
            for (var i = 0; i < Rows; i++)
            for (var j = 0; j < Columns; j++)
                vals[i + j * Rows] = MatrixData[i][j];
            return vals;
        }

        /// <summary>Make matrixData one-dimensional row packed copy of the internal array.</summary>
        /// <returns>Matrix elements packed in matrixData one-dimensional array by rows.</returns>
        public double[] GetRowPackedCopy()
        {
            var vals = new double[Rows * Columns];
            for (var i = 0; i < Rows; i++)
            for (var j = 0; j < Columns; j++)
                vals[i * Columns + j] = MatrixData[i][j];
            return vals;
        }

        /// <summary>Get row dimension.</summary>
        /// <returns>rows, the number of rows.</returns>
        public int GetRowDimension()
        {
            return Rows;
        }

        /// <summary>Get column dimension.</summary>
        /// <returns>columns, the number of columns.</returns>
        public int GetColumnDimension()
        {
            return Columns;
        }

        /// <summary>Get matrixData single element.</summary>
        /// <param name="i">Row index.</param>
        /// <param name="j">Column index.</param>
        /// <returns>matrixData[i][j]</returns>
        public double Get(int i, int j)
        {
            return MatrixData[i][j];
        }

        /// <summary>
        ///     Return a Column from the matrix
        /// </summary>
        /// <param name="column">column number</param>
        /// <returns></returns>
        public double[] GetColumn(int column)
        {
            if (column > -1 && column < Columns) return MatrixData.Select(row => row[column]).ToArray();
            return null;
        }

        /// <summary>
        ///     Return a Row from the matrix
        /// </summary>
        /// <param name="row">row number</param>
        /// <returns></returns>
        public double[] GetRow(int row)
        {
            if (row > -1 && row < Rows) return MatrixData[row];
            return null;
        }

        /// <summary>Get matrixData submatrix.</summary>
        /// <param name="i0">Initial row index</param>
        /// <param name="i1">Final row index</param>
        /// <param name="j0">Initial column index</param>
        /// <param name="j1">Final column index</param>
        /// <returns>matrixData(i0:i1,j0:j1)</returns>
        /// <exception cref="IndexOutOfRangeException">Submatrix indices</exception>
        public Matrix GetMatrix(int i0, int i1, int j0, int j1)
        {
            var X = new Matrix(i1 - i0 + 1, j1 - j0 + 1);
            var B = X.GetArray();
            try
            {
                for (var i = i0; i <= i1; i++)
                for (var j = j0; j <= j1; j++)
                    B[i - i0][j - j0] = MatrixData[i][j];
            }
            catch (Exception)
            {
                throw new Exception("Submatrix indices");
            }

            return X;
        }

        /// <summary>Get matrixData submatrix.</summary>
        /// <param name="r">Array of row indices.</param>
        /// <param name="c">Array of column indices.</param>
        /// <returns>matrixData(r(:),c(:))</returns>
        /// <exception cref="IndexOutOfRangeException">Submatrix indices</exception>
        public Matrix GetMatrix(int[] r, int[] c)
        {
            var X = new Matrix(r.Length, c.Length);
            var B = X.GetArray();
            try
            {
                for (var i = 0; i < r.Length; i++)
                for (var j = 0; j < c.Length; j++)
                    B[i][j] = MatrixData[r[i]][c[j]];
            }
            catch (Exception)
            {
                throw new Exception("Submatrix indices");
            }

            return X;
        }

        /// <summary>Get matrixData submatrix.</summary>
        /// <param name="i0">Initial row index</param>
        /// <param name="i1">Final row index</param>
        /// <param name="c">Array of column indices.</param>
        /// <returns>matrixData(i0:i1,c(:))</returns>
        /// <exception cref="IndexOutOfRangeException">Submatrix indices</exception>
        public Matrix GetMatrix(int i0, int i1, int[] c)
        {
            var X = new Matrix(i1 - i0 + 1, c.Length);
            var B = X.GetArray();
            try
            {
                for (var i = i0; i <= i1; i++)
                for (var j = 0; j < c.Length; j++)
                    B[i - i0][j] = MatrixData[i][c[j]];
            }
            catch (Exception)
            {
                throw new Exception("Submatrix indices");
            }

            return X;
        }

        /// <summary>Get matrixData submatrix.</summary>
        /// <param name="r">Array of row indices.</param>
        /// <param name="j0">Initial column index</param>
        /// <param name="j1">Final column index</param>
        /// <returns>matrixData(r(:),j0:j1)</returns>
        /// <exception cref="">IndexOutOfRangeException Submatrix indices</exception>
        public Matrix GetMatrix(int[] r, int j0, int j1)
        {
            var X = new Matrix(r.Length, j1 - j0 + 1);
            var B = X.GetArray();
            try
            {
                for (var i = 0; i < r.Length; i++)
                for (var j = j0; j <= j1; j++)
                    B[i][j - j0] = MatrixData[r[i]][j];
            }
            catch (Exception)
            {
                throw new Exception("Submatrix indices");
            }

            return X;
        }

        #endregion

        #region Set Methods

        /// <summary>Set matrixData single element.</summary>
        /// <param name="i">Row index.</param>
        /// <param name="j">Column index.</param>
        /// <param name="s">matrixData(i,j).</param>
        /// <exception cref="">IndexOutOfRangeException</exception>
        public void Set(int i, int j, double s)
        {
            MatrixData[i][j] = s;
        }

        /// <summary>Set matrixData submatrix.</summary>
        /// <param name="i0">Initial row index</param>
        /// <param name="i1">Final row index</param>
        /// <param name="j0">Initial column index</param>
        /// <param name="j1">Final column index</param>
        /// <param name="X">matrixData(i0:i1,j0:j1)</param>
        /// <exception cref="">Exception Submatrix indices</exception>
        public void SetMatrix(int i0, int i1, int j0, int j1, Matrix X)
        {
            try
            {
                for (var i = i0; i <= i1; i++)
                for (var j = j0; j <= j1; j++)
                    MatrixData[i][j] = X.Get(i - i0, j - j0);
            }
            catch (Exception)
            {
                throw new Exception("Submatrix indices");
            }
        }

        /// <summary>Set matrixData submatrix.</summary>
        /// <param name="r">Array of row indices.</param>
        /// <param name="c">Array of column indices.</param>
        /// <param name="X">matrixData(r(:),c(:))</param>
        /// <exception cref="">Exception Submatrix indices</exception>
        public void SetMatrix(int[] r, int[] c, Matrix X)
        {
            try
            {
                for (var i = 0; i < r.Length; i++)
                for (var j = 0; j < c.Length; j++)
                    MatrixData[r[i]][c[j]] = X.Get(i, j);
            }
            catch (Exception)
            {
                throw new Exception("Submatrix indices");
            }
        }

        /// <summary>Set matrixData submatrix.</summary>
        /// <param name="r">Array of row indices.</param>
        /// <param name="j0">Initial column index</param>
        /// <param name="j1">Final column index</param>
        /// <param name="X">matrixData(r(:),j0:j1)</param>
        /// <exception cref="">Exception Submatrix indices</exception>
        public void SetMatrix(int[] r, int j0, int j1, Matrix X)
        {
            try
            {
                for (var i = 0; i < r.Length; i++)
                for (var j = j0; j <= j1; j++)
                    MatrixData[r[i]][j] = X.Get(i, j - j0);
            }
            catch (Exception)
            {
                throw new Exception("Submatrix indices");
            }
        }

        /// <summary>Set matrixData submatrix.</summary>
        /// <param name="i0">Initial row index</param>
        /// <param name="i1">Final row index</param>
        /// <param name="c">Array of column indices.</param>
        /// <param name="X">matrixData(i0:i1,c(:))</param>
        /// <exception cref="">Exception Submatrix indices</exception>
        public void SetMatrix(int i0, int i1, int[] c, Matrix X)
        {
            try
            {
                for (var i = i0; i <= i1; i++)
                for (var j = 0; j < c.Length; j++)
                    MatrixData[i][c[j]] = X.Get(i - i0, j);
            }
            catch (Exception)
            {
                throw new Exception("Submatrix indices");
            }
        }

        #endregion

        #region Math and Matrix Methods

        /// <summary>Matrix transpose.</summary>
        /// <returns>matrixData'</returns>
        public Matrix Transpose()
        {
            var X = new Matrix(Columns, Rows);
            var C = X.GetArray();
            for (var i = 0; i < Rows; i++)
            for (var j = 0; j < Columns; j++)
                C[j][i] = MatrixData[i][j];
            return X;
        }

        /// <summary>
        ///     Find maximum number when all numbers are made positive.
        /// </summary>
        /// <returns>max value</returns>
        public double Max()
        {
            var max = MatrixData.Max(b => b.Max(v => Math.Abs(v)));
            return max;
        }

        /// <summary>
        ///     Find minimum number when all numbers are made positive.
        /// </summary>
        /// <returns>min value</returns>
        public double Min()
        {
            var min = MatrixData.Min(b => b.Min(v => Math.Abs(v)));
            return min;
        }

        /// <summary>One norm</summary>
        /// <returns>maximum column sum.</returns>
        public double Norm1()
        {
            double f = 0;
            for (var j = 0; j < Columns; j++)
            {
                double s = 0;
                for (var i = 0; i < Rows; i++) s += Math.Abs(MatrixData[i][j]);
                f = Math.Max(f, s);
            }

            return f;
        }

        /// <summary>Two norm</summary>
        /// <returns>maximum singular value.</returns>
        public double Norm2()
        {
            return new SingularValueDecomposition(this).Norm2();
        }

        /// <summary>Infinity norm</summary>
        /// <returns>maximum row sum.</returns>
        public double NormInf()
        {
            double f = 0;
            for (var i = 0; i < Rows; i++)
            {
                double s = 0;
                for (var j = 0; j < Columns; j++) s += Math.Abs(MatrixData[i][j]);
                f = Math.Max(f, s);
            }

            return f;
        }

        /// <summary>Frobenius norm</summary>
        /// <returns>sqrt of sum of squares of all elements.</returns>
        public double NormF()
        {
            double f = 0;
            for (var i = 0; i < Rows; i++)
            for (var j = 0; j < Columns; j++)
                f = MathUtils.Hypot(f, MatrixData[i][j]);
            return f;
        }

        /// <summary>Unary minus</summary>
        /// <returns>-matrixData</returns>
        public Matrix Uminus()
        {
            var X = new Matrix(Rows, Columns);
            var C = X.GetArray();
            for (var i = 0; i < Rows; i++)
            for (var j = 0; j < Columns; j++)
                C[i][j] = -MatrixData[i][j];
            return X;
        }

        /// <summary>C = matrixData + B</summary>
        /// <param name="B">another matrix</param>
        /// <returns>matrixData + B</returns>
        public Matrix Plus(Matrix B)
        {
            CheckMatrixDimensions(B);
            var X = new Matrix(Rows, Columns);
            var C = X.GetArray();
            for (var i = 0; i < Rows; i++)
            for (var j = 0; j < Columns; j++)
                C[i][j] = MatrixData[i][j] + B.MatrixData[i][j];
            return X;
        }

        /// <summary>matrixData = matrixData + B</summary>
        /// <param name="B">another matrix</param>
        /// <returns>matrixData + B</returns>
        public Matrix PlusEquals(Matrix B)
        {
            CheckMatrixDimensions(B);
            for (var i = 0; i < Rows; i++)
            for (var j = 0; j < Columns; j++)
                MatrixData[i][j] = MatrixData[i][j] + B.MatrixData[i][j];
            return this;
        }

        /// <summary>C = matrixData - B</summary>
        /// <param name="B">another matrix</param>
        /// <returns>matrixData - B</returns>
        public Matrix Minus(Matrix B)
        {
            CheckMatrixDimensions(B);
            var X = new Matrix(Rows, Columns);
            var C = X.GetArray();
            for (var i = 0; i < Rows; i++)
            for (var j = 0; j < Columns; j++)
                C[i][j] = MatrixData[i][j] - B.MatrixData[i][j];
            return X;
        }

        /// <summary>matrixData = matrixData - B</summary>
        /// <param name="B">another matrix</param>
        /// <returns>matrixData - B</returns>
        public Matrix MinusEquals(Matrix B)
        {
            CheckMatrixDimensions(B);
            for (var i = 0; i < Rows; i++)
            for (var j = 0; j < Columns; j++)
                MatrixData[i][j] = MatrixData[i][j] - B.MatrixData[i][j];
            return this;
        }

        /// <summary>Element-by-element multiplication, C = matrixData.*B</summary>
        /// <param name="B">another matrix</param>
        /// <returns>matrixData.*B</returns>
        public Matrix ArrayTimes(Matrix B)
        {
            CheckMatrixDimensions(B);
            var X = new Matrix(Rows, Columns);
            var C = X.GetArray();
            for (var i = 0; i < Rows; i++)
            for (var j = 0; j < Columns; j++)
                C[i][j] = MatrixData[i][j] * B.MatrixData[i][j];
            return X;
        }

        /// <summary>Element-by-element multiplication in place, matrixData = matrixData.*B</summary>
        /// <param name="B">another matrix</param>
        /// <returns>matrixData.*B</returns>
        public Matrix ArrayTimesEquals(Matrix B)
        {
            CheckMatrixDimensions(B);
            for (var i = 0; i < Rows; i++)
            for (var j = 0; j < Columns; j++)
                MatrixData[i][j] = MatrixData[i][j] * B.MatrixData[i][j];
            return this;
        }

        /// <summary>Divide matrixData matrix by matrixData scalar, C = matrixData/s</summary>
        /// <param name="s">scalar</param>
        /// <returns>matrixData/s</returns>
        public Matrix Divide(double s)
        {
            if (s == 0) return this;

            var X = new Matrix(Rows, Columns);
            var C = X.GetArray();
            for (var i = 0; i < Rows; i++)
            for (var j = 0; j < Columns; j++)
                C[i][j] = MatrixData[i][j] / s;
            return X;
        }

        /// <summary>Element-by-element right division, C = matrixData./B</summary>
        /// <param name="B">another matrix</param>
        /// <returns>matrixData./B</returns>
        public Matrix ArrayRightDivide(Matrix B)
        {
            CheckMatrixDimensions(B);
            var X = new Matrix(Rows, Columns);
            var C = X.GetArray();
            for (var i = 0; i < Rows; i++)
            for (var j = 0; j < Columns; j++)
                C[i][j] = MatrixData[i][j] / B.MatrixData[i][j];
            return X;
        }

        /// <summary>Element-by-element right division in place, matrixData = matrixData./B</summary>
        /// <param name="B">another matrix</param>
        /// <returns>matrixData./B</returns>
        public Matrix ArrayRightDivideEquals(Matrix B)
        {
            CheckMatrixDimensions(B);
            for (var i = 0; i < Rows; i++)
            for (var j = 0; j < Columns; j++)
                MatrixData[i][j] = MatrixData[i][j] / B.MatrixData[i][j];
            return this;
        }

        /// <summary>Element-by-element left division, C = matrixData.\B</summary>
        /// <param name="B">another matrix</param>
        /// <returns>matrixData.\B</returns>
        public Matrix ArrayLeftDivide(Matrix B)
        {
            CheckMatrixDimensions(B);
            var X = new Matrix(Rows, Columns);
            var C = X.GetArray();
            for (var i = 0; i < Rows; i++)
            for (var j = 0; j < Columns; j++)
                C[i][j] = B.MatrixData[i][j] / MatrixData[i][j];
            return X;
        }

        /// <summary>Element-by-element left division in place, matrixData = matrixData.\B</summary>
        /// <param name="B">another matrix</param>
        /// <returns>matrixData.\B</returns>
        public Matrix ArrayLeftDivideEquals(Matrix B)
        {
            CheckMatrixDimensions(B);
            for (var i = 0; i < Rows; i++)
            for (var j = 0; j < Columns; j++)
                MatrixData[i][j] = B.MatrixData[i][j] / MatrixData[i][j];
            return this;
        }

        /// <summary>Multiply matrixData matrix by matrixData scalar, C = s*matrixData</summary>
        /// <param name="s">scalar</param>
        /// <returns>s*matrixData</returns>
        public Matrix Times(double s)
        {
            var X = new Matrix(Rows, Columns);
            var C = X.GetArray();
            for (var i = 0; i < Rows; i++)
            for (var j = 0; j < Columns; j++)
                C[i][j] = s * MatrixData[i][j];
            return X;
        }

        /// <summary>Multiply matrixData matrix by matrixData scalar, C = s*matrixData</summary>
        /// <param name="s">scalar</param>
        /// <returns>s*matrixData</returns>
        public Matrix Multiply(double s)
        {
            return Times(s);
        }

        /// <summary>Multiply matrixData matrix by matrixData scalar in place, matrixData = s*matrixData</summary>
        /// <param name="s">scalar</param>
        /// <returns>replace matrixData by s*matrixData</returns>
        public Matrix TimesEquals(double s)
        {
            for (var i = 0; i < Rows; i++)
            for (var j = 0; j < Columns; j++)
                MatrixData[i][j] = s * MatrixData[i][j];
            return this;
        }

        /// <summary>Linear algebraic matrix multiplication, matrixData * B</summary>
        /// <param name="B">another matrix</param>
        /// <returns>Matrix product, matrixData * B</returns>
        /// <exception cref="ArgumentException">Matrix inner dimensions must agree.</exception>
        public Matrix Times(Matrix B)
        {
            if (B.Rows != Columns) throw new ArgumentException("Matrix inner dimensions must agree.");
            var X = new Matrix(Rows, B.Columns);
            var C = X.GetArray();
            var Bcolj = new double[Columns];
            for (var j = 0; j < B.Columns; j++)
            {
                for (var k = 0; k < Columns; k++) Bcolj[k] = B.MatrixData[k][j];
                for (var i = 0; i < Rows; i++)
                {
                    var Arowi = MatrixData[i];
                    double s = 0;
                    for (var k = 0; k < Columns; k++) s += Arowi[k] * Bcolj[k];
                    C[i][j] = s;
                }
            }

            return X;
        }

        /// <summary>Linear algebraic matrix multiplication, matrixData * B</summary>
        /// <param name="B">another matrix</param>
        /// <returns>Matrix product, matrixData * B</returns>
        public Matrix Multiply(Matrix B)
        {
            return Times(B);
        }

        /// <summary>
        ///     Linear algebraic matrix multiplication, matrixData * B
        ///     B being matrixData triangular matrix
        ///     <b>Note:</b>
        ///     Actually the matrix should be matrixData
        ///     <b>
        ///         column oriented, upper triangular
        ///         matrix
        ///     </b>
        ///     but use the <b>row oriented, lower triangular matrix</b>
        ///     instead (transposed), because this is faster due to the easyer array
        ///     access.
        /// </summary>
        /// <param name="B">another matrix</param>
        /// <returns>Matrix product, matrixData * B</returns>
        /// <exception cref="ArgumentException">Matrix inner dimensions must agree.</exception>
        public Matrix TimesTriangular(Matrix B)
        {
            if (B.Rows != Columns)
                throw new ArgumentException("Matrix inner dimensions must agree.");

            var X = new Matrix(Rows, B.Columns);
            var c = X.GetArray();
            double[][] b;
            double s = 0;
            double[] Arowi;
            double[] Browj;

            b = B.GetArray();
            // multiply with each row of matrixData
            for (var i = 0; i < Rows; i++)
            {
                Arowi = MatrixData[i];

                // for all columns of B
                for (var j = 0; j < B.Columns; j++)
                {
                    s = 0;
                    Browj = b[j];

                    // since B being triangular, this loop uses k <= j
                    for (var k = 0; k <= j; k++) s += Arowi[k] * Browj[k];
                    c[i][j] = s;
                }
            }

            return X;
        }

        /// <summary>
        ///     X.diffEquals() calculates differences between adjacent columns of this
        ///     matrix. Consequently the size of the matrix is reduced by one. The result
        ///     is stored in this matrix object again.
        /// </summary>
        public void DiffEquals()
        {
            double[] col = null;
            for (var i = 0; i < MatrixData.Length; i++)
            {
                col = new double[MatrixData[i].Length - 1];

                for (var j = 1; j < MatrixData[i].Length; j++)
                    col[j - 1] = Math.Abs(MatrixData[i][j] - MatrixData[i][j - 1]);

                MatrixData[i] = col;
            }

            Columns--;
        }

        /// <summary>
        ///     X.logEquals() calculates the natural logarithem of each element of the
        ///     matrix. The result is stored in this matrix object again.
        /// </summary>
        public void LogEquals()
        {
            for (var i = 0; i < MatrixData.Length; i++)
            for (var j = 0; j < MatrixData[i].Length; j++)
                MatrixData[i][j] = Math.Log(MatrixData[i][j]);
        }

        /// <summary>
        ///     X.powEquals() calculates the power of each element of the matrix.
        ///     The result is stored in this matrix object again.
        /// </summary>
        /// <param name="exp"></param>
        public void PowEquals(double exp)
        {
            for (var i = 0; i < MatrixData.Length; i++)
            for (var j = 0; j < MatrixData[i].Length; j++)
                MatrixData[i][j] = Math.Pow(MatrixData[i][j], exp);
        }

        /// <summary>
        ///     X.powEquals() calculates the power of each element of the matrix.
        /// </summary>
        /// <param name="exp"></param>
        /// <returns>Matrix</returns>
        public Matrix Pow(double exp)
        {
            var X = new Matrix(Rows, Columns);
            var C = X.GetArray();

            for (var i = 0; i < Rows; i++)
            for (var j = 0; j < Columns; j++)
                C[i][j] = Math.Pow(MatrixData[i][j], exp);

            return X;
        }

        /// <summary>
        ///     X.thrunkAtLowerBoundariy(). All values smaller than the given one are set
        ///     to this lower boundary.
        /// </summary>
        /// <param name="value">Lower boundary value</param>
        public void ThrunkAtLowerBoundary(double value)
        {
            for (var i = 0; i < MatrixData.Length; i++)
            for (var j = 0; j < MatrixData[i].Length; j++)
                if (MatrixData[i][j] < value)
                    MatrixData[i][j] = value;
        }

        /// <summary>LU Decomposition</summary>
        /// <returns>LUDecomposition</returns>
        /// <seealso cref="LUDecomposition">LUDecomposition</seealso>
        public LUDecomposition LU()
        {
            return new LUDecomposition(this);
        }

        /// <summary>QR Decomposition</summary>
        /// <returns>QRDecomposition</returns>
        /// <seealso cref="QRDecomposition">QRDecomposition</seealso>
        public QRDecomposition QR()
        {
            return new QRDecomposition(this);
        }

        /// <summary>Cholesky Decomposition</summary>
        /// <returns>CholeskyDecomposition</returns>
        /// <seealso cref="CholeskyDecomposition">CholeskyDecomposition</seealso>
        public CholeskyDecomposition Chol()
        {
            return new CholeskyDecomposition(this);
        }

        /// <summary>Singular Value Decomposition</summary>
        /// <returns>SingularValueDecomposition</returns>
        /// <seealso cref="SingularValueDecomposition">SingularValueDecomposition</seealso>
        public SingularValueDecomposition Svd()
        {
            return new SingularValueDecomposition(this);
        }

        /// <summary>Eigenvalue Decomposition</summary>
        /// <returns>EigenvalueDecomposition</returns>
        /// <seealso cref="EigenvalueDecomposition">EigenvalueDecomposition</seealso>
        public EigenvalueDecomposition Eig()
        {
            return new EigenvalueDecomposition(this);
        }

        /// <summary>Solve matrixData*X = B</summary>
        /// <param name="B">right hand side</param>
        /// <returns>solution if matrixData is square, least squares solution otherwise</returns>
        public Matrix Solve(Matrix B)
        {
            return Rows == Columns ? new LUDecomposition(this).Solve(B) : new QRDecomposition(this).Solve(B);
        }

        /// <summary>Solve X*matrixData = B, which is also matrixData'*X' = B'</summary>
        /// <param name="B">right hand side</param>
        /// <returns>solution if matrixData is square, least squares solution otherwise.</returns>
        public Matrix SolveTranspose(Matrix B)
        {
            return Transpose().Solve(B.Transpose());
        }

        /// <summary>Matrix inverse or pseudoinverse</summary>
        /// <returns>inverse(matrixData) if matrixData is square, pseudoinverse otherwise.</returns>
        public Matrix Inverse()
        {
            return Solve(Identity(Rows, Rows));
        }

        /// <summary>Matrix determinant</summary>
        /// <returns>determinant</returns>
        public double Det()
        {
            return new LUDecomposition(this).Det();
        }

        /// <summary>Matrix rank</summary>
        /// <returns>effective numerical rank, obtained from SVD.</returns>
        public int Rank()
        {
            return new SingularValueDecomposition(this).Rank();
        }

        /// <summary>Matrix condition (2 norm)</summary>
        /// <returns>ratio of largest to smallest singular value.</returns>
        public double Cond()
        {
            return new SingularValueDecomposition(this).Cond();
        }

        /// <summary>Matrix trace.</summary>
        /// <returns>sum of the diagonal elements.</returns>
        public double Trace()
        {
            double t = 0;
            for (var i = 0; i < Math.Min(Rows, Columns); i++) t += MatrixData[i][i];
            return t;
        }

        /// <summary>
        ///     Generate matrix with random elements
        /// </summary>
        /// <param name="rows">Number of rows.</param>
        /// <param name="columns">Number of colums.</param>
        /// <param name="minVal">minimum random value</param>
        /// <param name="maxVal">maximum random value</param>
        /// <returns>An rows-by-columns matrix with uniformly distributed random elements.</returns>
        /// <remarks>Developed by James McCaffrey</remarks>
        /// <see cref="http://msdn.microsoft.com/en-us/magazine/jj863137.aspx">Matrix Decomposition</see>
        public static Matrix Random(int rows, int columns, double minVal, double maxVal)
        {
            // return matrix with values between minVal and maxVal
            var result = new Matrix(rows, columns);
            for (var i = 0; i < rows; ++i)
            for (var j = 0; j < columns; ++j)
                result.MatrixData[i][j] = (maxVal - minVal) * random.NextDouble() + minVal;
            return result;
        }

        /// <summary>Generate matrix with random elements</summary>
        /// <param name="rows">Number of rows.</param>
        /// <param name="columns">Number of colums.</param>
        /// <returns>An rows-by-columns matrix with uniformly distributed random elements.</returns>
        public static Matrix Random(int rows, int columns)
        {
            return Random(rows, columns, -1.0, 1.0);
        }

        /// <summary>Generate identity matrix</summary>
        /// <param name="rows">Number of rows.</param>
        /// <param name="columns">Number of colums.</param>
        /// <returns>An rows-by-columns matrix with ones on the diagonal and zeros elsewhere.</returns>
        public static Matrix Identity(int rows, int columns)
        {
            var matrixData = new Matrix(rows, columns);
            var X = matrixData.GetArray();
            for (var i = 0; i < rows; i++)
            for (var j = 0; j < columns; j++)
                X[i][j] = i == j ? 1.0 : 0.0;
            return matrixData;
        }

        /// <summary>Returns the mean values along the specified dimension.</summary>
        /// <param name="dim">
        ///     If 1, then the mean of each column is returned in matrixData row
        ///     vector. If 2, then the mean of each row is returned in matrixData
        ///     column vector.
        /// </param>
        /// <returns>matrixData vector containing the mean values along the specified dimension.</returns>
        public Matrix Mean(int dim)
        {
            Matrix result;
            switch (dim)
            {
                case 1:
                    result = new Matrix(1, Columns);
                    for (var currN = 0; currN < Columns; currN++)
                    {
                        for (var currM = 0; currM < Rows; currM++)
                            result.MatrixData[0][currN] += MatrixData[currM][currN];
                        result.MatrixData[0][currN] /= Rows;
                    }

                    return result;
                case 2:
                    result = new Matrix(Rows, 1);
                    for (var currM = 0; currM < Rows; currM++)
                    {
                        for (var currN = 0; currN < Columns; currN++)
                            result.MatrixData[currM][0] += MatrixData[currM][currN];
                        result.MatrixData[currM][0] /= Columns;
                    }

                    return result;
                default:
                    new ArgumentException("dim must be either 1 or 2, and not: " + dim);
                    return null;
            }
        }

        /// <summary>Calculate the full covariance matrix.</summary>
        /// <returns>the covariance matrix</returns>
        public Matrix Cov()
        {
            var transe = Transpose();
            var result = new Matrix(transe.Rows, transe.Rows);
            for (var currM = 0; currM < transe.Rows; currM++)
            for (var currN = currM; currN < transe.Rows; currN++)
            {
                var covMN = Cov(transe.MatrixData[currM], transe.MatrixData[currN]);
                result.MatrixData[currM][currN] = covMN;
                result.MatrixData[currN][currM] = covMN;
            }

            return result;
        }

        /// <summary>
        ///     Covariance Methods copied from the Mirage matrix class
        /// </summary>
        /// <param name="mean">Column Vector Matrix with the mean values - e.g. mfcc.Mean(2);</param>
        /// <returns>Covariance Matrix</returns>
        public Matrix Cov(Matrix mean)
        {
            var cache = new Matrix(Rows, Columns);
            var factor = 1.0f / (Columns - 1);
            for (var j = 0; j < Rows; j++)
            for (var i = 0; i < Columns; i++)
                cache.MatrixData[j][i] = MatrixData[j][i] - mean.MatrixData[j][0];

            var cov = new Matrix(mean.Rows, mean.Rows);
            for (var i = 0; i < cov.Rows; i++)
            for (var j = 0; j <= i; j++)
            {
                var sum = 0.0;
                for (var k = 0; k < Columns; k++) sum += cache.MatrixData[i][k] * cache.MatrixData[j][k];
                sum *= factor;
                cov.MatrixData[i][j] = sum;
                if (i == j) continue;
                cov.MatrixData[j][i] = sum;
            }

            return cov;
        }

        /// <summary>
        ///     Gauss-Jordan routine to invert a matrix, decimal precision
        /// </summary>
        /// <returns>Matrix</returns>
        public Matrix InverseGausJordan()
        {
            var e = new decimal[Rows + 1, Columns + 1];
            for (var i = 1; i <= Rows; i++) e[i, i] = 1;
            var m = new decimal[Rows + 1, Columns + 1];
            for (var i = 1; i <= Rows; i++)
            for (var j = 1; j <= Columns; j++)
                if (!double.IsNaN(MatrixData[i - 1][j - 1]))
                    m[i, j] = (decimal)MatrixData[i - 1][j - 1];

            GaussJordan(ref m, Rows, ref e, Rows);
            var inv = new Matrix(Rows, Columns);

            for (var i = 1; i <= Rows; i++)
            for (var j = 1; j <= Columns; j++)
                inv.MatrixData[i - 1][j - 1] = (double)m[i, j];

            return inv;
        }

        /// <summary>
        ///     GaussJordan routine to invert a matrix, decimal precision
        /// </summary>
        /// <param name="a"></param>
        /// <param name="n"></param>
        /// <param name="b"></param>
        /// <param name="m"></param>
        private void GaussJordan(ref decimal[,] a, int n, ref decimal[,] b, int m)
        {
            var indxc = new int[n + 1];
            var indxr = new int[n + 1];
            var ipiv = new int[n + 1];
            int i, icol = 0, irow = 0, j, k, l, ll;
            decimal big, dum, pivinv, temp;

            for (j = 1; j <= n; j++) ipiv[j] = 0;

            for (i = 1; i <= n; i++)
            {
                big = 0;
                for (j = 1; j <= n; j++)
                    if (ipiv[j] != 1)
                        for (k = 1; k <= n; k++)
                            if (ipiv[k] == 0)
                            {
                                if (Math.Abs(a[j, k]) >= big)
                                {
                                    big = Math.Abs(a[j, k]);
                                    irow = j;
                                    icol = k;
                                }
                            }
                            else if (ipiv[k] > 1)
                            {
                                throw new Exception("Mirage - Gauss/Jordan Singular Matrix (1)");
                            }

                ipiv[icol]++;
                if (irow != icol)
                {
                    for (l = 1; l <= n; l++)
                    {
                        temp = a[irow, l];
                        a[irow, l] = a[icol, l];
                        a[icol, l] = temp;
                    }

                    for (l = 1; l <= m; l++)
                    {
                        temp = b[irow, l];
                        b[irow, l] = b[icol, l];
                        b[icol, l] = temp;
                    }
                }

                indxr[i] = irow;
                indxc[i] = icol;
                if (a[icol, icol] == 0) throw new Exception("Mirage - Gauss/Jordan Singular Matrix (2)");

                pivinv = 1 / a[icol, icol];
                a[icol, icol] = 1;

                for (l = 1; l <= n; l++) a[icol, l] *= pivinv;

                for (l = 1; l <= m; l++) b[icol, l] *= pivinv;

                for (ll = 1; ll <= n; ll++)
                    if (ll != icol)
                    {
                        dum = a[ll, icol];
                        a[ll, icol] = 0;

                        for (l = 1; l <= n; l++) a[ll, l] -= a[icol, l] * dum;

                        for (l = 1; l <= m; l++) b[ll, l] -= b[icol, l] * dum;
                    }
            }

            for (l = n; l >= 1; l--)
                if (indxr[l] != indxc[l])
                    for (k = 1; k <= n; k++)
                    {
                        temp = a[k, indxr[l]];
                        a[k, indxr[l]] = a[k, indxc[l]];
                        a[k, indxc[l]] = temp;
                    }
        }

        /// <summary>Calculate the covariance between the two vectors.</summary>
        /// <param name="vec1">double values</param>
        /// <param name="vec2">double values</param>
        /// <returns>the covariance between the two vectors.</returns>
        private double Cov(double[] vec1, double[] vec2)
        {
            double result = 0;
            var dim = vec1.Length;
            if (vec2.Length != dim)
                new ArgumentException("vectors are not of same length");
            double meanVec1 = Mean(vec1), meanVec2 = Mean(vec2);
            for (var i = 0; i < dim; i++) result += (vec1[i] - meanVec1) * (vec2[i] - meanVec2);
            return result / Math.Max(1, dim - 1);

            // int dim = vec1.Length;
            // if(vec2.Length != dim)
            //  (new ArgumentException("vectors are not of same length")).printStackTrace();
            // var times = new double[dim];
            // for(int i=0; i<dim; i++)
            //   times[i] += vec1[i]*vec2[i];
            // return mean(times) - mean(vec1)*mean(vec2);
        }

        /// <summary>The mean of the values in the double array</summary>
        /// <param name="vec">double values</param>
        /// <returns>the mean of the values in vec</returns>
        private double Mean(double[] vec)
        {
            double result = 0;
            for (var i = 0; i < vec.Length; i++)
                result += vec[i];
            return result / vec.Length;
        }

        /// <summary>Returns the sum of the component of the matrix.</summary>
        /// <returns>the sum</returns>
        public double Sum()
        {
            double result = 0;
            foreach (var dArr in MatrixData)
            foreach (var d in dArr)
                result += d;
            return result;
        }

        /// <summary>returns matrixData new Matrix object, where each value is set to the absolute value</summary>
        /// <returns>matrixData new Matrix with all values being positive</returns>
        public Matrix Abs()
        {
            var result = new Matrix(Rows, Columns); // don't use clone(), as the values are assigned in the loop.
            for (var i = 0; i < result.MatrixData.Length; i++)
            for (var j = 0; j < result.MatrixData[i].Length; j++)
                result.MatrixData[i][j] = Math.Abs(MatrixData[i][j]);
            return result;
        }

        /// <summary>
        ///     Checks if number of rows equals number of columns.
        /// </summary>
        /// <returns>True iff matrix is n by n.</returns>
        public bool IsSquare()
        {
            return Columns == Rows;
        }

        /// <summary>
        ///     Checks if A[i, j] == A[j, i].
        /// </summary>
        /// <returns>True iff matrix is symmetric.</returns>
        public bool IsSymmetric()
        {
            for (var i = 0; i < Rows; i++)
            for (var j = 0; j < Columns; j++)
                if (MatrixData[i][j] != MatrixData[j][i])
                    return false;
            return true;
        }

        #endregion

        #region Optimized Matrix Multiplication Methods

        /// <summary>
        ///     Multiply two square matrices
        ///     Optimized using The Task Parallel Library (TPL) in the System.Threading.Tasks namespace in the .NET Framework 4 and
        ///     later.
        /// </summary>
        /// <param name="matrixA">Matrix A</param>
        /// <param name="matrixB">Matrix B</param>
        /// <returns>The dot product</returns>
        /// <remarks>Developed by James McCaffrey</remarks>
        /// <see cref="http://msdn.microsoft.com/en-us/magazine/jj863137.aspx">Matrix Decomposition</see>
        public static double[][] MatrixProductParallel(double[][] matrixA, double[][] matrixB)
        {
            var aRows = matrixA.Length;
            var aCols = matrixA[0].Length;
            var bRows = matrixB.Length;
            var bCols = matrixB[0].Length;
            if (aCols != bRows)
                throw new Exception("Non-conformable matrices in MatrixProduct");

            var result = new Matrix(aRows, bCols);

            /*
            for (int i = 0; i < aRows; ++i) // each row of A
                for (int j = 0; j < bCols; ++j) // each col of B
                    for (int k = 0; k < aCols; ++k) // could use k < bRows
                        result.MatrixData[i][j] += matrixA[i][k] * matrixB[k][j];
             */

            Parallel.For(0, aRows, i =>
                {
                    for (var j = 0; j < bCols; ++j) // each col of B
                    for (var k = 0; k < aCols; ++k) // could use k < bRows
                        result.MatrixData[i][j] += matrixA[i][k] * matrixB[k][j];
                }
            );

            return result.MatrixData;
        }

        /// <summary>
        ///     Multiply two square matrices
        ///     Optimized using The Task Parallel Library (TPL) in the System.Threading.Tasks namespace in the .NET Framework 4 and
        ///     later.
        /// </summary>
        /// <param name="matrixA">Matrix A</param>
        /// <param name="matrixB">Matrix B</param>
        /// <returns>The dot product</returns>
        /// <remarks>Developed by James McCaffrey</remarks>
        /// <see cref="http://msdn.microsoft.com/en-us/magazine/jj863137.aspx">Matrix Decomposition</see>
        public static Matrix MatrixProductParallel(Matrix matrixA, Matrix matrixB)
        {
            var aRows = matrixA.MatrixData.Length;
            var aCols = matrixA.MatrixData[0].Length;
            var bRows = matrixB.MatrixData.Length;
            var bCols = matrixB.MatrixData[0].Length;
            if (aCols != bRows)
                throw new Exception("Non-conformable matrices in MatrixProduct");

            var result = new Matrix(aRows, bCols);

            /*
            for (int i = 0; i < aRows; ++i) // each row of A
                for (int j = 0; j < bCols; ++j) // each col of B
                    for (int k = 0; k < aCols; ++k) // could use k < bRows
                        result.MatrixData[i][j] += matrixA.MatrixData[i][k] * matrixB.MatrixData[k][j];
             */

            Parallel.For(0, aRows, i =>
                {
                    for (var j = 0; j < bCols; ++j) // each col of B
                    for (var k = 0; k < aCols; ++k) // could use k < bRows
                        result.MatrixData[i][j] += matrixA.MatrixData[i][k] * matrixB.MatrixData[k][j];
                }
            );

            return result;
        }

        /// <summary>
        ///     Optimized inline multplication of a square matrix: C = A * B.
        ///     Parallel optimization using PLINQ can be turned on, which gives us an easy way to perform parallel tasks.
        /// </summary>
        /// <param name="matrixA">Matrix A</param>
        /// <param name="matrixB">Matrix B</param>
        /// <param name="doParallel">Whether to use parallel processing (Note! This is much slower when running in STAThread mode!)</param>
        /// <returns>The dot product</returns>
        /// <remarks>Developed by Ron Whittle</remarks>
        /// <see cref="http://www.daniweb.com/software-development/csharp/code/355645/optimizing-matrix-multiplication">
        ///     Optimizing
        ///     Matrix Multiplication
        /// </see>
        public static Matrix MatrixProductFast(Matrix matrixA, Matrix matrixB, bool doParallel = false)
        {
            var aRows = matrixA.MatrixData.Length;
            var aCols = matrixA.MatrixData[0].Length;
            var bRows = matrixB.MatrixData.Length;
            var bCols = matrixB.MatrixData[0].Length;
            if (aCols != bRows)
                throw new Exception("Non-conformable matrices in MatrixProduct");

            var matrixC = new Matrix(aRows, bCols);
            MatrixProductFast(aRows, matrixA.MatrixData, matrixB.MatrixData, matrixC.MatrixData, doParallel);

            return matrixC;
        }

        /// <summary>
        ///     Optimized inline multplication of a square matrix: C = A * B.
        ///     Parallel optimization using PLINQ can be turned on, which gives us an easy way to perform parallel tasks.
        /// </summary>
        /// <param name="N">Number of Rows and Columns (identical since the matrix has to be square and of similar size)</param>
        /// <param name="A">Matrix A</param>
        /// <param name="B">Matrix B</param>
        /// <param name="C">Resulting Matrix C. Array C must be fully allocated or you'll get a null reference exception</param>
        /// <param name="doParallel">Whether to use parallel processing (Note! This is much slower when running in STAThread mode!)</param>
        /// <remarks>Developed by Ron Whittle</remarks>
        /// <see cref="http://www.daniweb.com/software-development/csharp/code/355645/optimizing-matrix-multiplication">
        ///     Optimizing
        ///     Matrix Multiplication
        /// </see>
        private static void MatrixProductFast(int N, double[][] A, double[][] B, double[][] C, bool doParallel)
        {
            if (doParallel)
            {
                var source = Enumerable.Range(0, N);
                var pquery = from num in source.AsParallel()
                    select num;
                pquery.ForAll(i => MatrixProductFast(N, A, B, C, i));
            }
            else
            {
                for (var i = 0; i < N; i++) MatrixProductFast(N, A, B, C, i);
            }
        }

        /// <summary>
        ///     Optimized inline multplication of a square matrix: C = A * B.
        ///     Optimized using  PLINQ, which gives us an easy way to perform parallel tasks.
        /// </summary>
        /// <param name="N">Number of Rows and Columns (identical since the matrix has to be square and of similar size)</param>
        /// <param name="A">Matrix A</param>
        /// <param name="B">Matrix B</param>
        /// <param name="C">Resulting Matrix C. Array C must be fully allocated or you'll get a null reference exception</param>
        /// <param name="i">Parameter to the method and calculate multiple rows at a time</param>
        /// <remarks>Developed by Ron Whittle</remarks>
        /// <see cref="http://www.daniweb.com/software-development/csharp/code/355645/optimizing-matrix-multiplication">
        ///     Optimizing
        ///     Matrix Multiplication
        /// </see>
        /// <seealso cref="http://www.heatonresearch.com/content/choosing-best-c-array-type-matrix-multiplication">
        ///     Optimizing
        ///     Matrix Multiplication Conclusion
        /// </seealso>
        private static void MatrixProductFast(int N, double[][] A, double[][] B, double[][] C, int i)
        {
            // This leads into the best part about using jagged arrays.
            // With the current code every access to an element is a double index.
            // First to the row, then to the column.
            // By introducing some extra variables we can optimize this to a single index in each loop
            // 
            // Un-optimized loop:
            //	for (int i = 0; i < N; i++) {
            // 		for (int j = 0; j < N; j++) {
            //			for (int k = 0; k < N; k++) {
            //				C[i][j] += A[i][k] * B[k][j];
            //			}
            //		}
            //	}

            // By taking one of the indexes out of the loop,
            // we can use it as a parameter to the method and calculate multiple rows at a time
            var iRowA = A[i];
            var iRowC = C[i];
            for (var k = 0; k < N; k++)
            {
                var kRowB = B[k];
                var ikA = iRowA[k];
                for (var j = 0; j < N; j++) iRowC[j] += ikA * kRowB[j];
            }
        }

        #endregion

        #region Print

        /// <summary>
        ///     Print the matrix to stdout. Line the elements up in columns
        ///     with matrixData Fortran-like 'Fw.d' style format.
        /// </summary>
        public void Print()
        {
            Print(Console.Out);
        }

        /// <summary>
        ///     Print the matrix to stdout. Line the elements up in columns
        ///     with matrixData Fortran-like 'Fw.d' style format.
        /// </summary>
        /// <param name="w">Column width.</param>
        /// <param name="d">Number of digits after the decimal.</param>
        public void Print(int w, int d)
        {
            Print(Console.Out, w, d);
        }

        /// <summary>
        ///     Print the matrix to the output stream. Line the elements up in
        ///     columns with matrixData Fortran-like 'Fw.d' style format.
        /// </summary>
        /// <param name="output">Output stream.</param>
        public void Print(TextWriter output)
        {
            Print(output, 18, 7);
        }

        /// <summary>
        ///     Print the matrix to the output stream. Line the elements up in
        ///     columns with matrixData Fortran-like 'Fw.d' style format.
        /// </summary>
        /// <param name="output">Output stream.</param>
        /// <param name="w">Column width.</param>
        /// <param name="d">Number of digits after the decimal.</param>
        public void Print(TextWriter output, int w, int d)
        {
            var format = new CultureInfo("en-US", false).NumberFormat;
            format.NumberDecimalDigits = d;
            Print(output, format, w);
            output.Flush();
        }

        /// <summary>
        ///     Print the matrix to the output stream. Line the elements up in columns.
        ///     Use the format object, and right justify within columns of width
        ///     characters.
        ///     Note that is the matrix is to be read back in, you probably will want
        ///     to use matrixData NumberFormat that is set to US Locale.
        /// </summary>
        /// <param name="output">the output stream.</param>
        /// <param name="format">matrixData formatting object to format the matrix elements</param>
        /// <param name="width">Column width.</param>
        /// <seealso cref="NumberFormatInfo">NumberFormatInfo</seealso>
        public void Print(TextWriter output, IFormatProvider format, int width)
        {
            output.WriteLine(); // start on new line.
            for (var i = 0; i < Rows; i++)
            {
                for (var j = 0; j < Columns; j++)
                {
                    //string s = matrixData[i][j].ToString("F", format);
                    // round to better printable precision
                    var d = (decimal)MatrixData[i][j];
                    var rounded = Math.Round(d, ((NumberFormatInfo)format).NumberDecimalDigits);
                    var s = rounded.ToString("G29", format);
                    output.Write(s.PadRight(width));
                }

                output.WriteLine();
            }

            output.WriteLine(); // end with blank line.
        }

        #endregion

        #region ReadWrite Methods

        /// <summary>
        ///     Write XML to Text Writer
        /// </summary>
        /// <param name="textWriter"></param>
        /// <example>
        ///     mfccs.Write(File.CreateText("mfccs.xml"));
        /// </example>
        public void Write(TextWriter textWriter)
        {
            var xmlTextWriter = new XmlTextWriter(textWriter);
            xmlTextWriter.Formatting = Formatting.Indented;
            xmlTextWriter.Indentation = 4;
            WriteXML(xmlTextWriter, null);
            xmlTextWriter.Close();
        }

        /// <summary>
        ///     Read XML from Text Reader
        /// </summary>
        /// <param name="textReader"></param>
        /// <example>
        ///     mfccs.Read(new StreamReader("mfccs.xml"));
        /// </example>
        public void Read(TextReader textReader)
        {
            var xmlTextReader = new XmlTextReader(textReader);
            ReadXML(XDocument.Load(xmlTextReader), null);
            xmlTextReader.Close();
        }

        /// <summary>
        ///     Writes the xml representation of this object to the xml text writer.
        ///     There is the convention, that each call to matrixData <code>WriteXML()</code> method
        ///     results in one xml element in the output stream.
        ///     Note! It is the callers responisbility to flush the stream.
        /// </summary>
        /// <param name="xmlWriter">XmlTextWriter the xml output stream</param>
        /// <param name="matrixName">The name to use for this matrix</param>
        /// <example>
        ///     mfccs.WriteXML(new XmlTextWriter("mfccs.xml", null));
        /// </example>
        public void WriteXML(XmlWriter xmlWriter, string matrixName)
        {
            xmlWriter.WriteStartElement("matrix");
            xmlWriter.WriteAttributeString("rows", Rows.ToString());
            xmlWriter.WriteAttributeString("cols", Columns.ToString());
            xmlWriter.WriteAttributeString("name", matrixName);

            for (var i = 0; i < Rows; i++)
            {
                xmlWriter.WriteStartElement("matrixrow");
                for (var j = 0; j < Columns; j++)
                {
                    xmlWriter.WriteStartElement("cn");
                    //xmlWriter.WriteAttributeString("type","IEEE-754");
                    xmlWriter.WriteString(MatrixData[i][j].ToString());
                    xmlWriter.WriteEndElement();
                }

                xmlWriter.WriteEndElement();
            }

            xmlWriter.WriteEndElement();
        }

        /// <summary>
        ///     Reads the xml representation of an object form the xml text reader.
        /// </summary>
        /// <param name="xdoc">XDocument the xml input stream</param>
        /// <param name="matrixName">The name to use for this matrix</param>
        /// <example>
        ///     mfccs.ReadXML(new XDocument("mfccs.xml", "matrix"));
        /// </example>
        public void ReadXML(XDocument xdoc, string matrixName)
        {
            // TODO: XElement crashes SharpDevelop in Debug mode when you want to see variables in the IDE
#if !DEBUG
			XElement dimensions = null;
			if (matrixName != null) {
				// look up by attribute name
				dimensions = (from x in xdoc.Descendants("matrix")
				              where x.Attribute("name").Value == matrixName
				              select x).FirstOrDefault();
			} else {
				dimensions = xdoc.Element("matrix");
			}
			
			string srows = dimensions.Attribute("rows").Value;
			string scols = dimensions.Attribute("cols").Value;
			int rows = int.Parse(srows);
			int columns = int.Parse(scols);

			var matrixrows = from row in dimensions.Descendants("matrixrow")
				select new {
				Children = row.Descendants("cn")
			};
			
			if (rows != matrixrows.Count() || columns != matrixrows.FirstOrDefault().Children.Count()) {
				// Dimension errors
				throw new ArgumentException("Matrix dimensions must agree.");
			} else {
				this.rowCount = rows;
				this.columnCount = columns;
			}
			
			this.matrixData = new double[rows][];

			int i = 0, j = 0;
			foreach (var matrixrow in matrixrows) {
				this.matrixData[i] = new double[columns];
				j = 0;
				foreach(var cn in matrixrow.Children) {
					string val = cn.Value;
					this.matrixData[i][j] = double.Parse(val);
					j++;
				}
				i++;
			}
#endif
        }

        /// <summary>
        ///     Writes the Matrix to a comma separated file that can be read by Excel.
        ///     <param name="filename">the name of the csv file to create, e.g. "C:\\temp\\matrix.csv"</param>
        ///     <param name="columnSeparator">the separator character to use</param>
        public void WriteCSV(string filename, string columnSeparator)
        {
            TextWriter pw = File.CreateText(filename);
            for (var i = 0; i < Rows; i++)
            {
                for (var j = 0; j < Columns; j++) pw.Write("\"{0}\"{1}", MatrixData[i][j].ToString(), columnSeparator);
                pw.Write("\r");
            }

            pw.Close();
        }

        /// <summary>
        ///     Writes the Matrix to an ascii-textfile that can be read by Matlab.
        ///     Usage in Matlab: load('filename', '-ascii');
        /// </summary>
        /// <param name="filename">the name of the ascii file to create, e.g. "C:\\temp\\matrix.ascii"</param>
        public void WriteAscii(string filename)
        {
            TextWriter pw = File.CreateText(filename);
            for (var i = 0; i < Rows; i++)
            {
                for (var j = 0; j < Columns; j++)
                    pw.Write(" {0}", MatrixData[i][j].ToString("#.00000000e+000", CultureInfo.InvariantCulture));
                pw.Write("\r");
            }

            pw.Close();
        }

        /// <summary>
        ///     Write matrix to file using F3 formatting
        /// </summary>
        /// <param name="filename">filename</param>
        public void WriteF3Formatted(string filename)
        {
            TextWriter pw = File.CreateText(filename);
            for (var i = 0; i < Rows; i++)
            {
                for (var j = 0; j < Columns; j++)
                    pw.Write("{0}", MatrixData[i][j].ToString("F3", CultureInfo.InvariantCulture).PadLeft(10) + " ");
                pw.Write("\r");
            }

            pw.Close();
        }

        /// <summary>
        ///     Write matrix to file
        /// </summary>
        /// <param name="filename">filename</param>
        public void WriteText(string filename)
        {
            TextWriter pw = File.CreateText(filename);
            Print(pw);
            pw.Close();
        }

        /// <summary>
        ///     Write Matrix to a binary file
        /// </summary>
        /// <param name="filename">filename</param>
        public void Write(string filename)
        {
            WriteBinary(filename);
        }

        /// <summary>
        ///     Write Matrix to a binary file
        /// </summary>
        /// <param name="filename">filename</param>
        public void WriteBinary(string filename)
        {
            WriteBinary(File.Open(filename, FileMode.Create));
        }

        /// <summary>
        ///     Write Matrix to a stream
        /// </summary>
        /// <param name="filestream">filestream</param>
        public void WriteBinary(Stream filestream)
        {
            var binWriter = new BinaryWriter(filestream);
            binWriter.Write(Rows);
            binWriter.Write(Columns);

            for (var i = 0; i < Rows; i++)
            for (var j = 0; j < Columns; j++)
                binWriter.Write((float)MatrixData[i][j]);
        }

        /// <summary>
        ///     Load a Matrix from a binary representation stored in a file
        /// </summary>
        /// <param name="filename">filename</param>
        /// <returns>a Matrix</returns>
        public static Matrix Load(string filename)
        {
            return LoadBinary(filename);
        }

        /// <summary>
        ///     Load a Matrix from a binary representation stored in a file
        /// </summary>
        /// <param name="filename">filename</param>
        /// <returns>a Matrix</returns>
        public static Matrix LoadBinary(string filename)
        {
            return LoadBinary(new FileStream(filename, FileMode.Open));
        }

        /// <summary>
        ///     Load a Matrix from a binary representation
        /// </summary>
        /// <param name="filestream">filestream</param>
        /// <returns>a Matrix</returns>
        /// <example>dct = Matrix.LoadBinary(new FileStream("Mirage/Resources/dct.filter", FileMode.Open));</example>
        public static Matrix LoadBinary(Stream filestream)
        {
            using (var binReader = new BinaryReader(filestream))
            {
                var rows = binReader.ReadInt32();
                var columns = binReader.ReadInt32();
                var m = new Matrix(rows, columns);

                for (var i = 0; i < rows; i++)
                for (var j = 0; j < columns; j++)
                    m.MatrixData[i][j] = binReader.ReadSingle();
                return m;
            }
        }

        #endregion

        #region Draw Methods

        /// <summary>
        ///     Draw the matrix as a image graph
        ///     Imitating Matlabs plot(M), where M is the matrix
        /// </summary>
        /// <param name="fileName">filename</param>
        public void DrawMatrixGraph(string fileName, bool forceUseRows = false)
        {
            GraphPane myPane;
            var rect = new RectangleF(0, 0, 1200, 600);

            var ppl = new PointPairList();
            if (Columns == 1)
            {
                myPane = new GraphPane(rect, "Matrix", "Rows", "Value");
                for (var i = 0; i < Rows; i++) ppl.Add(i, MatrixData[i][0]);
                var myCurve = myPane.AddCurve("", ppl.Clone(), Color.Black, SymbolType.None);
            }
            else if (Rows == 1)
            {
                myPane = new GraphPane(rect, "Matrix", "Columns", "Value");
                for (var i = 0; i < Columns; i++) ppl.Add(i, MatrixData[0][i]);
                var myCurve = myPane.AddCurve("", ppl.Clone(), Color.Black, SymbolType.None);
            }
            else if (!forceUseRows && Columns > Rows)
            {
                myPane = new GraphPane(rect, "Matrix", "Columns", "Value");
                for (var i = 0; i < Rows; i++)
                {
                    ppl.Clear();
                    for (var j = 0; j < Columns; j++) ppl.Add(j, MatrixData[i][j]);
                    var color = ColorUtils.MatlabGraphColor(i);
                    var myCurve = myPane.AddCurve("", ppl.Clone(), color, SymbolType.None);
                }
            }
            else
            {
                // (columns < rows)
                myPane = new GraphPane(rect, "Matrix", "Rows", "Value");
                for (var j = 0; j < Columns; j++)
                {
                    ppl.Clear();
                    for (var i = 0; i < Rows; i++) ppl.Add(i, MatrixData[i][j]);
                    var color = ColorUtils.MatlabGraphColor(j);
                    var myCurve = myPane.AddCurve("", ppl.Clone(), color, SymbolType.None);
                }
            }

            var bm = new Bitmap(1, 1);
            using (var g = Graphics.FromImage(bm))
            {
                myPane.AxisChange(g);
            }

            myPane.GetImage().Save(fileName, ImageFormat.Png);
        }

        /// <summary>
        ///     Draw the matrix as an image
        ///     Imitating Matlabs imagesc(M), where M is the matrix
        /// </summary>
        /// <param name="fileName">filename</param>
        /// <param name="forceWidth">force pixel width (default=600). To ignore use 0 or -1</param>
        /// <param name="forceHeight">force pixel height (default=400). To ignore use 0 or -1</param>
        /// <param name="colorize">colorize (default=true) or use black and white (false)</param>
        /// <param name="flipYscale">bool whether to flip the y scale (default=false)</param>
        /// <returns>an image</returns>
        public Image DrawMatrixImage(string fileName, int forceWidth = 600, int forceHeight = 400, bool colorize = true,
            bool flipYscale = false)
        {
            var maxValue = Max();
            if (maxValue == 0.0f)
                return null;

            // map matrix values to colormap
            var rgb = new List<byte>();
            for (var i = 0; i < Rows; i++)
            for (var j = 0; j < Columns; j++)
            {
                var val = MatrixData[i][j];
                val /= maxValue; // Convert to range between [0 - 1]
                var color = (byte)(Math.Abs(val) * 255); // convert to between 0 - 255

                // Pixel data is ARGB, 1 byte for alpha, 1 for red, 1 for green, 1 for blue.
                // On a little-endian machine, like yours and many others,
                // the byte order is B G R A (little end is first).
                // So 0 0 255 255 equals blue = 0, green = 0, red = 255, alpha = 255. Red.
                // This endian-ness order disappears when you cast bd.Scan0 to an int* (pointer-to-integer)
                // since integers are stored little-endian as well.
                rgb.Add(color); // B
                rgb.Add(color); // G
                rgb.Add(color); // R
                rgb.Add(255); // A
            }

            // Convert to image
            var img = ImageUtils.ByteArrayToImage(rgb.ToArray(), Columns, Rows, PixelFormat.Format32bppArgb);

            // Should we resize?
            if (forceHeight > 0 && forceWidth > 0) img = ImageUtils.Resize(img, forceWidth, forceHeight, false);

            // Should we colorize?
            if (colorize) img = ColorUtils.Colorize(img, 255, ColorUtils.ColorPaletteType.MATLAB);

            if (flipYscale) img.RotateFlip(RotateFlipType.RotateNoneFlipY);

            // Save and return the image
            img.Save(fileName, ImageFormat.Png);
            return img;
        }

        /// <summary>
        ///     Draw the matrix as an image doing a log() on each of the values
        ///     For STFT matrixes this is the linear spectrogram
        /// </summary>
        /// <param name="fileName">filename</param>
        /// <param name="flipYscale">bool whether to flip the y scale (default=false)</param>
        /// <param name="usePowerSpectrum">bool whether to use powerspectrum (true) or amplitude/magnitude spectrum (false)</param>
        /// <param name="forceWidth">force pixel width (default=600). To ignore use 0 or -1</param>
        /// <param name="forceHeight">force pixel height (default=400). To ignore use 0 or -1</param>
        /// <param name="colorize">colorize (default=true) or use black and white (false)</param>
        /// <returns>an image</returns>
        /// <example>
        ///     This method can be used to plot a spectrogram
        ///     like the octave method:
        ///     audio = load ('audiodata.ascii.txt', '-ascii');
        ///     specgram (audio*32768, 2048, 44100, hanning(2048), 1024);
        ///     This is the same as:
        ///     stft = load ('stftdata.ascii.txt', '-ascii');
        ///     imagesc (flipud(log(stft)));
        /// </example>
        public Image DrawMatrixImageLogValues(string fileName, bool flipYscale = false, bool usePowerSpectrum = false,
            int forceWidth = 600, int forceHeight = 400, bool colorize = true)
        {
            // amplitude (or magnitude) is the square root of the power spectrum
            // the magnitude spectrum is abs(fft), i.e. Math.Sqrt(re*re + img*img)
            // use 20*log10(Y) to get dB from amplitude
            // the power spectrum is the magnitude spectrum squared
            // use 10*log10(Y) to get dB from power spectrum
            var maxValue = Max();
            if (usePowerSpectrum)
                maxValue = 10 * Math.Log10(maxValue);
            else
                maxValue = 20 * Math.Log10(maxValue);

            if (maxValue == 0.0f)
                return null;

            var blockSizeX = 1;
            var blockSizeY = 1;

            var img = new Bitmap(Columns * blockSizeX, Rows * blockSizeY);
            var graphics = Graphics.FromImage(img);

            for (var row = 0; row < Rows; row++)
            for (var column = 0; column < Columns; column++)
            {
                var val = MatrixData[row][column];
                if (usePowerSpectrum)
                    val = 10 * Math.Log10(val);
                else
                    val = 20 * Math.Log10(val);

                var color = ColorUtils.ValueToBlackWhiteColor(val, maxValue);
                Brush brush = new SolidBrush(color);

                if (flipYscale) // draw a small square
                    graphics.FillRectangle(brush, column * blockSizeX, (Rows - row - 1) * blockSizeY, blockSizeX,
                        blockSizeY);
                else // draw a small square
                    graphics.FillRectangle(brush, column * blockSizeX, row * blockSizeY, blockSizeX, blockSizeY);
            }

            // Should we resize?
            if (forceHeight > 0 && forceWidth > 0) img = (Bitmap)ImageUtils.Resize(img, forceWidth, forceHeight, false);

            // Should we colorize?
            if (colorize) img = ColorUtils.Colorize(img, 255, ColorUtils.ColorPaletteType.MATLAB);

            // Save and return the image
            img.Save(fileName, ImageFormat.Png);
            return img;
        }

        /// <summary>
        ///     Draw the matrix as an image where the Y scale and the values are logarithmic
        ///     For STFT matrixes this is the logarithmic spectrogram (magnitude spectrum)
        /// </summary>
        /// <param name="fileName">filename</param>
        /// <param name="sampleRate">Signal's sample rate</param>
        /// <param name="minFreq">Min frequency</param>
        /// <param name="maxFreq">Max frequency</param>
        /// <param name="logBins">Number of logarithmically spaced bins</param>
        /// <param name="fftSize">FFT Size</param>
        /// <param name="forceWidth">force pixel width (default=600). To ignore use 0 or -1</param>
        /// <param name="forceHeight">force pixel height (default=400). To ignore use 0 or -1</param>
        /// <param name="colorize">colorize (default=true) or use black and white (false)</param>
        /// <returns>an image</returns>
        public Image DrawMatrixImageLogY(string fileName, double sampleRate, double minFreq, double maxFreq,
            int logBins, int fftSize, int forceWidth = 600, int forceHeight = 400, bool colorize = true)
        {
            var maxValue = Max();
            maxValue = 20 * Math.Log10(maxValue);
            if (maxValue == 0.0f)
                return null;

            var blockSizeX = 1;
            var blockSizeY = 1;

            var img = new Bitmap(Columns * blockSizeX, logBins * blockSizeY);
            var graphics = Graphics.FromImage(img);

            int[] indexes;
            float[] frequencies;
            GenerateLogFrequencies(sampleRate, minFreq, maxFreq, logBins, fftSize, Math.E, out indexes,
                out frequencies);

            for (var column = 0; column < Columns; column++)
            {
                var col = GetColumn(column);
                var avg = ComputeLogAverages(col, sampleRate, Rows, logBins, indexes);

                for (var logBin = 0; logBin < logBins; logBin++)
                {
                    var val = avg[logBin];
                    val = 20 * Math.Log10(val);
                    var color = ColorUtils.ValueToBlackWhiteColor(val, maxValue);
                    Brush brush = new SolidBrush(color);

                    // draw a small square
                    graphics.FillRectangle(brush, column * blockSizeX, (logBins - logBin - 1) * blockSizeY, blockSizeX,
                        blockSizeY);
                }
            }

            // Should we resize?
            if (forceHeight > 0 && forceWidth > 0) img = (Bitmap)ImageUtils.Resize(img, forceWidth, forceHeight, false);

            // Should we colorize?
            if (colorize) img = ColorUtils.Colorize(img, 255, ColorUtils.ColorPaletteType.MATLAB);

            // Save and return the image
            img.Save(fileName, ImageFormat.Png);
            return img;
        }

        #endregion

        #region Overrides & Operators

        public override string ToString()
        {
            return string.Format("Rows: {0}, Columns: {1}", Rows, Columns);
        }

        public string GetAsString()
        {
            var str = new StringWriter();
            Print(str);
            return str.ToString();
        }

        public override int GetHashCode()
        {
            return -1;
        }

        public override bool Equals(object obj)
        {
            return MatrixAreEqual(((Matrix)obj).MatrixData, MatrixData, EPSILON);
        }

        public static bool operator ==(Matrix A, Matrix B)
        {
            return MatrixAreEqual(A.MatrixData, B.MatrixData, EPSILON);
        }

        public static bool operator !=(Matrix A, Matrix B)
        {
            return !(A == B);
        }

        public static Matrix operator +(Matrix A, Matrix B)
        {
            return A.Plus(B);
        }

        public static Matrix operator -(Matrix A, Matrix B)
        {
            return A.Minus(B);
        }

        public static Matrix operator *(Matrix A, Matrix B)
        {
            return A.Times(B);
        }

        public static Matrix operator *(Matrix A, double x)
        {
            return A.Times(x);
        }

        public static Matrix operator *(double x, Matrix A)
        {
            return A.Times(x);
        }

        public static Matrix operator /(Matrix A, Matrix B)
        {
            return A.ArrayRightDivide(B);
        }

        public static Matrix operator /(Matrix A, double x)
        {
            return A.Divide(x);
        }

        public static Matrix operator ^(Matrix A, int k)
        {
            return A.Pow(k);
        }

        #endregion

        #region Virtuals and Indexers

        /// <summary>
        ///     Access the component in row i, column j of a non-empty matrix.
        /// </summary>
        /// <param name="i">One-based row index.</param>
        /// <param name="j">One-based column index.</param>
        /// <returns></returns>
        public virtual double this[int i, int j]
        {
            set
            {
                if (i <= 0 || j <= 0) throw new ArgumentOutOfRangeException("Indices must be real positive.");

                if (i < Rows && j < Columns) MatrixData[i - 1][j - 1] = value;
            }
            get
            {
                if (i > 0 && i <= Rows && j > 0 && j <= Columns) return MatrixData[i - 1][j - 1];
                throw new ArgumentOutOfRangeException("Indices must not exceed size of matrix.");
            }
        }

        /// <summary>
        ///     Access to the i-th component of an n by one Matrix(column vector)
        ///     or one by n Matrix(row vector).
        /// </summary>
        /// <param name="i">One-based index.</param>
        /// <returns></returns>
        public virtual double this[int i]
        {
            set
            {
                if (Rows == 1) // row vector
                    MatrixData[0][i] = value;
                else if (Columns == 1) // column vector
                    MatrixData[i][0] = value;
                else
                    throw new InvalidOperationException("Cannot access multidimensional matrix via single index.");
            }
            get
            {
                if (Rows == 1) // row vector
                    return MatrixData[0][i];
                if (Columns == 1) // column vector
                    return MatrixData[i][0];
                throw new InvalidOperationException("General matrix access requires double indexing.");
            }
        }

        #endregion

        #region Private Methods

        // ------------------------
        //   Private Methods
        // ------------------------

        /// <summary>
        ///     Check if size(matrixData) == size(B)
        /// </summary>
        /// <param name="B">Matrix</param>
        private void CheckMatrixDimensions(Matrix B)
        {
            if (B.Rows != Rows || B.Columns != Columns) throw new ArgumentException("Matrix dimensions must agree.");
        }

        /// <summary>
        ///     Get logarithmically spaced indexes
        /// </summary>
        /// <param name="sampleRate">Signal's sample rate</param>
        /// <param name="minFreq">Min frequency</param>
        /// <param name="maxFreq">Max frequency</param>
        /// <param name="logBins">Number of logarithmically spaced bins</param>
        /// <param name="fftSize">FFT Size</param>
        /// <param name="logarithmicBase">Logarithm base, like Math.E</param>
        /// <param name="logFrequenciesIndex">Indexes output array</param>
        /// <param name="logFrequencies">Frequency output array</param>
        /// <remarks>
        ///     Copied from Sound Fingerprinting framework FingerprintManager
        ///     git://github.com/AddictedCS/soundfingerprinting.git
        ///     Code license: CPOL v.1.02
        ///     ciumac.sergiu@gmail.com
        /// </remarks>
        private static void GenerateLogFrequencies(double sampleRate, double minFreq, double maxFreq, int logBins,
            int fftSize, double logarithmicBase, out int[] logFrequenciesIndex, out float[] logFrequencies)
        {
            var logMin = Math.Log(minFreq, logarithmicBase);
            var logMax = Math.Log(maxFreq, logarithmicBase);
            var delta = (logMax - logMin) / logBins;

            logFrequenciesIndex = new int[logBins + 1];
            logFrequencies = new float[logBins + 1];
            double accDelta = 0;
            for (var i = 0; i < logBins; ++i)
            {
                var freq = (float)Math.Pow(logarithmicBase, logMin + accDelta);
                logFrequencies[i] = freq;

                accDelta += delta; // accDelta = delta * i;
                logFrequenciesIndex[i] = MathUtils.FreqToIndex(freq, sampleRate, fftSize);
            }
        }

        /// <summary>
        ///     Compute log bin averages for a passed spectrum
        /// </summary>
        /// <param name="spectrum">spectrum array (i.e. one column in a FFT matrix)</param>
        /// <param name="sampleRate">Signal's sample rate</param>
        /// <param name="fftSize">FFT Size</param>
        /// <param name="logBins">Number of logarithmically spaced bins</param>
        /// <param name="logFrequenciesIndex">logarithmically spaced indexes</param>
        /// <returns>array averages for each of the logBins</returns>
        private static double[] ComputeLogAverages(double[] spectrum, double sampleRate, int fftSize, int logBins,
            int[] logFrequenciesIndex)
        {
            var averages = new double[logBins];

            for (var i = 0; i < logBins; i++)
            {
                double avg = 0;

                var lowBound = logFrequenciesIndex[i];
                var hiBound = logFrequenciesIndex[i + 1];

                for (var j = lowBound; j <= hiBound; j++) // added the <= equal sign to ensure that we get the edges
                    avg += spectrum[j];

                avg /= hiBound - lowBound + 1; // added + 1 at the end to ensure we get the edges
                averages[i] = avg;
            }

            return averages;
        }

        #endregion
    }
}