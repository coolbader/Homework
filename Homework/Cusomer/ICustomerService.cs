using Homework;

namespace Homework
{
    public interface ICustomerService
    {
      
        int GetCustomerCount();
        List<CustomerVO> GetCustomersAroundCustomer(long customerId, int high, int low);
        List<CustomerVO> GetCustomersByRank(int start, int end);
        List<CustomerData> GetTheSame();
        CustomerVO UpdateScore(long customerId, decimal score);
    }
}