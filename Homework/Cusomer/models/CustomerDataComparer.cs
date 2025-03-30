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
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            int scoreComparison = y.Score.CompareTo(x.Score);
            if (scoreComparison != 0)
            {
                return scoreComparison;
            }
            return x.CustomerId.CompareTo(y.CustomerId);
        }
    }
}
