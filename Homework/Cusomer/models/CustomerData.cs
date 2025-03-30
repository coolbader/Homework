namespace Homework;

public class CustomerData : IComparable<CustomerData>, IEquatable<CustomerData>
{
   
    public long CustomerId { get; set; }
    public decimal Score { get; set; }

    public CustomerData(long customerId, decimal score)
    {
        CustomerId = customerId;
        Score = score;
    }

    public int CompareTo(CustomerData? other)
    {
        if (other == null) return 1;
        // 先按 Score 降序，Score 相同则按 CustomerId 升序
        int scoreComparison = other.Score.CompareTo(this.Score);  // 降序
        if (scoreComparison == 0)
        {
            return this.CustomerId.CompareTo(other.CustomerId);  // 升序
        }
        return scoreComparison;
    }

    // 相等性方法
    public bool Equals(CustomerData? other)
    {
        if (other == null) return false;
        return this.CustomerId == other.CustomerId ;
    }

    public override bool Equals(object? obj)
    {
        return obj is CustomerData other && Equals(other);
    }
    /// <summary>
    /// 比较hash
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
        return CustomerId.GetHashCode() ^ Score.GetHashCode();
    }
}