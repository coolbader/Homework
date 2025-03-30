using Customer;

namespace Homework
{
    public class CustomerRankManager
    {
        /// <summary>
        /// 使用红黑树用于存储客户排名,仅作为计算排名使用。
        /// 1.使用 SortedSet 是为了对客户得分进行排序。
        /// 2.排名索引得到排名数据，在_customers中得到实际的数据。
        /// </summary>
        private readonly SortedSet<CustomerData> _rankedCustomers = new SortedSet<CustomerData>(new CustomerDataComparer());

        public int Count=> _rankedCustomers.Count;
        // 添加 CustomerData
        public void AddCustomer(CustomerData customer)
        {
            _rankedCustomers.Add(customer);
        }
       
        // 删除 CustomerData
        public bool RemoveCustomer(CustomerData customer)
        {
            return _rankedCustomers.Remove(customer);
        }

        // 根据 CustomerData 获取当前排序位置
        public int GetRank(CustomerData customer)
        {
            int rank = 0;
            foreach (var c in _rankedCustomers)
            {
                if (c.Equals(customer))
                {
                    return rank;
                }
                rank++;
            }
            return -1; // 未找到
        }

        // 根据索引获取 CustomerData
        public CustomerData? GetIndex(int index)
        {
            if (index < 0 || index >= _rankedCustomers.Count)
            {
                return null;
            }
            int currentIndex = 0;
            foreach (var customer in _rankedCustomers)
            {
                if (currentIndex == index)
                {
                    return customer;
                }
                currentIndex++;
            }
            return null;
        }
        /// <summary>
        /// 测试使用，获取相同的客户ID
        /// </summary>
        /// <returns></returns>
        public List<CustomerData> GetTheSame()
        {
            return _rankedCustomers.GroupBy(a => a.CustomerId).Where(a => a.Count() > 1).SelectMany(a => a).ToList();
        }
    }

}
