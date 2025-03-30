using Homework;

namespace Homework
{
    public class RankedCustomersManager
    {
        private readonly RedBlackTree<CustomerData,CustomerIdComparer> _rankedCustomers = new RedBlackTree<CustomerData, CustomerIdComparer>();

        public void Insert(CustomerData customer)
        {
            _rankedCustomers.Insert(customer);
        }

        public void Delete(CustomerData customer)
        {
            _rankedCustomers.Delete(customer);
        }

        public int GetIndex(CustomerData customer)
        {
            return _rankedCustomers.GetRank(customer);
        }

        public CustomerData GetByIndex(int index)
        {
            return _rankedCustomers.GetByIndex(index);
        }
        public void BatchSave(IEnumerable<CustomerData> datas)
        {
             _rankedCustomers.BatchSave(datas);
        }
        public int Count => _rankedCustomers.Count;
    }
}
