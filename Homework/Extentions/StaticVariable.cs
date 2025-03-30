using System.Collections.Concurrent;

namespace Homework
{
    public static class StaticVariable
    {
        public static ConcurrentQueue<CustomerData> _queue = new ConcurrentQueue<CustomerData>();
    }
}
