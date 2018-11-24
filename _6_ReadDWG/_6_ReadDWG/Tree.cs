using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _6_ReadDWG
{
    class Tree
    {
        public int parent { get; set; }
        public List<int> children { get; set; }
        private int Layer { get; set; }
        private Dictionary<int, Tree> All_children;
        public Tree(int parent_, List<int> children_)
        {
            this.Layer = 0;
            this.parent = parent_;
            this.children = children_;
        } 


        public void Add()
        {
            this.Layer++;
        }



        
    }
}
