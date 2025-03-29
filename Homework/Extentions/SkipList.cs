namespace Homework;
public class SkipListNode<T>
{
    public T Value;
    public SkipListNode<T>[] Forward;

    public SkipListNode(int level, T value)
    {
        Forward = new SkipListNode<T>[level + 1];
        Value = value;
    }
}

public class SkipList<T> where T : IComparable<T>
{
    private readonly Random rand = new();
    private readonly int maxLevel = 16;  // 允许的最大层数
    private int level;
    private readonly SkipListNode<T> head;
    private readonly IComparer<T> comparer;  // 添加比较器

    public int Count { get; private set; }

    // 修改构造函数，接受比较器
    public SkipList(IComparer<T> comparer)
    {
        this.comparer = comparer ?? Comparer<T>.Default;  // 默认比较器
        head = new SkipListNode<T>(maxLevel, default);
        level = 0;
        Count = 0;
    }

    // 生成随机层级（几率降低）
    private int RandomLevel()
    {
        int lvl = 0;
        while (rand.Next(2) == 1 && lvl < maxLevel) lvl++;
        return lvl;
    }

    // 插入值（O(log N)）
    public void Add(T value)
    {
        SkipListNode<T>[] update = new SkipListNode<T>[maxLevel + 1];
        SkipListNode<T> current = head;

        for (int i = level; i >= 0; i--)
        {
            while (current.Forward[i] != null && comparer.Compare(current.Forward[i].Value, value) < 0)
                current = current.Forward[i];
            update[i] = current;
        }

        int newLevel = RandomLevel();
        if (newLevel > level)
        {
            for (int i = level + 1; i <= newLevel; i++)
                update[i] = head;
            level = newLevel;
        }

        SkipListNode<T> newNode = new SkipListNode<T>(newLevel, value);
        for (int i = 0; i <= newLevel; i++)
        {
            newNode.Forward[i] = update[i].Forward[i];
            update[i].Forward[i] = newNode;
        }

        Count++;
    }

    // 查找值是否存在（O(log N)）
    public bool Contains(T value)
    {
        SkipListNode<T> current = head;
        for (int i = level; i >= 0; i--)
        {
            while (current.Forward[i] != null && comparer.Compare(current.Forward[i].Value, value) < 0)
                current = current.Forward[i];
        }
        current = current.Forward[0];
        return current != null && comparer.Compare(current.Value, value) == 0;
    }

    // 删除值（O(log N)）
    public bool Remove(T value)
    {
        SkipListNode<T>[] update = new SkipListNode<T>[maxLevel + 1];
        SkipListNode<T> current = head;

        for (int i = level; i >= 0; i--)
        {
            while (current.Forward[i] != null && comparer.Compare(current.Forward[i].Value, value) < 0)
                current = current.Forward[i];
            update[i] = current;
        }

        current = current.Forward[0];
        if (current != null && comparer.Compare(current.Value, value) == 0)
        {
            for (int i = 0; i <= level; i++)
            {
                if (update[i].Forward[i] != current) break;
                update[i].Forward[i] = current.Forward[i];
            }

            while (level > 0 && head.Forward[level] == null)
                level--;

            Count--;
            return true;
        }
        return false;
    }

    // 获取索引（O(log N)）
    public int GetIndex(T value)
    {
        SkipListNode<T> current = head;
        int index = 0;

        for (int i = level; i >= 0; i--)
        {
            while (current.Forward[i] != null && comparer.Compare(current.Forward[i].Value, value) < 0)
            {
                index += (1 << i);  // 累加步长
                current = current.Forward[i];
            }
        }

        current = current.Forward[0];
        return (current != null && comparer.Compare(current.Value, value) == 0) ? index : -1;
    }

    // 按索引获取值（O(log N)）
    public T GetByIndex(int index)
    {
        if (index < 0 || index >= Count) throw new IndexOutOfRangeException();
        SkipListNode<T> current = head;
        int pos = -1;

        for (int i = level; i >= 0; i--)
        {
            while (current.Forward[i] != null && pos + (1 << i) < index)
            {
                pos += (1 << i);
                current = current.Forward[i];
            }
        }

        return current.Forward[0].Value;
    }

    // 更新值（O(log N)）
    public bool Update(T oldValue, T newValue)
    {
        if (Remove(oldValue))
        {
            Add(newValue);
            return true;
        }
        return false;
    }

    // 打印整个跳表（调试用）
    public void Print()
    {
        for (int i = level; i >= 0; i--)
        {
            SkipListNode<T> node = head.Forward[i];
            Console.Write($"Level {i}: ");
            while (node != null)
            {
                Console.Write($"{node.Value} -> ");
                node = node.Forward[i];
            }
            Console.WriteLine("null");
        }
    }
}