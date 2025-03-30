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
        // �Ȱ� Score ����Score ��ͬ�� CustomerId ����
        int scoreComparison = other.Score.CompareTo(this.Score);  // ����
        if (scoreComparison == 0)
        {
            return this.CustomerId.CompareTo(other.CustomerId);  // ����
        }
        return scoreComparison;
    }

    // ����Է���
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
    /// �Ƚ�hash
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
        return CustomerId.GetHashCode() ^ Score.GetHashCode();
    }
}