using Customer;
using System;
using System.Collections.Immutable;
using System.Reflection.Metadata.Ecma335;

namespace Homework
{
    public class CustomerDataComparer :  IComparer<CustomerData>
    {
        public int Compare(CustomerData x, CustomerData y)
        {
            // 按照分数降序排序
            int scoreComparison = y.Score.CompareTo(x.Score);
            if (scoreComparison != 0)
            {
                return scoreComparison;
            }
            // 分数相同，按 CustomerId 升序排序
            return x.CustomerId.CompareTo(y.CustomerId);
        }
    }
}
