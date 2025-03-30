using global::Customer;
using Homework.Extentions;
using System.Collections.Concurrent;
using System.Threading;
namespace Homework
{

    
    public class CustomerRBTService
    {
        /// <summary>
        /// 用于存储所有客户<客户ID，客户数据>
        /// </summary>
        private readonly ConcurrentDictionary<long, CustomerData> _customers = new();

        private readonly RedBlackTree<CustomerData> _rankedCustomers = new RedBlackTree<CustomerData>();
        private readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();

        public CustomerVO UpdateScore(long customerId, decimal score)
        {
            if (score < -1000 || score > 1000)
            {
                throw new ArgumentException("Score must be between -1000 and 1000");
            }
            CustomerData resultcustomer = null;
            int rankIndex = 0;
            _rwLock.EnterUpgradeableReadLock();
            try
            {
                if (_customers.TryGetValue(customerId, out var existingCustomer))
                {
                    _rwLock.EnterWriteLock();
                    try
                    {
                        var updated = new CustomerData(customerId, existingCustomer.Score + score);
                        _rankedCustomers.Delete(existingCustomer);
                        _rankedCustomers.Insert(updated);
                        _customers.TryUpdate(customerId, updated, existingCustomer);
                        rankIndex = _rankedCustomers.GetRank(updated);
                        resultcustomer = updated;
                    }
                    finally { _rwLock.ExitWriteLock(); }
                }
                else
                {
                    _rwLock.EnterWriteLock();
                    try
                    {
                        var newCustomer = new CustomerData(customerId, score);
                        _customers.TryAdd(customerId, newCustomer);
                        _rankedCustomers.Insert(newCustomer);
                        rankIndex = _rankedCustomers.GetRank(newCustomer);
                        resultcustomer = newCustomer;
                    }
                    finally
                    {
                        _rwLock.ExitWriteLock();
                    }
                }
            }
            finally { _rwLock.ExitUpgradeableReadLock(); }
            return new CustomerVO(resultcustomer, rankIndex + 1);
        }

        /// <summary>
        /// - {start}：
        /// - {end}：
        /// </summary>
        /// <param name="start">起始排名，如果存在则包含在响应中</param>
        /// <param name="end">结束排名，如果存在则包含在响应中</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public List<CustomerVO> GetCustomersByRank(int start, int end)
        {
            _rwLock.EnterReadLock();
            try
            {
                if (start < 1 || end < start)
                {
                    throw new ArgumentException("Invalid rank range");
                }

                var result = new List<CustomerVO>();
                for (int rank = start; rank <= end; rank++)
                {
                    if (_rankedCustomers.Count < rank)
                    {
                        break;
                    }
                    var customer = _rankedCustomers.Select(rank - 1);
                    var customervo = new CustomerVO(customer, rank);
                    result.Add(customervo);
                }
                return result;
            }
            finally { _rwLock.ExitReadLock(); }
        }

        public List<CustomerVO> GetCustomersAroundCustomer(long customerId, int high, int low)
        {
            var result = new List<CustomerVO>();
            _rwLock.EnterReadLock();
            try
            {
                if (!_customers.TryGetValue(customerId, out var targetCustomer))
                {
                    throw new KeyNotFoundException($"CustomerData {customerId} not found");
                }

                var rankindex = _rankedCustomers.GetRank(targetCustomer);
                if (high > 0)
                {
                    for (int i = 1; i <= high; i++)
                    {
                        var highindex = rankindex - i;
                        if (highindex < 0)
                        {
                            break;
                        }

                        var highrankCustomer = _rankedCustomers.Select(highindex);
                        var highcustomer = new CustomerVO(highrankCustomer, highindex + 1);
                        result.Add(highcustomer);
                    }
                }
                var nowcustomer = new CustomerVO(targetCustomer, rankindex + 1);
                result.Add(nowcustomer);
                if (low > 0)
                {
                    for (int i = 1; i <= low; i++)
                    {
                        var lowindex = rankindex + i;
                        if (lowindex >= _rankedCustomers.Count)
                        {
                            break;
                        }

                        var lowrankCustomer = _rankedCustomers.Select(lowindex);
                        var lowcustomer = new CustomerVO(lowrankCustomer, lowindex + 1);
                        result.Add(lowcustomer);
                    }
                }
            }
            finally { _rwLock.ExitReadLock(); }
            return result;
        }

        public int GetCustomerCount()
        {
            return _rankedCustomers.Count;
        }

        public List<CustomerData> GetTheSame()
        {
            // 这里需要实现获取相同分数客户的逻辑
            throw new NotImplementedException();
        }
    }


}
