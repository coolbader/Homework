namespace Homework
{
    /// <summary>
    /// 只做ID比较，是否同一个客户
    /// </summary>
    public class CustomerIdComparer : IComparer<CustomerData>
    {
        public int Compare(CustomerData x, CustomerData y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            return x.CustomerId.CompareTo(y.CustomerId);
        }
    }
}
