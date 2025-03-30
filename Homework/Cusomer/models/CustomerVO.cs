using Customer;

namespace Homework
{
    public class CustomerVO
    {
        public CustomerVO(CustomerData data,int rank)
        {
            this.CustomerId = data.CustomerId;
            this.Score= data.Score;
            this.Rank = rank;
        }
        public CustomerVO() { }
        public long CustomerId { get; set; }
        public decimal Score { get; set; }
        public int Rank { get; set; }
    }
}
