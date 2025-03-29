using Homework;
using Homework.Cusomer;
using System.Collections.Concurrent;
namespace Customer;

[Obsolete]
public class CustomerObsoleteService
{
    /// <summary>
    /// ���ڴ洢���пͻ�<�ͻ�ID���ͻ�����>
    /// </summary>
    private readonly ConcurrentDictionary<long, CustomerData> _customers = new();
    /// <summary>
    /// ʹ�ú�������ڴ洢�ͻ�����,����Ϊ��������ʹ�á�
    /// 1.ʹ�� SortedSet ��Ϊ�˶Կͻ��÷ֽ�������
    /// 2.ʹ�� CustomerDataComparer ��Ϊ�˰��յ÷ֽ�������
    /// 3.���������õ��������ݣ���_customers�еõ�ʵ�ʵ����ݡ�
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
    /// 1.����¿ͻ��������б�
    /// 2.����=�����б�����+1
    /// �̲߳���ȫ����Ҫ�ڵ��ô�����
    /// </summary>
    /// <param name="customer"></param>
    /// <returns></returns>
    private void UpdateRanksAndRtnRank(CustomerData customer)
    {
        // �Ƴ�������
        if (_customers.TryGetValue(customer.CustomerId, out var existingCustomer))
        {
            _rankedCustomers.Remove(existingCustomer);
        }
        // ��ӽ���ȥ�����¼�������
        _rankedCustomers.Add(customer);
        _rankedCustomersDic.Add(_rankedCustomers.GetSortedSetIndex(customer) + 1, customer.CustomerId);
        // Rank=����+1,��ȡֵ��ʱ�������ֵ
        //customer.Rank = _rankedCustomers.GetSortedSetIndex(customer) + 1;
        //�ؽ���ѯ����

    }



    /// <summary>
    /// - {start}��
    /// - {end}��
    /// </summary>
    /// <param name="start">��ʼ����������������������Ӧ��</param>
    /// <param name="end">��������������������������Ӧ��</param>
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
