using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _6_ReadDWG
{
    public class VECTOR
    {

        public double X;
        public double Y;
        public double Z;

        public VECTOR(double x, double y, double z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public VECTOR Cross(VECTOR Point2)
        {
            return new VECTOR(this.Y * Point2.Z - Point2.Y * this.Z,
                           -this.X * Point2.Z + Point2.X * this.Z,
                           this.X * Point2.Y - Point2.X * this.Y);
        }

        public double Dot(VECTOR Point2)
        { 
            return (this.X * Point2.X +this.Y * Point2.Y + this.Z * Point2.Z);
        }

        public double DotAngle(VECTOR Point2)
        {
            return Math.Acos(this.Dot(Point2) / (this.GetLen() * Point2.GetLen()));
        }



        public VECTOR UnitForm()
        {
            double Len = this.GetLen();
            return new VECTOR(this.X / Len, this.Y / Len, this.Z / Len);
        }


        public double GetLen()
        {
            return Math.Sqrt((this.X) * (this.X) + (this.Y) * (this.Y) + (this.Z) * (this.Z));
        }

        public static VECTOR operator +(VECTOR a, VECTOR b)
        {
            return new VECTOR(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }


        public static VECTOR operator -(VECTOR a, VECTOR b)
        {
            return new VECTOR(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }


        public static VECTOR operator /(VECTOR a, double b)
        {
            return new VECTOR(a.X / b, a.Y / b, a.Z / b);
        }
    }
}
