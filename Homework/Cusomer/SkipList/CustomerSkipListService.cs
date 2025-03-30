using Homework;
using System.Collections.Concurrent;

namespace Homework
{

    /// <summary>
    /// 核心思路：
    /// 1.字典保存客户得分信息。
    /// 2.跳表排序，跳表里Index就是排名。O(log n).
    /// </summary>
    public class CustomerSkipListService : ICustomerService
    {
        /// <summary>
        ///  客户数据字典
        /// </summary>
        private readonly ConcurrentDictionary<long, CustomerData> _customers = new();
        /// <summary>
        ///  跳表
        /// </summary>
        private readonly SkipList<CustomerData> _rankedCustomers = new SkipList<CustomerData>(new CustomerDataComparer());

        /// <summary>
        ///  读写锁
        /// </summary>
        private static ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        private readonly object _lock = new object();

        public CustomerVO UpdateScore(long customerId, decimal score)
        {
            if (score < -1000 || score > 1000)
            {
                throw new ArgumentException("Score must be between -1000 and 1000");
            }
            CustomerData resultCustomer = null;
            int rank = 0;
            if (_customers.TryGetValue(customerId, out var existingCustomer))
            {
                _rwLock.EnterUpgradeableReadLock();
                try
                {
                    // 删除旧排名，修改为新排名
                    resultCustomer = new CustomerData(customerId, existingCustomer.Score + score);
                    _rankedCustomers.Update(existingCustomer, resultCustomer);
                    rank = _rankedCustomers.GetIndex(resultCustomer);
                    // 更新字典中的客户数据
                    _customers[customerId] = resultCustomer;
                }
                finally { _rwLock.ExitUpgradeableReadLock(); }
            }
            else
            {
                _rwLock.EnterWriteLock();
                try
                {
                    resultCustomer = new CustomerData(customerId, score);
                    _customers.TryAdd(customerId, resultCustomer);
                    _rankedCustomers.Add(resultCustomer);
                    rank = _rankedCustomers.GetIndex(resultCustomer);
                }
                finally { _rwLock.ExitWriteLock(); }

            }
            var result = new CustomerVO(resultCustomer, rank + 1);

            return result;
        }


        public List<CustomerVO> GetCustomersByRank(int start, int end)
        {
            if (start < 1 || end < start)
            {
                throw new ArgumentException("Invalid rank range");
            }
            var result = new List<CustomerVO>();
            _rwLock.EnterReadLock();
            try
            {
                for (int i = start - 1; i < end && i < _customers.Count; i++)
                {
                    var customer = _rankedCustomers.GetByIndex(i);
                    var vo = new CustomerVO(customer, i + 1);
                    result.Add(vo);
                }
                return result;
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
        }


        public List<CustomerVO> GetCustomersAroundCustomer(long customerId, int high, int low)
        {
            var result = new List<CustomerVO>();
            try
            {
                // 开始读锁
                _rwLock.EnterReadLock();

                if (!_customers.TryGetValue(customerId, out var targetCustomer))
                {
                    throw new KeyNotFoundException($"CustomerData {customerId} not found");
                }

                // 获取目标客户的index
                int targetRankIndex = _rankedCustomers.GetIndex(targetCustomer);
                if (targetRankIndex == -1)
                {
                    throw new InvalidOperationException($"CustomerData {customerId} not found in ranked list");
                }
                var selfcustomer = new CustomerVO(targetCustomer, targetRankIndex);
                // 开始获取目标客户的后high个客户数据
                if (high > 0)
                {
                    for (int i = 1; i <= high; i++)
                    {
                        int highIndex = targetRankIndex - i;
                        if (highIndex >= 0 && highIndex < _customers.Count)
                        {
                            var customer = _rankedCustomers.GetByIndex(highIndex);
                            var highvo = new CustomerVO(customer, highIndex + 1);
                            result.Add(highvo);
                        }
                    }
                }
                // 添加目标客户数据
                result.Add(selfcustomer);

                // 开始获取目标客户的前low个和后high个客户数据
                if (low > 0)
                {
                    for (int i = 1; i <= low; i++)
                    {
                        int lowIndex = targetRankIndex + i;
                        if (lowIndex >= 0 && lowIndex < _customers.Count)
                        {
                            var customer = _rankedCustomers.GetByIndex(lowIndex);
                            var lowvo = new CustomerVO(customer, lowIndex + 1);
                            result.Add(lowvo);
                        }
                    }
                }

            }
            finally
            {
                //  结束读锁
                if (_rwLock.IsReadLockHeld)
                    _rwLock.ExitReadLock();
            }
            return result;
        }

        /// <summary>
        ///  释放资源
        /// </summary>
        public void Dispose()
        {
            _rwLock?.Dispose();
        }

        public int GetCustomerCount()
        {
            return _rankedCustomers.Count();
        }

        public List<CustomerData> GetTheSame()
        {
            return _rankedCustomers.GroupBy(a => a.CustomerId).Where(g => g.Count() > 1)
            .SelectMany(g => g).ToList();

        }
    }
}