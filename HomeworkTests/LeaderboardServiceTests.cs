using Customer;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Homework.Tests
{
    [TestClass()]
    public class LeaderboardServiceTests
    {

        private CustomerService _service;

        [TestInitialize]
        public void Setup()
        {
            _service = new CustomerService();
        }
        /// <summary>
        /// 添加新客户，得分一致，排行榜一致。
        /// </summary>

        [TestMethod]
        public void UpdateScore_NewCustomer()
        {
            var customer = _service.UpdateScore(1, 100);
           
            Assert.IsNotNull(customer);
            Assert.AreEqual(100, customer.Score);
            Assert.AreEqual(1, _service.GetCustomersByRank(1, 10).Count);
        }

        /// <summary>
        /// Test to update customer score1.
        /// </summary>
        [TestMethod]
        public void UpdateScore_ExistingCustomer()
        {
            _service.UpdateScore(1, 100);
            var customer = _service.UpdateScore(1, 50);

            Assert.AreEqual(150, customer.Score);
            Assert.AreEqual(1, _service.GetCustomersByRank(1, 10).Count);
        }

        /// <summary>
        /// Test to get leaderboard and mock customer scores.
        /// </summary>
        [TestMethod]
        public void GetLeaderboard_ShouldReturnCorrectRanking()
        {
            _service.UpdateScore(1, 50);
            _service.UpdateScore(2, 200);
            _service.UpdateScore(3, 100);

            var leaderboard = _service.GetCustomersByRank(1, 3);

            Assert.AreEqual(2, leaderboard[0].CustomerId); // mock customer 2 has highest score1
            Assert.AreEqual(3, leaderboard[1].CustomerId); // mock customer 3 has second highest score1
            Assert.AreEqual(1, leaderboard[2].CustomerId); // mock customer 1 has lowest score1
        }

        /// <summary>
        /// Test to get customer with neighbors.
        /// </summary>
        [TestMethod]
        public void GetCustomerWithNeighbors_ShouldReturnCorrectNeighbors()
        {
            _service.UpdateScore(1, 100);
            _service.UpdateScore(2, 200);
            _service.UpdateScore(3, 300);
            _service.UpdateScore(4, 400);
            _service.UpdateScore(5, 500);

            var result = _service.GetCustomersAroundCustomer(3, 1, 1);

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(4, result[0].CustomerId);
            Assert.AreEqual(3, result[1].CustomerId);
            Assert.AreEqual(2, result[2].CustomerId);

        }
        /// <summary>
        /// 模拟100001个客户。
        /// 其中
        /// </summary>
        [TestMethod]
        public void Simulate_10001_Customer_Data()
        {
            Random random = new Random();
            Stopwatch stopwatch = Stopwatch.StartNew();
            int totalRequests = 10001 - 5;
            for (int i = 0; i < totalRequests; i++)
            {
                //int Id = i + 1;
                //int score1 = 200;
                _service.UpdateScore(1000,00);
            }
            for (int i = 0; i < totalRequests+5; i++) {
                int customerId = i + 1;
                int score = 600;
                _service.UpdateScore(customerId, score);
            }
            stopwatch.Stop();
            Trace.WriteLine($"Total Requests: {totalRequests+5}");
            Trace.WriteLine($"Time Taken: {stopwatch.ElapsedMilliseconds} ms");
            var result = _service.GetCustomersByRank(1,50);
            Trace.WriteLine("Customer Insert Count:"+_service.GetCustomerCount());
            foreach (var item in result)
            {
                Trace.WriteLine($"cusomerid:{item.CustomerId},Score:{item.Score},Rank:{item.Rank}");
            }

        }
        [TestMethod]
        public void Simulate_10_Customer_Data() {
            for (int i = 0; i < 10; i++)
            {
                _service.UpdateScore(i + 1, 1);
            }
            var list = _service.GetCustomersByRank(1, 50);

        }



    }
}