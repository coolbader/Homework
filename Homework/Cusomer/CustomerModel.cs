using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
namespace Customer;
public class CustomerModel
{
    public readonly object LockObj = new object();
    public ConcurrentDictionary<long, decimal> CustomerScores { get; } = new ConcurrentDictionary<long, decimal>();
    public SortedDictionary<decimal, List<long>> Leaderboard { get; } = new SortedDictionary<decimal, List<long>>(Comparer<decimal>.Create((x, y) => y.CompareTo(x)));
    public Dictionary<long, int> CustomerRanks { get; } = new Dictionary<long, int>();
}