﻿using System;

namespace Comirva.Audio.Util.Maths
{
    /// <summary>
    ///     CoMIRVA: Collection of Music Information Retrieval and Visualization Applications
    ///     Ported from Java to C# by perivar@nerseth.com
    /// </summary>
    /// LU Decomposition.
    /// <P>
    ///     For an m-by-n matrix A with m >= n, the LU decomposition is an m-by-n
    ///     unit lower triangular matrix L, an n-by-n upper triangular matrix U,
    ///     and a permutation vector piv of length m so that A(piv,:) = L*U.
    ///     If m < n, then L is m-by-m and U is m-by-n.
    ///     <P>
    ///         The LU decompostion with pivoting always exists, even if the matrix is
    ///         singular, so the constructor will never fail.  The primary use of the
    ///         LU decomposition is in the solution of square systems of simultaneous
    ///         linear equations.  This will fail if isNonsingular() returns false.
    public class LUDecomposition
    {
        // ------------------------
        //   Class variables
        // ------------------------

        // Array for internal storage of decomposition.
        // @serial internal array storage.
        private readonly double[][] LU;

        // Row and column dimensions, and pivot sign.
        // @serial column dimension.
        // @serial row dimension.
        // @serial pivot sign.
        private readonly int m;
        private readonly int n;
        private readonly int pivsign;

        // Internal storage of pivot vector.
        // @serial pivot vector.
        private readonly int[] piv;

        // ------------------------
        //   Constructor
        // ------------------------

        // LU Decomposition
        // @param  A   Rectangular matrix
        public LUDecomposition(Matrix A)
        {
            // Use a "left-looking", dot-product, Crout/Doolittle algorithm.
            LU = A.GetArrayCopy();
            m = A.GetRowDimension();
            n = A.GetColumnDimension();
            piv = new int[m];
            for (var i = 0; i < m; i++) piv[i] = i;
            pivsign = 1;
            double[] LUrowi;
            var LUcolj = new double[m];

            // Outer loop.
            for (var j = 0; j < n; j++)
            {
                // Make a copy of the j-th column to localize references.
                for (var i = 0; i < m; i++) LUcolj[i] = LU[i][j];

                // Apply previous transformations.
                for (var i = 0; i < m; i++)
                {
                    LUrowi = LU[i];

                    // Most of the time is spent in the following dot product.
                    var kmax = Math.Min(i, j);
                    var s = 0.0;
                    for (var k = 0; k < kmax; k++) s += LUrowi[k] * LUcolj[k];

                    LUrowi[j] = LUcolj[i] -= s;
                }

                // Find pivot and exchange if necessary.
                var p = j;
                for (var i = j + 1; i < m; i++)
                    if (Math.Abs(LUcolj[i]) > Math.Abs(LUcolj[p]))
                        p = i;
                if (p != j)
                {
                    for (var k = 0; k < n; k++)
                    {
                        var t = LU[p][k];
                        LU[p][k] = LU[j][k];
                        LU[j][k] = t;
                    }

                    var k1 = piv[p];
                    piv[p] = piv[j];
                    piv[j] = k1;
                    pivsign = -pivsign;
                }

                // Compute multipliers.
                if ((j < m) & (LU[j][j] != 0.0))
                    for (var i = j + 1; i < m; i++)
                        LU[i][j] /= LU[j][j];
            }
        }

