using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Homework
{
    public class CustomerService
    {
        /// <summary>
        /// 用于存储所有客户<客户ID，客户数据>
        /// </summary>
        private readonly ConcurrentDictionary<long, CustomerData> _customers = new();

        private readonly RankedCustomersManager _rankedCustomersManager;
        private readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();

        public CustomerService(RankedCustomersManager rankedCustomersManager)
        {
            _rankedCustomersManager = rankedCustomersManager;
        }

        public void UpdateScore(long customerId, decimal score)
        {
            if (score < -1000 || score > 1000)
            {
                throw new ArgumentException("Score must be between -1000 and 1000");
            }
            _customers.AddOrUpdate(customerId,
            id =>
            {
                var newCustomer = new CustomerData(customerId, score);
                StaticVariable._queue.Enqueue(newCustomer);
                return newCustomer;
            },
            (id, existingCustomer) =>
            {
                var updated = new CustomerData(customerId, existingCustomer.Score + score);
                StaticVariable._queue.Enqueue(updated);
                return updated;
            });

        }

        /// <summary>
        /// 根据排名范围获取客户信息
        /// </summary>
        /// <param name="start">起始排名，如果存在则包含在响应中</param>
        /// <param name="end">结束排名，如果存在则包含在响应中</param>
        /// <returns>客户信息列表</returns>
        /// <exception cref="ArgumentException">排名范围无效时抛出</exception>
        public List<CustomerVO> GetCustomersByRank(int start, int end)
        {
            if (start < 1 || end < start)
            {
                throw new ArgumentException("Invalid rank range");
            }

            _rwLock.EnterReadLock();
            try
            {
                var result = new List<CustomerVO>();
                for (int rank = start; rank <= end; rank++)
                {
                    if (_rankedCustomersManager.Count < rank)
                    {
                        break;
                    }
                    var customer = _rankedCustomersManager.GetByIndex(rank - 1);
                    if (customer != null)
                    {
                        var customervo = new CustomerVO(customer, rank);
                        result.Add(customervo);
                    }
                }
                return result;
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
        }

        /// <summary>
        /// 获取指定客户周围的客户信息
        /// </summary>
        /// <param name="customerId">指定客户的ID</param>
        /// <param name="high">指定客户上方的排名数量</param>
        /// <param name="low">指定客户下方的排名数量</param>
        /// <returns>客户信息列表</returns>
        /// <exception cref="KeyNotFoundException">指定客户不存在时抛出</exception>
        public List<CustomerVO> GetCustomersAroundCustomer(long customerId, int high, int low)
        {
            _rwLock.EnterReadLock();
            try
            {
                if (!_customers.TryGetValue(customerId, out var targetCustomer))
                {
                    throw new KeyNotFoundException($"CustomerData {customerId} not found");
                }

                var rankindex = _rankedCustomersManager.GetIndex(targetCustomer);
                var result = new List<CustomerVO>();

                if (high > 0)
                {
                    for (int i = 1; i <= high; i++)
                    {
                        var highindex = rankindex - i;
                        if (highindex < 0)
                        {
                            break;
                        }
                        var highrankCustomer = _rankedCustomersManager.GetByIndex(highindex);
                        if (highrankCustomer != null)
                        {
                            var highcustomer = new CustomerVO(highrankCustomer, highindex + 1);
                            result.Add(highcustomer);
                        }
                    }
                }

                var nowcustomer = new CustomerVO(targetCustomer, rankindex + 1);
                result.Add(nowcustomer);

                if (low > 0)
                {
                    for (int i = 1; i <= low; i++)
                    {
                        var lowindex = rankindex + i;
                        if (lowindex >= _rankedCustomersManager.Count)
                        {
                            break;
                        }

                        var lowrankCustomer = _rankedCustomersManager.GetByIndex(lowindex);
                        if (lowrankCustomer != null)
                        {
                            var lowcustomer = new CustomerVO(lowrankCustomer, lowindex + 1);
                            result.Add(lowcustomer);
                        }
                    }
                }

                return result;
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
        }

        public int GetCustomerCount()
        {
            return _rankedCustomersManager.Count;
        }

        /// <summary>
        /// 获取相同分数的客户信息
        /// 此方法目前未实现具体逻辑
        /// </summary>
        /// <returns>相同分数的客户信息列表</returns>
        public List<CustomerData> GetTheSame()
        {
            // 实现思路：遍历所有客户，根据分数进行分组，返回分数相同的客户列表
            // 示例代码：
            // var groupedCustomers = _customers.Values.GroupBy(c => c.Score);
            // var sameScoreCustomers = groupedCustomers.Where(g => g.Count() > 1).SelectMany(g => g).ToList();
            // return sameScoreCustomers;
            return null;
        }

        internal int? GetCusomerCount()
        {
            return _customers.Count;
        }

        internal int? GetRankCount()
        {
            return _rankedCustomersManager.Count;
        }

        internal string DataAccuracy()
        {
            StringBuilder result = new StringBuilder();
            if (_customers.Count == _rankedCustomersManager.Count)
            {
                result.AppendLine($"customersCount=RankCount -{_customers.Count}-{_rankedCustomersManager.Count}");
            }
            else
            {
                result.AppendLine($"customersCount<>RankCount -{_customers.Count}-{_rankedCustomersManager.Count}");
            }
            foreach (var customer in _customers) {
                if (_rankedCustomersManager.Exist(customer.Value))
                {
                    result.AppendLine($"Exist {customer.Value.CustomerId},score:{customer.Value.Score}");
                }
                else
                {
                    result.AppendLine($"-------------Not Exist {customer.Value.CustomerId},score:{customer.Value.Score}");
                }
            }
            return result.ToString();
        }
    }
}