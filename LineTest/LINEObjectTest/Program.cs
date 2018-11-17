using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace LINEObjectTest
{
    class Program
    {
        static void Main(string[] args)
        {
            LINE line1 = new LINE(new XYZ(0,0,0), new XYZ(6,0,0));
            LINE line2 = new LINE(new XYZ(10,0,0), new XYZ(15,0,0));

            XYZ res = GetCrossPoint(line1,line2);
            res.ShowRes();
            
        }


        public static XYZ GetCrossPoint(LINE line1, LINE line2)
        {

            if (line1.GetSlope() == line2.GetSlope())
            {
                return line1.endPoint;
            }

            MATRIX m1 = new MATRIX(new double[,] { { line1.Direction.X, -line2.Direction.X },
                                                    {line1.Direction.Y, -line2.Direction.Y } });
            MATRIX m2 = new MATRIX(new double[,] { { line2.OriPoint.X-line1.OriPoint.X }, { line2.OriPoint.Y - line1.OriPoint.Y } });
             

            MATRIX m3 = m1.InverseMatrix();
            MATRIX res = m3.CrossMatrix(m2);

            double[,] tt = res.Matrix;
            double newX = line1.OriPoint.X + line1.Direction.X * tt[0,0];
            double newY = line1.OriPoint.Y + line1.Direction.Y * tt[0,0];
             

            return new XYZ(newX, newY, 0);

        }


    }


    public class MATRIX
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

       
    public class LINE  
    { 
        public XYZ startPoint { get; set; }
        public XYZ endPoint { get; set; }
        public string Name { get; set; }
        public double Slope { get; set; }
        public double c { get; set; }
        public XYZ OriPoint { get; set; }
        public XYZ Direction { get; set; }
        public LINE(XYZ _startPoint, XYZ _endPoint)
        {
            this.startPoint = _startPoint;
            this.endPoint = _endPoint;
            this.Name = "";
            this.Slope = this.GetSlope();
            this.c = this.Slope == -1 ? 0 : this.startPoint.Y - this.Slope * this.startPoint.X;
            GetParaMeters();
        }
        public LINE(XYZ _startPoint, XYZ _endPoint, string NAME)
        {
            this.startPoint = _startPoint;
            this.endPoint = _endPoint;
            this.Name = NAME;
            this.Slope = this.GetSlope();
            this.c = this.Slope == -1 ? 0 : this.startPoint.Y - this.Slope * this.startPoint.X;
            GetParaMeters();
        }

        public void GetParaMeters()
        {
            double dx = this.endPoint.X - this.startPoint.X;
            double dy = this.endPoint.Y - this.startPoint.Y;
            double dz = this.endPoint.Z - this.startPoint.Z;
            this.OriPoint = new XYZ(this.startPoint.X, this.startPoint.Y, this.startPoint.Z);
            this.Direction = new XYZ(dx / this.GetLength(), dy / this.GetLength(), 0);  
        }


        public List<LINE> GetShiftLines(XYZ point,double DD,string CASE)
        { 
            LINE[] lines = GetShiftLines(DD);
            double LL1 = lines[0].GetDistanceFromPoint(point);
            double LL2 = lines[1].GetDistanceFromPoint(point);
            Dictionary<string, List<LINE>> allCase = new Dictionary<string, List<LINE>>();
            allCase["IN"] = LL1 > LL2 ? new List<LINE> { lines[1] } : new List<LINE> { lines[0] };
            allCase["OUT"] = LL1 > LL2 ? new List<LINE> { lines[0] } : new List<LINE> { lines[1] };
            allCase["BOTH"] = lines.ToList();
             

            return allCase[CASE];

        }


        public LINE[] GetShiftLines(double DD)
        { 
            if (this.Slope == -1)
            { 
                LINE line1 = new LINE(new XYZ(this.startPoint.X + DD, this.startPoint.Y, this.startPoint.Z), 
                                      new XYZ(this.endPoint.X + DD, this.endPoint.Y, this.endPoint.Z));
                LINE line2 = new LINE(new XYZ(this.startPoint.X - DD, this.startPoint.Y, this.startPoint.Z),
                                      new XYZ(this.endPoint.X - DD, this.endPoint.Y, this.endPoint.Z));
                return new LINE[] { line1, line2 };
            }
            else if (this.Slope == 0)
            {
                LINE line1 = new LINE(new XYZ(this.startPoint.X, this.startPoint.Y+DD, this.startPoint.Z),
                                      new XYZ(this.endPoint.X, this.endPoint.Y+DD, this.endPoint.Z));
                LINE line2 = new LINE(new XYZ(this.startPoint.X, this.startPoint.Y-DD, this.startPoint.Z),
                                      new XYZ(this.endPoint.X, this.endPoint.Y-DD, this.endPoint.Z));
                return new LINE[] { line1, line2 }; 
            }
            else
            { 
                double _m = -1 / this.Slope;
                double tmpData = DD / Math.Sqrt(_m * _m + 1);
                double newX1 = this.startPoint.X + tmpData;
                double newY1 = this.startPoint.Y + _m * tmpData;
                double newX2 = this.startPoint.X - tmpData;
                double newY2 = this.startPoint.Y - _m * tmpData;

                XYZ newStartPoint1 = new XYZ(newX1, newY1, 0);
                XYZ newStartPoint2 = new XYZ(newX2, newY2, 0);
                return new LINE[2] { getNewLine(newStartPoint1), getNewLine(newStartPoint2) };
            }  
        }


        private double GetDistanceFromPoint(XYZ targetPoint)
        {
            double m = this.Slope;
            double b = this.startPoint.Y - m * this.startPoint.X;
            double res = m == -1 ? Math.Abs(targetPoint.X - this.startPoint.X) :
                                   Math.Abs(m * targetPoint.X - targetPoint.Y + b) / Math.Sqrt(m * m + 1);
            return res;
        }

        private LINE getNewLine(XYZ point)
        {
            double m = this.Slope;
            double tmpData = 1 / Math.Sqrt(m * m + 1);
            double newX2 = point.X + this.GetLength() * tmpData;
            double newY2 = point.Y + m * this.GetLength() * tmpData;
            return new LINE(point, new XYZ(newX2, newY2, 0));  
        }

        public double GetSlope()
        {
            double Slope = 0;
            if (this.startPoint.Y == this.endPoint.Y)
            {
                Slope = 0;
            }
            else if (this.startPoint.X == this.endPoint.X)
            {
                Slope = -1;
            }
            else
            {
                Slope = (this.startPoint.Y - this.endPoint.Y) / (this.startPoint.X - this.endPoint.X);
            }
            return Slope;
        }

        public double GetLength()
        {
            return Math.Sqrt(
                (this.startPoint.X - this.endPoint.X) * (this.startPoint.X - this.endPoint.X) +
                (this.startPoint.Y - this.endPoint.Y) * (this.startPoint.Y - this.endPoint.Y) +
                (this.startPoint.Z - this.endPoint.Z) * (this.startPoint.Z - this.endPoint.Z)
                );
        }

        public double getSumOfCoordinate()
        {
            return this.startPoint.X + this.startPoint.Y + this.startPoint.Z + this.endPoint.X + this.endPoint.Y + this.endPoint.Z;
        }


        public string ShowPoint()
        {
            string startPoint = "(" + this.startPoint.X.ToString() + " , " + this.startPoint.Y.ToString() + " , " + this.startPoint.Z.ToString() + ")";
            string endPoint = "(" + this.endPoint.X.ToString() + " , " + this.endPoint.Y.ToString() + " , " + this.endPoint.Z.ToString() + ")";
            return startPoint + "   " + endPoint;  
        } 

    }



    











     





    public class XYZ
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public XYZ(double x, double y, double z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public void ShowRes()
        {

            Console.WriteLine(this.X.ToString() + " , " + this.Y.ToString() + " , " + this.Z.ToString());
        }

    }
}
