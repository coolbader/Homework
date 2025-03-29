using Homework;
using Homework.Cusomer;
using System.Collections.Concurrent;
namespace Customer;

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
    /// 2.使用 CustomerDataComparer 是为了按照得分降序排序。
    /// 3.排名索引得到排名数据，在_customers中得到实际的数据。
    /// </summary>
    private readonly SortedSet<CustomerData> _rankedCustomers = new SortedSet<CustomerData>(new CustomerDataComparer());

    private readonly SortedDictionary<int,long> _rankedCustomersDic = new SortedDictionary<int, long>();

    private List<CustomerData> _cusomerIndex = new();
    private readonly object _lock = new();

    public CustomerData UpdateScore(long customerId, decimal score)
    {
        if (score < -1000 || score > 1000)
        {
            throw new ArgumentException("Score must be between -1000 and 1000");
        }

        lock (_lock)
        {
            if (_customers.TryGetValue(customerId, out var existingCustomer))
            {
                existingCustomer.Score += score;
                _customers.TryUpdate(customerId, existingCustomer, existingCustomer);
                UpdateRanksAndRtnRank(existingCustomer);
                return existingCustomer;
            }
            else
            {
                var newCustomer = new CustomerData(customerId, score);
                _customers.TryAdd(customerId, newCustomer);
                UpdateRanksAndRtnRank(newCustomer);
                return newCustomer;
            }
        }
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
        _rankedCustomersDic.Add(_rankedCustomers.GetSortedSetIndex(customer) + 1, customer.CustomerId);
        // Rank=索引+1,在取值的时候关联赋值
        //customer.Rank = _rankedCustomers.GetSortedSetIndex(customer) + 1;
        //重建查询索引

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
        lock (_lock)
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
    }

    public List<CustomerData> GetCustomersAroundCustomer(long customerId, int high, int low)
    {
        lock (_lock)
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
    }

}
