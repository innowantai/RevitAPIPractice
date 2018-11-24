using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tree
{
    public class LinkedTree<T> : Tree<T>
    {
        protected LinkedList<LinkedTree<T>> childrenList;

        protected LinkedTree<T> parent;
        public override Tree<T> Parent
        {
            get
            {
                return parent;
            }
        }

        protected LinkedTreeList<T> children;
        public override TreeList<T> Children
        {
            get
            {
                return children;
            }
        }

        public override int Degree
        {
            get
            {
                return childrenList.Count;
            }
        }

        protected int count;
        public override int Count
        {
            get
            {
                return count;
            }
        }

        protected int depth;
        public override int Depth
        {
            get
            {
                return depth;
            }
        }

        protected int level;
        public override int Level
        {
            get
            {
                return level;
            }
        }

        public LinkedTree(T value)
            : base(value)
        {
            childrenList = new LinkedList<LinkedTree<T>>();
            children = new LinkedTreeList<T>(childrenList);
            depth = 1;
            level = 1;
            count = 1;
        }

        public override void Add(T value)
        {
            Add(new LinkedTree<T>(value));
        }

        public override void Add(Tree<T> tree)
        {
            LinkedTree<T> gtree = (LinkedTree<T>)tree;
            if (gtree.Parent != null)
                gtree.Remove();
            gtree.parent = this;
            if (gtree.depth + 1 > depth)
            {
                depth = gtree.depth + 1;
                BubbleDepth();
            }
            gtree.level = level + 1;
            gtree.UpdateLevel();
            childrenList.AddLast(gtree);
            count += tree.Count;
            BubbleCount(tree.Count);
        }

        public override void Remove()
        {
            if (parent == null)
                return;
            parent.childrenList.Remove(this);
            if (depth + 1 == parent.depth)
                parent.UpdateDepth();
            parent.count -= count;
            parent.BubbleCount(-count);
            parent = null;
        }

        public override Tree<T> Clone()
        {
            return Clone(1);
        }

        protected LinkedTree<T> Clone(int level)
        {
            LinkedTree<T> cloneTree = new LinkedTree<T>(Value);
            cloneTree.depth = depth;
            cloneTree.level = level;
            cloneTree.count = count;
            foreach (LinkedTree<T> child in childrenList)
            {
                LinkedTree<T> cloneChild = child.Clone(level + 1);
                cloneChild.parent = cloneTree;
                cloneTree.childrenList.AddLast(cloneChild);
            }
            return cloneTree;
        }

        protected void BubbleDepth()
        {
            if (parent == null)
                return;

            if (depth + 1 > parent.depth)
            {
                parent.depth = depth + 1;
                parent.BubbleDepth();
            }
        }

        protected void UpdateDepth()
        {
            int tmpDepth = depth;
            depth = 1;
            foreach (LinkedTree<T> child in childrenList)
                if (child.depth + 1 > depth)
                    depth = child.depth + 1;
            if (tmpDepth == depth || parent == null)
                return;
            if (tmpDepth + 1 == parent.depth)
                parent.UpdateDepth();
        }

        protected void BubbleCount(int diff)
        {
            if (parent == null)
                return;

            parent.count += diff;
            parent.BubbleCount(diff);
        }

        protected void UpdateLevel()
        {
            int childLevel = level + 1;
            foreach (LinkedTree<T> child in childrenList)
            {
                child.level = childLevel;
                child.UpdateLevel();
            }
        }
    }

    public class LinkedTreeList<T> : TreeList<T>
    {
        protected LinkedList<LinkedTree<T>> list;

        public LinkedTreeList(LinkedList<LinkedTree<T>> list)
        {
            this.list = list;
        }

        public override int Count
        {
            get
            {
                return list.Count;
            }
        }

        public override IEnumerator<Tree<T>> GetEnumerator()
        {
            return list.GetEnumerator();
        }
    }

    public abstract class Tree<T>
    {
        public T Value { get; set; }

        public abstract Tree<T> Parent { get; }
        public abstract TreeList<T> Children { get; }
        public abstract int Count { get; }
        public abstract int Degree { get; }
        public abstract int Depth { get; }
        public abstract int Level { get; }

        public Tree(T value)
        {
            this.Value = value;
        }

        public abstract void Add(T value);
        public abstract void Add(Tree<T> tree);
        public abstract void Remove();
        public abstract Tree<T> Clone();
    }

    public abstract class TreeList<T> : IEnumerable<Tree<T>>
    {
        public abstract int Count { get; }
        public abstract IEnumerator<Tree<T>> GetEnumerator();

        IEnumerator<Tree<T>> IEnumerable<Tree<T>>.GetEnumerator()
        {
            return GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
