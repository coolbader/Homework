namespace Customer;

public class CustomerData: IComparable<CustomerData>
{
    public long CustomerId { get; set; }
    public decimal Score { get; set; }
    public int Rank { get; set; }

    public CustomerData(long customerId, decimal score)
    {
        CustomerId = customerId;
        Score = score;
    }

    public int CompareTo(CustomerData? other)
    {
        if (other == null) return 1;
        // 先按 Score 排序，若 Score 相同，则按 CustomerId 排序
        int scoreComparison = other.Score.CompareTo(this.Score);  // 降序
        if (scoreComparison == 0)
        {
            return this.CustomerId.CompareTo(other.CustomerId);  // 升序
        }
        return scoreComparison;
    }
}