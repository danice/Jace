/*
    MatrixInfo class in C#
    Written by Ivan Kuckir (ivan.kuckir@gmail.com, http://blog.ivank.net)
    Faculty of Mathematics and Physics
    Charles University in Prague
    (C) 2010
    - updated on 01.06.2014 - Trimming the string before parsing
    - updated on 14.06.2012 - parsing improved. Thanks to Andy!
    - updated on 03.10.2012 - there was a terrible bug in LU, SoLE and Inversion. Thanks to Danilo Neves Cruz for reporting that!
    - updated on 21.01.2014 - multiple changes based on comments -> see git for further info
	
    This code is distributed under MIT licence.
	
		Permission is hereby granted, free of charge, to any person
		obtaining a copy of this software and associated documentation
		files (the "Software"), to deal in the Software without
		restriction, including without limitation the rights to use,
		copy, modify, merge, publish, distribute, sublicense, and/or sell
		copies of the Software, and to permit persons to whom the
		Software is furnished to do so, subject to the following
		conditions:

		The above copyright notice and this permission notice shall be
		included in all copies or substantial portions of the Software.

		THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
		EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
		OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
		NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
		HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
		WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
		FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
		OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Jace.Execution
{
    public class MatrixInfo : RegistryBaseInfo
    {
        public int rows;
        public int cols;
        public double[] mat;

        public MatrixInfo L;
        public MatrixInfo U;
        private int[] pi;
        private double detOfP = 1;

        public MatrixInfo(string constantName, bool isOverWritable, int iRows, int iCols) : base(constantName, isOverWritable)         // MatrixInfo Class constructor
        {           
            rows = iRows;
            cols = iCols;
            mat = new double[rows * cols];
        }

        public MatrixInfo(int iRows, int iCols) : base(null,true)        // MatrixInfo Class constructor
        {            
            this.IsOverWritable = true;
            rows = iRows;
            cols = iCols;
            mat = new double[rows * cols];
        }

        public Boolean IsSquare()
        {
            return (rows == cols);
        }

        public double this[int iRow, int iCol]      // Access this MatrixInfo as a 2D array
        {
            get { 
                if (cols < 2)
                    return mat[iRow]; 
                else 
                    return mat[iRow * cols + iCol]; 
            }
            set { mat[iRow * cols + iCol] = value; }
        }

        public double GetItem(int iRow, int iCol)
        {
            return this[iRow, iCol];
        }

        public double GetItemBaseOne(int iRow, int iCol)
        {
            return this[iRow - 1, iCol - 1];
        }

        public double GetRowItemBaseOne(int iRow)
        {
            return mat[iRow - 1];
        }
       
        public double[] Values
        {
            set
            {
                this.mat = value;
            }
            get
            {
                return this.mat;
            }
        }

        public MatrixInfo GetCol(int k)
        {
            MatrixInfo m = new MatrixInfo(String.Format("{0}-col-{1}",this.Name, k), true, rows, 1);
            for (int i = 0; i < rows; i++) m[i, 0] = this[i, k];
            return m;
        }

        public void SetCol(MatrixInfo v, int k)
        {
            for (int i = 0; i < rows; i++) this[i, k] = v[i, 0];
        }

        public void MakeLU()                        // Function for LU decomposition
        {
            if (!IsSquare()) throw new MException("The MatrixInfo is not square!");
            L = IdentityMatrixInfo(rows, cols);
            U = Duplicate();

            pi = new int[rows];
            for (int i = 0; i < rows; i++) pi[i] = i;

            double p = 0;
            double pom2;
            int k0 = 0;
            int pom1 = 0;

            for (int k = 0; k < cols - 1; k++)
            {
                p = 0;
                for (int i = k; i < rows; i++)      // find the row with the biggest pivot
                {
                    if (Math.Abs(U[i, k]) > p)
                    {
                        p = Math.Abs(U[i, k]);
                        k0 = i;
                    }
                }
                if (p == 0) // samé nuly ve sloupci
                    throw new MException("The MatrixInfo is singular!");

                pom1 = pi[k]; pi[k] = pi[k0]; pi[k0] = pom1;    // switch two rows in permutation MatrixInfo

                for (int i = 0; i < k; i++)
                {
                    pom2 = L[k, i]; L[k, i] = L[k0, i]; L[k0, i] = pom2;
                }

                if (k != k0) detOfP *= -1;

                for (int i = 0; i < cols; i++)                  // Switch rows in U
                {
                    pom2 = U[k, i]; U[k, i] = U[k0, i]; U[k0, i] = pom2;
                }

                for (int i = k + 1; i < rows; i++)
                {
                    L[i, k] = U[i, k] / U[k, k];
                    for (int j = k; j < cols; j++)
                        U[i, j] = U[i, j] - L[i, k] * U[k, j];
                }
            }
        }

        public MatrixInfo SolveWith(MatrixInfo v)                        // Function solves Ax = v in confirmity with solution vector "v"
        {
            if (rows != cols) throw new MException("The MatrixInfo is not square!");
            if (rows != v.rows) throw new MException("Wrong number of results in solution vector!");
            if (L == null) MakeLU();

            MatrixInfo b = new MatrixInfo(rows, 1);
            for (int i = 0; i < rows; i++) b[i, 0] = v[pi[i], 0];   // switch two items in "v" due to permutation MatrixInfo

            MatrixInfo z = SubsForth(L, b);
            MatrixInfo x = SubsBack(U, z);

            return x;
        }

        // TODO check for redundancy with MakeLU() and SolveWith()
        public void MakeRref()                                    // Function makes reduced echolon form
        {
            int lead = 0;
            for (int r = 0; r < rows; r++)
            {
                if (cols <= lead) break;
                int i = r;
                while (this[i, lead] == 0)
                {
                    i++;
                    if (i == rows)
                    {
                        i = r;
                        lead++;
                        if (cols == lead)
                        {
                            lead--;
                            break;
                        }
                    }
                }
                for (int j = 0; j < cols; j++)
                {
                    double temp = this[r, j];
                    this[r, j] = this[i, j];
                    this[i, j] = temp;
                }
                double div = this[r, lead];
                for (int j = 0; j < cols; j++) this[r, j] /= div;
                for (int j = 0; j < rows; j++)
                {
                    if (j != r)
                    {
                        double sub = this[j, lead];
                        for (int k = 0; k < cols; k++) this[j, k] -= (sub * this[r, k]);
                    }
                }
                lead++;
            }
        }

        public MatrixInfo Invert()                                   // Function returns the inverted MatrixInfo
        {
            if (L == null) MakeLU();

            MatrixInfo inv = new MatrixInfo(rows, cols);

            for (int i = 0; i < rows; i++)
            {
                MatrixInfo Ei = MatrixInfo.ZeroMatrixInfo(rows, 1);
                Ei[i, 0] = 1;
                MatrixInfo col = SolveWith(Ei);
                inv.SetCol(col, i);
            }
            return inv;
        }


        public double Det()                         // Function for determinant
        {
            if (L == null) MakeLU();
            double det = detOfP;
            for (int i = 0; i < rows; i++) det *= U[i, i];
            return det;
        }

        public MatrixInfo GetP()                        // Function returns permutation MatrixInfo "P" due to permutation vector "pi"
        {
            if (L == null) MakeLU();

            MatrixInfo MatrixInfo = ZeroMatrixInfo(rows, cols);
            for (int i = 0; i < rows; i++) MatrixInfo[pi[i], i] = 1;
            return MatrixInfo;
        }

        public MatrixInfo Duplicate()                   // Function returns the copy of this MatrixInfo
        {
            MatrixInfo MatrixInfo = new MatrixInfo(rows, cols);
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    MatrixInfo[i, j] = this[i, j];
            return MatrixInfo;
        }

        public static MatrixInfo SubsForth(MatrixInfo A, MatrixInfo b)          // Function solves Ax = b for A as a lower triangular MatrixInfo
        {
            if (A.L == null) A.MakeLU();
            int n = A.rows;
            MatrixInfo x = new MatrixInfo(n, 1);

            for (int i = 0; i < n; i++)
            {
                x[i, 0] = b[i, 0];
                for (int j = 0; j < i; j++) x[i, 0] -= A[i, j] * x[j, 0];
                x[i, 0] = x[i, 0] / A[i, i];
            }
            return x;
        }

        public static MatrixInfo SubsBack(MatrixInfo A, MatrixInfo b)           // Function solves Ax = b for A as an upper triangular MatrixInfo
        {
            if (A.L == null) A.MakeLU();
            int n = A.rows;
            MatrixInfo x = new MatrixInfo(n, 1);

            for (int i = n - 1; i > -1; i--)
            {
                x[i, 0] = b[i, 0];
                for (int j = n - 1; j > i; j--) x[i, 0] -= A[i, j] * x[j, 0];
                x[i, 0] = x[i, 0] / A[i, i];
            }
            return x;
        }

        public static MatrixInfo ZeroMatrixInfo(int iRows, int iCols)       // Function generates the zero MatrixInfo
        {
            MatrixInfo MatrixInfo = new MatrixInfo(iRows, iCols);
            for (int i = 0; i < iRows; i++)
                for (int j = 0; j < iCols; j++)
                    MatrixInfo[i, j] = 0;
            return MatrixInfo;
        }

        public static MatrixInfo IdentityMatrixInfo(int iRows, int iCols)   // Function generates the identity MatrixInfo
        {
            MatrixInfo MatrixInfo = ZeroMatrixInfo(iRows, iCols);
            for (int i = 0; i < Math.Min(iRows, iCols); i++)
                MatrixInfo[i, i] = 1;
            return MatrixInfo;
        }

        public static MatrixInfo RandomMatrixInfo(int iRows, int iCols, int dispersion)       // Function generates the random MatrixInfo
        {
            Random random = new Random();
            MatrixInfo MatrixInfo = new MatrixInfo(iRows, iCols);
            for (int i = 0; i < iRows; i++)
                for (int j = 0; j < iCols; j++)
                    MatrixInfo[i, j] = random.Next(-dispersion, dispersion);
            return MatrixInfo;
        }

        public static MatrixInfo Parse(string ps)                        // Function parses the MatrixInfo from string
        {
            string s = NormalizeMatrixInfoString(ps);
            string[] rows = Regex.Split(s, "\r\n");
            string[] nums = rows[0].Split(' ');
            MatrixInfo MatrixInfo = new MatrixInfo(rows.Length, nums.Length);
            try
            {
                for (int i = 0; i < rows.Length; i++)
                {
                    nums = rows[i].Split(' ');
                    for (int j = 0; j < nums.Length; j++) MatrixInfo[i, j] = double.Parse(nums[j]);
                }
            }
            catch (FormatException) { throw new MException("Wrong input format!"); }
            return MatrixInfo;
        }

        public override string ToString()                           // Function returns MatrixInfo as a string
        {
            StringBuilder s = new StringBuilder();
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                    s.Append(String.Format("{0,5:E2}", this[i, j]) + " ");
                s.AppendLine();
            }
            return s.ToString();
        }

        public static MatrixInfo Transpose(MatrixInfo m)              // MatrixInfo transpose, for any rectangular MatrixInfo
        {
            MatrixInfo t = new MatrixInfo(m.cols, m.rows);
            for (int i = 0; i < m.rows; i++)
                for (int j = 0; j < m.cols; j++)
                    t[j, i] = m[i, j];
            return t;
        }

        public static MatrixInfo Power(MatrixInfo m, int pow)           // Power MatrixInfo to exponent
        {
            if (pow == 0) return IdentityMatrixInfo(m.rows, m.cols);
            if (pow == 1) return m.Duplicate();
            if (pow == -1) return m.Invert();

            MatrixInfo x;
            if (pow < 0) { x = m.Invert(); pow *= -1; }
            else x = m.Duplicate();

            MatrixInfo ret = IdentityMatrixInfo(m.rows, m.cols);
            while (pow != 0)
            {
                if ((pow & 1) == 1) ret *= x;
                x *= x;
                pow >>= 1;
            }
            return ret;
        }

        private static void SafeAplusBintoC(MatrixInfo A, int xa, int ya, MatrixInfo B, int xb, int yb, MatrixInfo C, int size)
        {
            for (int i = 0; i < size; i++)          // rows
                for (int j = 0; j < size; j++)     // cols
                {
                    C[i, j] = 0;
                    if (xa + j < A.cols && ya + i < A.rows) C[i, j] += A[ya + i, xa + j];
                    if (xb + j < B.cols && yb + i < B.rows) C[i, j] += B[yb + i, xb + j];
                }
        }

        private static void SafeAminusBintoC(MatrixInfo A, int xa, int ya, MatrixInfo B, int xb, int yb, MatrixInfo C, int size)
        {
            for (int i = 0; i < size; i++)          // rows
                for (int j = 0; j < size; j++)     // cols
                {
                    C[i, j] = 0;
                    if (xa + j < A.cols && ya + i < A.rows) C[i, j] += A[ya + i, xa + j];
                    if (xb + j < B.cols && yb + i < B.rows) C[i, j] -= B[yb + i, xb + j];
                }
        }

        private static void SafeACopytoC(MatrixInfo A, int xa, int ya, MatrixInfo C, int size)
        {
            for (int i = 0; i < size; i++)          // rows
                for (int j = 0; j < size; j++)     // cols
                {
                    C[i, j] = 0;
                    if (xa + j < A.cols && ya + i < A.rows) C[i, j] += A[ya + i, xa + j];
                }
        }

        private static void AplusBintoC(MatrixInfo A, int xa, int ya, MatrixInfo B, int xb, int yb, MatrixInfo C, int size)
        {
            for (int i = 0; i < size; i++)          // rows
                for (int j = 0; j < size; j++) C[i, j] = A[ya + i, xa + j] + B[yb + i, xb + j];
        }

        private static void AminusBintoC(MatrixInfo A, int xa, int ya, MatrixInfo B, int xb, int yb, MatrixInfo C, int size)
        {
            for (int i = 0; i < size; i++)          // rows
                for (int j = 0; j < size; j++) C[i, j] = A[ya + i, xa + j] - B[yb + i, xb + j];
        }

        private static void ACopytoC(MatrixInfo A, int xa, int ya, MatrixInfo C, int size)
        {
            for (int i = 0; i < size; i++)          // rows
                for (int j = 0; j < size; j++) C[i, j] = A[ya + i, xa + j];
        }

        // TODO assume MatrixInfo 2^N x 2^N and then directly call StrassenMultiplyRun(A,B,?,1,?)
        private static MatrixInfo StrassenMultiply(MatrixInfo A, MatrixInfo B)                // Smart MatrixInfo multiplication
        {
            if (A.cols != B.rows) throw new MException("Wrong dimension of MatrixInfo!");

            MatrixInfo R;

            int msize = Math.Max(Math.Max(A.rows, A.cols), Math.Max(B.rows, B.cols));

            int size = 1; int n = 0;
            while (msize > size) { size *= 2; n++; };
            int h = size / 2;


            MatrixInfo[,] mField = new MatrixInfo[n, 9];

            /*
             *  8x8, 8x8, 8x8, ...
             *  4x4, 4x4, 4x4, ...
             *  2x2, 2x2, 2x2, ...
             *  . . .
             */

            int z;
            for (int i = 0; i < n - 4; i++)          // rows
            {
                z = (int)Math.Pow(2, n - i - 1);
                for (int j = 0; j < 9; j++) mField[i, j] = new MatrixInfo(z, z);
            }

            SafeAplusBintoC(A, 0, 0, A, h, h, mField[0, 0], h);
            SafeAplusBintoC(B, 0, 0, B, h, h, mField[0, 1], h);
            StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 1], 1, mField); // (A11 + A22) * (B11 + B22);

            SafeAplusBintoC(A, 0, h, A, h, h, mField[0, 0], h);
            SafeACopytoC(B, 0, 0, mField[0, 1], h);
            StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 2], 1, mField); // (A21 + A22) * B11;

            SafeACopytoC(A, 0, 0, mField[0, 0], h);
            SafeAminusBintoC(B, h, 0, B, h, h, mField[0, 1], h);
            StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 3], 1, mField); //A11 * (B12 - B22);

            SafeACopytoC(A, h, h, mField[0, 0], h);
            SafeAminusBintoC(B, 0, h, B, 0, 0, mField[0, 1], h);
            StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 4], 1, mField); //A22 * (B21 - B11);

            SafeAplusBintoC(A, 0, 0, A, h, 0, mField[0, 0], h);
            SafeACopytoC(B, h, h, mField[0, 1], h);
            StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 5], 1, mField); //(A11 + A12) * B22;

            SafeAminusBintoC(A, 0, h, A, 0, 0, mField[0, 0], h);
            SafeAplusBintoC(B, 0, 0, B, h, 0, mField[0, 1], h);
            StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 6], 1, mField); //(A21 - A11) * (B11 + B12);

            SafeAminusBintoC(A, h, 0, A, h, h, mField[0, 0], h);
            SafeAplusBintoC(B, 0, h, B, h, h, mField[0, 1], h);
            StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 7], 1, mField); // (A12 - A22) * (B21 + B22);

            R = new MatrixInfo(A.rows, B.cols);                  // result

            /// C11
            for (int i = 0; i < Math.Min(h, R.rows); i++)          // rows
                for (int j = 0; j < Math.Min(h, R.cols); j++)     // cols
                    R[i, j] = mField[0, 1 + 1][i, j] + mField[0, 1 + 4][i, j] - mField[0, 1 + 5][i, j] + mField[0, 1 + 7][i, j];

            /// C12
            for (int i = 0; i < Math.Min(h, R.rows); i++)          // rows
                for (int j = h; j < Math.Min(2 * h, R.cols); j++)     // cols
                    R[i, j] = mField[0, 1 + 3][i, j - h] + mField[0, 1 + 5][i, j - h];

            /// C21
            for (int i = h; i < Math.Min(2 * h, R.rows); i++)          // rows
                for (int j = 0; j < Math.Min(h, R.cols); j++)     // cols
                    R[i, j] = mField[0, 1 + 2][i - h, j] + mField[0, 1 + 4][i - h, j];

            /// C22
            for (int i = h; i < Math.Min(2 * h, R.rows); i++)          // rows
                for (int j = h; j < Math.Min(2 * h, R.cols); j++)     // cols
                    R[i, j] = mField[0, 1 + 1][i - h, j - h] - mField[0, 1 + 2][i - h, j - h] + mField[0, 1 + 3][i - h, j - h] + mField[0, 1 + 6][i - h, j - h];

            return R;
        }
        private static void StrassenMultiplyRun(MatrixInfo A, MatrixInfo B, MatrixInfo C, int l, MatrixInfo[,] f)    // A * B into C, level of recursion, MatrixInfo field
        {
            int size = A.rows;
            int h = size / 2;

            AplusBintoC(A, 0, 0, A, h, h, f[l, 0], h);
            AplusBintoC(B, 0, 0, B, h, h, f[l, 1], h);
            StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 1 + 1], l + 1, f); // (A11 + A22) * (B11 + B22);

            AplusBintoC(A, 0, h, A, h, h, f[l, 0], h);
            ACopytoC(B, 0, 0, f[l, 1], h);
            StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 1 + 2], l + 1, f); // (A21 + A22) * B11;

            ACopytoC(A, 0, 0, f[l, 0], h);
            AminusBintoC(B, h, 0, B, h, h, f[l, 1], h);
            StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 1 + 3], l + 1, f); //A11 * (B12 - B22);

            ACopytoC(A, h, h, f[l, 0], h);
            AminusBintoC(B, 0, h, B, 0, 0, f[l, 1], h);
            StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 1 + 4], l + 1, f); //A22 * (B21 - B11);

            AplusBintoC(A, 0, 0, A, h, 0, f[l, 0], h);
            ACopytoC(B, h, h, f[l, 1], h);
            StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 1 + 5], l + 1, f); //(A11 + A12) * B22;

            AminusBintoC(A, 0, h, A, 0, 0, f[l, 0], h);
            AplusBintoC(B, 0, 0, B, h, 0, f[l, 1], h);
            StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 1 + 6], l + 1, f); //(A21 - A11) * (B11 + B12);

            AminusBintoC(A, h, 0, A, h, h, f[l, 0], h);
            AplusBintoC(B, 0, h, B, h, h, f[l, 1], h);
            StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 1 + 7], l + 1, f); // (A12 - A22) * (B21 + B22);

            /// C11
            for (int i = 0; i < h; i++)          // rows
                for (int j = 0; j < h; j++)     // cols
                    C[i, j] = f[l, 1 + 1][i, j] + f[l, 1 + 4][i, j] - f[l, 1 + 5][i, j] + f[l, 1 + 7][i, j];

            /// C12
            for (int i = 0; i < h; i++)          // rows
                for (int j = h; j < size; j++)     // cols
                    C[i, j] = f[l, 1 + 3][i, j - h] + f[l, 1 + 5][i, j - h];

            /// C21
            for (int i = h; i < size; i++)          // rows
                for (int j = 0; j < h; j++)     // cols
                    C[i, j] = f[l, 1 + 2][i - h, j] + f[l, 1 + 4][i - h, j];

            /// C22
            for (int i = h; i < size; i++)          // rows
                for (int j = h; j < size; j++)     // cols
                    C[i, j] = f[l, 1 + 1][i - h, j - h] - f[l, 1 + 2][i - h, j - h] + f[l, 1 + 3][i - h, j - h] + f[l, 1 + 6][i - h, j - h];
        }
        private static MatrixInfo StupidMultiply(MatrixInfo m1, MatrixInfo m2)                  // Stupid MatrixInfo multiplication
        {
            if (m1.cols != m2.rows) throw new MException("Wrong dimensions of MatrixInfo!");

            MatrixInfo result = ZeroMatrixInfo(m1.rows, m2.cols);
            for (int i = 0; i < result.rows; i++)
                for (int j = 0; j < result.cols; j++)
                    for (int k = 0; k < m1.cols; k++)
                        result[i, j] += m1[i, k] * m2[k, j];
            return result;
        }

        private static MatrixInfo Multiply(MatrixInfo m1, MatrixInfo m2)                         // MatrixInfo multiplication
        {
            if (m1.cols != m2.rows) throw new MException("Wrong dimension of MatrixInfo!");
            int msize = Math.Max(Math.Max(m1.rows, m1.cols), Math.Max(m2.rows, m2.cols));
            // stupid multiplication faster for small matrices
            if (msize < 32)
            {
                return StupidMultiply(m1, m2);
            }
            // stupid multiplication faster for non square matrices
            if (!m1.IsSquare() || !m2.IsSquare())
            {
                return StupidMultiply(m1, m2);
            }
            // Strassen multiplication is faster for large square MatrixInfo 2^N x 2^N
            // NOTE because of previous checks msize == m1.cols == m1.rows == m2.cols == m2.cols
            double exponent = Math.Log(msize) / Math.Log(2);
            if (Math.Pow(2, exponent) == msize)
            {
                return StrassenMultiply(m1, m2);
            }
            else
            {
                return StupidMultiply(m1, m2);
            }
        }
        private static MatrixInfo Multiply(double n, MatrixInfo m)                          // Multiplication by constant n
        {
            MatrixInfo r = new MatrixInfo(m.rows, m.cols);
            for (int i = 0; i < m.rows; i++)
                for (int j = 0; j < m.cols; j++)
                    r[i, j] = m[i, j] * n;
            return r;
        }
        private static MatrixInfo Add(MatrixInfo m1, MatrixInfo m2)         // Sčítání matic
        {
            if (m1.rows != m2.rows || m1.cols != m2.cols) throw new MException("Matrices must have the same dimensions!");
            MatrixInfo r = new MatrixInfo(m1.rows, m1.cols);
            for (int i = 0; i < r.rows; i++)
                for (int j = 0; j < r.cols; j++)
                    r[i, j] = m1[i, j] + m2[i, j];
            return r;
        }

        public static string NormalizeMatrixInfoString(string matStr)	// From Andy - thank you! :)
        {
            // Remove any multiple spaces
            while (matStr.IndexOf("  ") != -1)
                matStr = matStr.Replace("  ", " ");

            // Remove any spaces before or after newlines
            matStr = matStr.Replace(" \r\n", "\r\n");
            matStr = matStr.Replace("\r\n ", "\r\n");

            // If the data ends in a newline, remove the trailing newline.
            // Make it easier by first replacing \r\n’s with |’s then
            // restore the |’s with \r\n’s
            matStr = matStr.Replace("\r\n", "|");
            while (matStr.LastIndexOf("|") == (matStr.Length - 1))
                matStr = matStr.Substring(0, matStr.Length - 1);

            matStr = matStr.Replace("|", "\r\n");
            return matStr.Trim();
        }

        //   O P E R A T O R S

        public static MatrixInfo operator -(MatrixInfo m)
        { return MatrixInfo.Multiply(-1, m); }

        public static MatrixInfo operator +(MatrixInfo m1, MatrixInfo m2)
        { return MatrixInfo.Add(m1, m2); }

        public static MatrixInfo operator -(MatrixInfo m1, MatrixInfo m2)
        { return MatrixInfo.Add(m1, -m2); }

        public static MatrixInfo operator *(MatrixInfo m1, MatrixInfo m2)
        { return MatrixInfo.Multiply(m1, m2); }

        public static MatrixInfo operator *(double n, MatrixInfo m)
        { return MatrixInfo.Multiply(n, m); }

        protected override int GetNumberOfParameters()
        {
            if (cols > 1 && rows > 1)
                return 2;
            return 1;            
        }
    }

    //  The class for exceptions

    public class MException : Exception
    {
        public MException(string Message)
            : base(Message)
        { }
    }
}