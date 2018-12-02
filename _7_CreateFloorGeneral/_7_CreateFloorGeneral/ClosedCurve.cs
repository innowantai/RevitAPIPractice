using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7_CreateFloorGeneral
{
    class ClosedCurve
    {
        public List<Geometry> Data { get; set; }

         
        public ClosedCurve()
        {
            this.Data = new List<Geometry>();
        }

        public void Add(Geometry input)
        {
            this.Data.Add(input);
        }

        public Geometry GetLastItem()
        {
            return this.Data[this.Data.Count - 1];
        }

        public bool IsClosed()
        {
            return Math.Round((this.Data[0].startPoint - this.Data[this.Data.Count - 1].endPoint).GetLength(),4) == 0 ? true : false;
        }
         
        public int GetCount()
        {
            return this.Data.Count();
        } 
    }
}
