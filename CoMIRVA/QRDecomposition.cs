﻿using System;
using CommonUtils;

namespace Comirva.Audio.Util.Maths
{
    /// <summary>
    ///     CoMIRVA: Collection of Music Information Retrieval and Visualization Applications
    ///     Ported from Java to C# by perivar@nerseth.com
    /// </summary>
    /// QR Decomposition.
    /// <P>
    ///     For an m-by-n matrix A with m >= n, the QR decomposition is an m-by-n
    ///     orthogonal matrix Q and an n-by-n upper triangular matrix R so that
    ///     A = Q*R.
    ///     <P>
    ///         The QR decompostion always exists, even if the matrix does not have
    ///         full rank, so the constructor will never fail.  The primary use of the
    ///         QR decomposition is in the least squares solution of nonsquare systems
    ///         of simultaneous linear equations.  This will fail if isFullRank()
    ///         returns false.
    public class QRDecomposition
    {
        // Row and column dimensions.
        // @serial column dimension.
        // @serial row dimension.
        private readonly int m;

        private readonly int n;
        // ------------------------
        //   Class variables
        // ------------------------

        // Array for internal storage of decomposition.
        // @serial internal array storage.
        private readonly double[][] QR;

        // Array for internal storage of diagonal of R.
        // @serial diagonal of R.
        private readonly double[] Rdiag;

        // ------------------------
        //   Constructor
        // ------------------------

        // QR Decomposition, computed by Householder reflections.
        // @param A    Rectangular matrix
        public QRDecomposition(Matrix A)
        {
            // Initialize.
            QR = A.GetArrayCopy();
            m = A.GetRowDimension();
            n = A.GetColumnDimension();
            Rdiag = new double[n];

            // Main loop.
            for (var k = 0; k < n; k++)
            {
                // Compute 2-norm of k-th column without under/overflow.
                double nrm = 0;
                for (var i = k; i < m; i++) nrm = MathUtils.Hypot(nrm, QR[i][k]);

                if (nrm != 0.0)
                {
                    // Form k-th Householder vector.
                    if (QR[k][k] < 0) nrm = -nrm;
                    for (var i = k; i < m; i++) QR[i][k] /= nrm;
                    QR[k][k] += 1.0;

                    // Apply transformation to remaining columns.
                    for (var j = k + 1; j < n; j++)
                    {
                        var s = 0.0;
                        for (var i = k; i < m; i++) s += QR[i][k] * QR[i][j];
                        s = -s / QR[k][k];
                        for (var i = k; i < m; i++) QR[i][j] += s * QR[i][k];
                    }
                }

                Rdiag[k] = -nrm;
            }
        }

        // ------------------------
        // Public Methods
        // ------------------------

        // Is the matrix full rank?
        // @return     true if R, and hence A, has full rank.
        public bool IsFullRank()
        {
            for (var j = 0; j < n; j++)
                if (Rdiag[j] == 0)
                    return false;
            return true;
        }

        // Return the Householder vectors
        // @return     Lower trapezoidal matrix whose columns define the reflections
        public Matrix GetH()
        {
            var X = new Matrix(m, n);
            var H = X.GetArray();
            for (var i = 0; i < m; i++)
            for (var j = 0; j < n; j++)
                if (i >= j)
                    H[i][j] = QR[i][j];
                else
                    H[i][j] = 0.0;
            return X;
        }

        // Return the upper triangular factor
        // @return     R
        public Matrix GetR()
        {
            var X = new Matrix(n, n);
            var R = X.GetArray();
            for (var i = 0; i < n; i++)
            for (var j = 0; j < n; j++)
                if (i < j)
                    R[i][j] = QR[i][j];
                else if (i == j)
                    R[i][j] = Rdiag[i];
                else
                    R[i][j] = 0.0;
            return X;
        }

        // Generate and return the (economy-sized) orthogonal factor
        // @return     Q
        public Matrix GetQ()
        {
            var X = new Matrix(m, n);
            var Q = X.GetArray();
            for (var k = n - 1; k >= 0; k--)
            {
                for (var i = 0; i < m; i++) Q[i][k] = 0.0;
                Q[k][k] = 1.0;
                for (var j = k; j < n; j++)
                    if (QR[k][k] != 0)
                    {
                        var s = 0.0;
                        for (var i = k; i < m; i++) s += QR[i][k] * Q[i][j];
                        s = -s / QR[k][k];
                        for (var i = k; i < m; i++) Q[i][j] += s * QR[i][k];
                    }
            }

            return X;
        }

        // Least squares solution of A*X = B
        // @param B    A Matrix with as many rows as A and any number of columns.
        // @return     X that minimizes the two norm of Q*R*X-B.
        // @exception  ArgumentException  Matrix row dimensions must agree.
        // @exception  Exception  Matrix is rank deficient.
        public Matrix Solve(Matrix B)
        {
            if (B.GetRowDimension() != m) throw new ArgumentException("Matrix row dimensions must agree.");
            if (!IsFullRank()) throw new Exception("Matrix is rank deficient.");

            // Copy right hand side
            var nx = B.GetColumnDimension();
            var X = B.GetArrayCopy();

            // Compute Y = transpose(Q)*B
            for (var k = 0; k < n; k++)
            for (var j = 0; j < nx; j++)
            {
                var s = 0.0;
                for (var i = k; i < m; i++) s += QR[i][k] * X[i][j];
                s = -s / QR[k][k];
                for (var i = k; i < m; i++) X[i][j] += s * QR[i][k];
            }

            // Solve R*X = Y;
            for (var k = n - 1; k >= 0; k--)
            {
                for (var j = 0; j < nx; j++) X[k][j] /= Rdiag[k];
                for (var i = 0; i < k; i++)
                for (var j = 0; j < nx; j++)
                    X[i][j] -= X[k][j] * QR[i][k];
            }

            return new Matrix(X, n, nx).GetMatrix(0, n - 1, 0, nx - 1);
        }
    }
}