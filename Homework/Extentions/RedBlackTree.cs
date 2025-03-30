public enum NodeColor
{
    Red,
    Black
}

public class RedBlackTreeNode<T> where T : IComparable<T>
{
    public T Value { get; set; }
    public RedBlackTreeNode<T> Left { get; set; }
    public RedBlackTreeNode<T> Right { get; set; }
    public RedBlackTreeNode<T> Parent { get; set; }
    public NodeColor Color { get; set; }
    public int Size { get; set; }

    public RedBlackTreeNode(T value)
    {
        Value = value;
        Color = NodeColor.Red;
        Size = 1;
    }
}

public class RedBlackTree<T, T1> where T : IComparable<T> where T1 : IComparer<T>, new()
{
    private RedBlackTreeNode<T> root;
    private readonly RedBlackTreeNode<T> nil;
    private readonly T1 comparer;

    public RedBlackTree()
    {
        nil = new RedBlackTreeNode<T>(default(T))
        {
            Color = NodeColor.Black,
            Size = 0
        };
        root = nil;
        comparer = new T1();
    }

    private void UpdateSize(RedBlackTreeNode<T> node)
    {
        if (node != nil)
        {
            node.Size = 1 + node.Left.Size + node.Right.Size;
        }
    }

    private void LeftRotate(RedBlackTreeNode<T> x)
    {
        RedBlackTreeNode<T> y = x.Right;
        x.Right = y.Left;

        if (y.Left != nil)
        {
            y.Left.Parent = x;
        }

        y.Parent = x.Parent;

        if (x.Parent == nil)
        {
            root = y;
        }
        else if (x == x.Parent.Left)
        {
            x.Parent.Left = y;
        }
        else
        {
            x.Parent.Right = y;
        }

        y.Left = x;
        x.Parent = y;

        UpdateSize(x);
        UpdateSize(y);
    }

    private void RightRotate(RedBlackTreeNode<T> y)
    {
        RedBlackTreeNode<T> x = y.Left;
        y.Left = x.Right;

        if (x.Right != nil)
        {
            x.Right.Parent = y;
        }

        x.Parent = y.Parent;

        if (y.Parent == nil)
        {
            root = x;
        }
        else if (y == y.Parent.Right)
        {
            y.Parent.Right = x;
        }
        else
        {
            y.Parent.Left = x;
        }

        x.Right = y;
        y.Parent = x;

        UpdateSize(y);
        UpdateSize(x);
    }

    private void InsertFixup(RedBlackTreeNode<T> z)
    {
        while (z.Parent.Color == NodeColor.Red)
        {
            if (z.Parent == z.Parent.Parent.Left)
            {
                RedBlackTreeNode<T> y = z.Parent.Parent.Right;
                if (y.Color == NodeColor.Red)
                {
                    z.Parent.Color = NodeColor.Black;
                    y.Color = NodeColor.Black;
                    z.Parent.Parent.Color = NodeColor.Red;
                    z = z.Parent.Parent;
                }
                else
                {
                    if (z == z.Parent.Right)
                    {
                        z = z.Parent;
                        LeftRotate(z);
                    }
                    z.Parent.Color = NodeColor.Black;
                    z.Parent.Parent.Color = NodeColor.Red;
                    RightRotate(z.Parent.Parent);
                }
            }
            else
            {
                RedBlackTreeNode<T> y = z.Parent.Parent.Left;
                if (y.Color == NodeColor.Red)
                {
                    z.Parent.Color = NodeColor.Black;
                    y.Color = NodeColor.Black;
                    z.Parent.Parent.Color = NodeColor.Red;
                    z = z.Parent.Parent;
                }
                else
                {
                    if (z == z.Parent.Left)
                    {
                        z = z.Parent;
                        RightRotate(z);
                    }
                    z.Parent.Color = NodeColor.Black;
                    z.Parent.Parent.Color = NodeColor.Red;
                    LeftRotate(z.Parent.Parent);
                }
            }
        }
        root.Color = NodeColor.Black;
    }

    public void Insert(T value)
    {
        RedBlackTreeNode<T> z = new RedBlackTreeNode<T>(value);
        RedBlackTreeNode<T> y = nil;
        RedBlackTreeNode<T> x = root;

        while (x != nil)
        {
            y = x;
            x.Size++;
            if (comparer.Compare(z.Value, x.Value) < 0)
            {
                x = x.Left;
            }
            else
            {
                x = x.Right;
            }
        }

        z.Parent = y;
        if (y == nil)
        {
            root = z;
        }
        else if (comparer.Compare(z.Value, y.Value) < 0)
        {
            y.Left = z;
        }
        else
        {
            y.Right = z;
        }

        z.Left = nil;
        z.Right = nil;
        z.Color = NodeColor.Red;

        InsertFixup(z);
    }

    public RedBlackTreeNode<T> Search(T value)
    {
        RedBlackTreeNode<T> x = root;
        while (x != nil && comparer.Compare(x.Value, value) != 0)
        {
            if (comparer.Compare(value, x.Value) < 0)
            {
                x = x.Left;
            }
            else
            {
                x = x.Right;
            }
        }
        return x;
    }