        // ------------------------
        // Temporary, experimental code.
        // ------------------------
        //
        //   LU Decomposition, computed by Gaussian elimination.
        //   <P>
        //   This constructor computes L and U with the "daxpy"-based elimination
        //   algorithm used in LINPACK and MATLAB.  In Java, we suspect the dot-product,
        //   Crout algorithm will be faster.  We have temporarily included this
        //   constructor until timing experiments confirm this suspicion.
        //   <P>
        //   @param  A             Rectangular matrix
        //   @param  linpackflag   Use Gaussian elimination.  Actual value ignored.
        //   @return               Structure to access L, U and piv.
        //
        //   public LUDecomposition (Matrix A, int linpackflag) {
        //      // Initialize.
        //      LU = A.getArrayCopy();
        //      m = A.getRowDimension();
        //      n = A.getColumnDimension();
        //      piv = new int[m];
        //      for (int i = 0; i < m; i++) {
        //         piv[i] = i;
        //      }
        //      pivsign = 1;
        //      // Main loop.
        //      for (int k = 0; k < n; k++) {
        //         // Find pivot.
        //         int p = k;
        //         for (int i = k+1; i < m; i++) {
        //            if (Math.abs(LU[i][k]) > Math.abs(LU[p][k])) {
        //               p = i;
        //            }
        //         }
        //         // Exchange if necessary.
        //         if (p != k) {
        //            for (int j = 0; j < n; j++) {
        //               double t = LU[p][j]; LU[p][j] = LU[k][j]; LU[k][j] = t;
        //            }
        //            int t = piv[p]; piv[p] = piv[k]; piv[k] = t;
        //            pivsign = -pivsign;
        //         }
        //         // Compute multipliers and eliminate k-th column.
        //         if (LU[k][k] != 0.0) {
        //            for (int i = k+1; i < m; i++) {
        //               LU[i][k] /= LU[k][k];
        //               for (int j = k+1; j < n; j++) {
        //                  LU[i][j] -= LU[i][k]*LU[k][j];
        //               }
        //            }
        //         }
        //      }
        //   }
        //
        // ------------------------
        // End of temporary code.
        // ------------------------

        // ------------------------
        // Public Methods
        // ------------------------

        // Is the matrix nonsingular?
        // @return     true if U, and hence A, is nonsingular.
        public bool IsNonsingular()
        {
            for (var j = 0; j < n; j++)
                if (LU[j][j] == 0)
                    return false;
            return true;
        }

        // Return lower triangular factor
        // @return     L
        public Matrix GetL()
        {
            var X = new Matrix(m, n);
            var L = X.GetArray();
            for (var i = 0; i < m; i++)
            for (var j = 0; j < n; j++)
                if (i > j)
                    L[i][j] = LU[i][j];
                else if (i == j)
                    L[i][j] = 1.0;
                else
                    L[i][j] = 0.0;
            return X;
        }

        // Return upper triangular factor
        // @return     U
        public Matrix GetU()
        {
            var X = new Matrix(n, n);
            var U = X.GetArray();
            for (var i = 0; i < n; i++)
            for (var j = 0; j < n; j++)
                if (i <= j)
                    U[i][j] = LU[i][j];
                else
                    U[i][j] = 0.0;
            return X;
        }

        // Return pivot permutation vector
        // @return     piv
        public int[] GetPivot()
        {
            var p = new int[m];
            for (var i = 0; i < m; i++) p[i] = piv[i];
            return p;
        }

        // Return pivot permutation vector as a one-dimensional double array
        // @return     (double) piv
        public double[] GetDoublePivot()
        {
            var vals = new double[m];
            for (var i = 0; i < m; i++) vals[i] = piv[i];
            return vals;
        }

        // Determinant
        // @return     det(A)
        // @exception  ArgumentException  Matrix must be square
        public double Det()
        {
            if (m != n) throw new ArgumentException("Matrix must be square.");
            double d = pivsign;
            for (var j = 0; j < n; j++) d *= LU[j][j];
            return d;
        }

        // Solve A*X = B
        // @param  B   A Matrix with as many rows as A and any number of columns.
        // @return     X so that L*U*X = B(piv,:)
        // @exception  ArgumentException Matrix row dimensions must agree.
        // @exception  Exception  Matrix is singular.
        public Matrix Solve(Matrix B)
        {
            if (B.GetRowDimension() != m) throw new ArgumentException("Matrix row dimensions must agree.");
            if (!IsNonsingular()) throw new Exception("Matrix is singular.");

            // Copy right hand side with pivoting
            var nx = B.GetColumnDimension();
            var Xmat = B.GetMatrix(piv, 0, nx - 1);
            var X = Xmat.GetArray();

            // Solve L*Y = B(piv,:)
            for (var k = 0; k < n; k++)
            for (var i = k + 1; i < n; i++)
            for (var j = 0; j < nx; j++)
                X[i][j] -= X[k][j] * LU[i][k];
            // Solve U*X = Y;
            for (var k = n - 1; k >= 0; k--)
            {
                for (var j = 0; j < nx; j++) X[k][j] /= LU[k][k];
                for (var i = 0; i < k; i++)
                for (var j = 0; j < nx; j++)
                    X[i][j] -= X[k][j] * LU[i][k];
            }

            return Xmat;
        }
    }
}