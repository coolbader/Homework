using Homework;
using Homework.Cusomer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Customer
{

    public class CustomerService
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
        private readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();

        private readonly object _lock = new object();

        public CustomerData UpdateScore(long customerId, decimal score)
        {
            if (score < -1000 || score > 1000)
            {
                throw new ArgumentException("Score must be between -1000 and 1000");
            }

            _rwLock.EnterWriteLock();
            try
            {
                if (_customers.TryGetValue(customerId, out var existingCustomer))
                {
                    //删除就排名
                    _rankedCustomers.Remove(existingCustomer);
                    existingCustomer.Score += score;
                    // 直接插入并获取新排名
                    _rankedCustomers.Add(existingCustomer);
                    return existingCustomer;
                }
                else
                {
                    var newCustomer = new CustomerData(customerId, score);
                    _customers.TryAdd(customerId, newCustomer);
                    _rankedCustomers.Add(newCustomer);
                    return newCustomer;
                }
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }


        public List<CustomerData> GetCustomersByRank(int start, int end)
        {
            if (start < 1 || end < start)
            {
                throw new ArgumentException("Invalid rank range");
            }
            var result = new List<CustomerData>();
            _rwLock.EnterReadLock();
            try
            {
              

                for (int i = start - 1; i < end && i < _customers.Count; i++)
                {
                    var customer = _rankedCustomers.GetByIndex(i);
                    //排名从1开始，+1
                    customer.Rank = i + 1;
                    result.Add(customer);
                }

                return result;
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
        }


        public List<CustomerData> GetCustomersAroundCustomer(long customerId, int high, int low)
        {
            var result = new List<CustomerData>();
            
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

                //排名从1开始，+1
                targetCustomer.Rank = targetRankIndex + 1;

                // 开始获取目标客户的后high个客户数据
                if (high > 0)
                {
                    for (int i = 1; i <= high; i++)
                    {
                        int highIndex = targetRankIndex - i;
                        if (highIndex >= 0 && highIndex < _customers.Count)
                        {
                            var customer = _rankedCustomers.GetByIndex(highIndex);
                            customer.Rank = highIndex + 1;
                            result.Add(customer);
                        }
                    }
                }
               

                // 添加目标客户数据
                result.Add(targetCustomer);

                // 开始获取目标客户的前low个和后high个客户数据
                if (low > 0)
                {
                    for (int i = 1; i <= low; i++)
                    {
                        int lowIndex = targetRankIndex + i;
                        if (lowIndex >= 0 && lowIndex < _customers.Count)
                        {
                            var customer = _rankedCustomers.GetByIndex(lowIndex);
                            //排名从1开始，+1
                            customer.Rank = lowIndex + 1;
                            result.Add(customer);
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
            return _rankedCustomers.Count;
        }
    }
}