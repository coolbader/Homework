using Homework;
using Homework.Cusomer;
using System.Collections.Concurrent;
namespace Customer;
/// <summary>
/// 采用字典存储客户数据，采用红黑树存储积分并排序,排名按照索引实时计算。
/// 缺点：
/// 1.新增红黑树数据O(1),更新红黑树先删除在新增。O（N）+O(1).
/// 2.红黑树对分数和客户ID进行排序，建立索引链表用于排名。重建链表会消耗O（N）
/// 3.获取索引的过程可能会消耗O（N），导致实际更新较慢。
/// 测试：
/// 实际测试10001条数据预计要3秒。
/// 总结：延迟重建索引，比如每隔5秒更新排行，但与需求不符。
/// </summary>
[Obsolete]
public class CustomerObsoleteService
{
    /// <summary>
    /// 用于存储所有客户<客户ID，客户数据>
    /// </summary>
    private readonly ConcurrentDictionary<long, CustomerData> _customers = new();
    /// <summary>
    /// 使用红黑树用于存储客户排名,仅作为计算排名使用。
    /// 1.使用 SortedSet 是为了对客户得分进行排序。
    /// 2.排名索引得到排名数据，在_customers中得到实际的数据。
    /// </summary>
    private readonly SortedSet<CustomerData> _rankedCustomers = new SortedSet<CustomerData>(new CustomerDataComparer());

    private List<CustomerData> _cusomerIndex = new();
    private readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();
    public CustomerData UpdateScore(long customerId, decimal score)
    {
        if (score < -1000 || score > 1000)
        {
            throw new ArgumentException("Score must be between -1000 and 1000");
        }

        _rwLock.EnterUpgradeableReadLock();
        try
        {
            if (_customers.TryGetValue(customerId, out var existingCustomer))
            {
                _rwLock.EnterReadLock();
                try
                {
                    var updated = new CustomerData(customerId, existingCustomer.Score + score);
                    if (_rankedCustomers.Remove(existingCustomer))
                    {
                        _rankedCustomers.Add(updated);
                        _customers.TryUpdate(customerId, updated, existingCustomer);
                        return updated;
                    }
                    return existingCustomer;
                }
                finally { _rwLock.ExitWriteLock(); }
            }
            else
            {
                _rwLock.EnterReadLock();
                try
                {
                    var newCustomer = new CustomerData(customerId, score);
                    _customers.TryAdd(customerId, newCustomer);
                    UpdateRanksAndRtnRank(newCustomer);
                    return newCustomer;
                }
                finally
                {

                    _rwLock.ExitReadLock();
                }
            }
        }
        finally { _rwLock.ExitUpgradeableReadLock(); }
    }
    /// <summary>
    /// 1.添加新客户到排名列表
    /// 2.排名=排名列表索引+1
    /// 线程不安全，需要在调用处加锁
    /// </summary>
    /// <param name="customer"></param>
    /// <returns></returns>
    private void UpdateRanksAndRtnRank(CustomerData customer)
    {
        // 移除旧排名
        if (_customers.TryGetValue(customer.CustomerId, out var existingCustomer))
        {
            _rankedCustomers.Remove(existingCustomer);
        }
        // 添加进进去，重新计算排名
        _rankedCustomers.Add(customer);
        RebuildIndex();
    }

    private void RebuildIndex()
    {
        _cusomerIndex = _rankedCustomers.ToList();
    }



    /// <summary>
    /// - {start}：
    /// - {end}：
    /// </summary>
    /// <param name="start">起始排名，如果存在则包含在响应中</param>
    /// <param name="end">结束排名，如果存在则包含在响应中</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public List<CustomerData> GetCustomersByRank(int start, int end)
    {
        _rwLock.EnterReadLock();
        try
        {
            if (start < 1 || end < start)
            {
                throw new ArgumentException("Invalid rank range");
            }

            var result = new List<CustomerData>();
            for (int rank = start; rank <= end; rank++)
            {
                if (_cusomerIndex.Count < rank)
                {
                    break;
                }
                _cusomerIndex[rank - 1].Rank = _rankedCustomers.GetSortedSetIndex(_cusomerIndex[rank - 1]) + 1;
                result.Add(_cusomerIndex[rank - 1]);
            }
            return result;
        }
        finally { _rwLock.ExitReadLock(); }
    }

    public List<CustomerData> GetCustomersAroundCustomer(long customerId, int high, int low)
    {
        _rwLock.EnterReadLock();
        try
        {
            if (!_customers.TryGetValue(customerId, out var targetCustomer))
            {
                throw new KeyNotFoundException($"CustomerData {customerId} not found");
            }
            var result = new List<CustomerData>();
            targetCustomer.Rank = _rankedCustomers.GetSortedSetIndex(targetCustomer) + 1;
            if (low > 0)
            {
                for (int i = 1; i <= low; i++)
                {
                    var lowindex = targetCustomer.Rank - 1 - i;
                    if (lowindex >= 0)
                    {
                        _cusomerIndex[lowindex].Rank = _rankedCustomers.GetSortedSetIndex(_cusomerIndex[lowindex]) + 1;
                        result.Add(_cusomerIndex[lowindex]);
                    }

                }
            }

            result.Add(targetCustomer);
            if (high > 0)
            {
                for (int i = 1; i <= high; i++)
                {
                    var highindex = targetCustomer.Rank - 1 + i;
                    if (highindex <= _cusomerIndex.Count)
                    {
                        _cusomerIndex[highindex].Rank = _rankedCustomers.GetSortedSetIndex(_cusomerIndex[highindex]) + 1;
                        result.Add(_cusomerIndex[highindex]);
                    }
                }
            }
            return result;

        }
        finally { _rwLock.ExitReadLock(); }
    }
}