    public int GetRank(T value)
    {
        int rank = 0;
        RedBlackTreeNode<T> x = root;
        while (x != nil)
        {
            int cmp = comparer.Compare(value, x.Value);
            if (cmp < 0)
            {
                x = x.Left;
            }
            else if (cmp > 0)
            {
                rank += 1 + x.Left.Size;
                x = x.Right;
            }
            else
            {
                rank += x.Left.Size;
                return rank;
            }
        }
        return -1;
    }

    public T GetByIndex(int k)
    {
        if (k < 0 || k >= root.Size)
        {
            throw new IndexOutOfRangeException();
        }
        return Select(root, k).Value;
    }

    private RedBlackTreeNode<T> Select(RedBlackTreeNode<T> x, int k)
    {
        int t = x.Left.Size;
        if (t > k)
        {
            return Select(x.Left, k);
        }
        else if (t < k)
        {
            return Select(x.Right, k - t - 1);
        }
        else
        {
            return x;
        }
    }

    private void Transplant(RedBlackTreeNode<T> u, RedBlackTreeNode<T> v)
    {
        if (u.Parent == nil)
        {
            root = v;
        }
        else if (u == u.Parent.Left)
        {
            u.Parent.Left = v;
        }
        else
        {
            u.Parent.Right = v;
        }
        v.Parent = u.Parent;
    }

    private void DeleteFixup(RedBlackTreeNode<T> x)
    {
        while (x != root && x.Color == NodeColor.Black)
        {
            if (x == x.Parent.Left)
            {
                RedBlackTreeNode<T> w = x.Parent.Right;
                if (w.Color == NodeColor.Red)
                {
                    w.Color = NodeColor.Black;
                    x.Parent.Color = NodeColor.Red;
                    LeftRotate(x.Parent);
                    w = x.Parent.Right;
                }
                if (w.Left.Color == NodeColor.Black && w.Right.Color == NodeColor.Black)
                {
                    w.Color = NodeColor.Red;
                    x = x.Parent;
                }
                else
                {
                    if (w.Right.Color == NodeColor.Black)
                    {
                        w.Left.Color = NodeColor.Black;
                        w.Color = NodeColor.Red;
                        RightRotate(w);
                        w = x.Parent.Right;
                    }
                    w.Color = x.Parent.Color;
                    x.Parent.Color = NodeColor.Black;
                    w.Right.Color = NodeColor.Black;
                    LeftRotate(x.Parent);
                    x = root;
                }
            }
            else
            {
                RedBlackTreeNode<T> w = x.Parent.Left;
                if (w.Color == NodeColor.Red)
                {
                    w.Color = NodeColor.Black;
                    x.Parent.Color = NodeColor.Red;
                    RightRotate(x.Parent);
                    w = x.Parent.Left;
                }
                if (w.Right.Color == NodeColor.Black && w.Left.Color == NodeColor.Black)
                {
                    w.Color = NodeColor.Red;
                    x = x.Parent;
                }
                else
                {
                    if (w.Left.Color == NodeColor.Black)
                    {
                        w.Right.Color = NodeColor.Black;
                        w.Color = NodeColor.Red;
                        LeftRotate(w);
                        w = x.Parent.Left;
                    }
                    w.Color = x.Parent.Color;
                    x.Parent.Color = NodeColor.Black;
                    w.Left.Color = NodeColor.Black;
                    RightRotate(x.Parent);
                    x = root;
                }
            }
        }
        x.Color = NodeColor.Black;
    }

    public void Delete(T value)
    {
        RedBlackTreeNode<T> z = Search(value);
        if (z == nil) return;

        RedBlackTreeNode<T> y = z;
        NodeColor yOriginalColor = y.Color;
        RedBlackTreeNode<T> x;

        if (z.Left == nil)
        {
            x = z.Right;
            Transplant(z, z.Right);
        }
        else if (z.Right == nil)
        {
            x = z.Left;
            Transplant(z, z.Left);
        }
        else
        {
            y = Minimum(z.Right);
            yOriginalColor = y.Color;
            x = y.Right;
            if (y.Parent == z)
            {
                x.Parent = y;
            }
            else
            {
                Transplant(y, y.Right);
                y.Right = z.Right;
                y.Right.Parent = y;
            }
            Transplant(z, y);
            y.Left = z.Left;
            y.Left.Parent = y;
            y.Color = z.Color;
        }

        RedBlackTreeNode<T> temp = x.Parent;
        while (temp != nil)
        {
            UpdateSize(temp);
            temp = temp.Parent;
        }

        if (yOriginalColor == NodeColor.Black)
        {
            DeleteFixup(x);
        }
    }

    private RedBlackTreeNode<T> Minimum(RedBlackTreeNode<T> node)
    {
        while (node.Left != nil)
        {
            node = node.Left;
        }
        return node;
    }

    public int Count => root.Size;

    public void BatchSave(IEnumerable<T> values)
    {
        foreach (var value in values)
        {
            var node = Search(value);
            if (node != nil)
            {
                Delete(node.Value);
                Insert(value);
            }
            else
            {
                Insert(value);
            }
        }
    }
}