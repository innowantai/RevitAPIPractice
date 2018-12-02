using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7_CreateFloorGeneral
{
    class MATRIX
    {
        public double[,] Matrix;
        private int row;
        private int col;
        public MATRIX(double[,] Matrix_)
        {
            this.Matrix = Matrix_;
            this.row = this.Matrix.GetLength(0);
            this.col = this.Matrix.GetLength(1);
        }

        public MATRIX InverseMatrix()
        {
            double[,] newData = new double[,] { {this.Matrix[1,1], -1*this.Matrix[0, 1] },
                                                { -1*this.Matrix[1, 0], this.Matrix[0, 0] } };


            return (new MATRIX(newData)).DivValue(this.GetDeterminant());

        }

        private double GetDeterminant()
        {
            return this.Matrix[0, 0] * this.Matrix[1, 1] - this.Matrix[1, 0] * this.Matrix[0, 1];
        }

        public double[,] CrossValue(double value)
        {
            double[,] newM = new double[this.row, this.col];
            for (int i = 0; i < this.row; i++)
            {
                for (int j = 0; j < this.col; j++)
                {
                    newM[i, j] = this.Matrix[i, j] * value;
                }
            }
            return newM;
        }

        public MATRIX DivValue(double value)
        {
            double[,] newM = new double[this.row, this.col];
            for (int i = 0; i < this.row; i++)
            {
                for (int j = 0; j < this.col; j++)
                {
                    newM[i, j] = this.Matrix[i, j] / value;
                }
            }
            return new MATRIX(newM);
        }


        public MATRIX CrossMatrix(MATRIX Matrix2)
        {
            double[,] M2 = Matrix2.Matrix;
            double[,] resData = new double[this.row, M2.GetLength(1)];
            for (int i = 0; i < this.row; i++)
            {
                for (int j = 0; j < M2.GetLength(1); j++)
                {
                    resData[i, j] = VectorDot(getVector(this.Matrix, i, "ROW"), getVector(M2, j, "COL"));
                }
            }

            return new MATRIX(resData);
        }


        private double VectorDot(double[] v1, double[] v2)
        {
            double res = 0;
            for (int i = 0; i < v1.Length; i++)
            {
                res += v1[i] * v2[i];
            }
            return res;
        }

        private double[] getVector(double[,] M, int po, string CASE)
        {
            int len = CASE == "ROW" ? M.GetLength(1) : M.GetLength(0);
            double[] ve = new double[len];
            for (int i = 0; i < len; i++)
            {
                if (CASE == "ROW")
                {
                    ve[i] = M[po, i];
                }
                else
                {
                    ve[i] = M[i, po];
                }
            }
            return ve;
        }

        public void ShowValues()
        {
            string res = "";
            for (int i = 0; i < this.row; i++)
            {
                for (int j = 0; j < this.col; j++)
                {
                    res += this.Matrix[i, j] + " ";
                }
                res += "\r\n";
            }
            Console.WriteLine(res);
        }

    }
}
