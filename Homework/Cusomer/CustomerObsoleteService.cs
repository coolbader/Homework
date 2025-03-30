using Homework;
using Homework.Cusomer;
using System.Collections.Concurrent;
namespace Customer;
/// <summary>
/// �����ֵ�洢�ͻ����ݣ����ú�����洢���ֲ�����,������������ʵʱ���㡣
/// ȱ�㣺
/// 1.�������������O(1),���º������ɾ����������O��N��+O(1).
/// 2.������Է����Ϳͻ�ID�������򣬽����������������������ؽ����������O��N��
/// 3.��ȡ�����Ĺ��̿��ܻ�����O��N��������ʵ�ʸ��½�����
/// ���ԣ�
/// ʵ�ʲ���10001������Ԥ��Ҫ3�롣
/// �ܽ᣺�ӳ��ؽ�����������ÿ��5��������У��������󲻷���
/// </summary>
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
    /// 2.���������õ��������ݣ���_customers�еõ�ʵ�ʵ����ݡ�
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
        RebuildIndex();
    }

    private void RebuildIndex()
    {
        _cusomerIndex = _rankedCustomers.ToList();
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


