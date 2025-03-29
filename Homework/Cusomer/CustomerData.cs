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
        // �Ȱ� Score ������ Score ��ͬ���� CustomerId ����
        int scoreComparison = other.Score.CompareTo(this.Score);  // ����
        if (scoreComparison == 0)
        {
            return this.CustomerId.CompareTo(other.CustomerId);  // ����
        }
        return scoreComparison;
    }
}