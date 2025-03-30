using Customer;
using Homework;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Diagnostics;
using System.Net.Http.Json;

namespace HomeworkTests
{
    [TestClass]
    public class CustomerApiTest
    {
        #region Init

       
        private static WebApplicationFactory<Program> _factory;
        private static HttpClient _client;

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            _factory = new WebApplicationFactory<Program>();
            _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost:5000")
            });

        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            _client.Dispose();
            _factory.Dispose();
        }

        #endregion

        #region Single Test
        /// <summary>
        /// Add Customer
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Add_Customer_Score()
        {
            string url = $"/customer/1/score/501";
            var response = await _client.PostAsync(url, null);
            response.EnsureSuccessStatusCode();
            var customers = await response.Content.ReadFromJsonAsync<CustomerVO>();
            Assert.AreEqual<long>(1, customers.CustomerId);
            Assert.AreEqual<decimal>(501, customers.Score);
            Assert.AreEqual<int>(1, customers.Rank);
        }

        /// <summary>
        /// Update Customer
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Update_Customer_Score()
        {
            string url1 = $"/customer/1/score/501";
            var response1 = await _client.PostAsync(url1, null);
            response1.EnsureSuccessStatusCode();
            string url2 = $"/customer/1/score/501";
            var response2 = await _client.PostAsync(url2, null);
            response2.EnsureSuccessStatusCode();
            var customers = await response2.Content.ReadFromJsonAsync<CustomerVO>();
            Assert.AreEqual<long>(1, customers.CustomerId);
            Assert.AreEqual<decimal>(1002, customers.Score);
            Assert.AreEqual<int>(1, customers.Rank);
        }
        /// <summary>
        /// Test to simulate 10001 concurrent requests
        /// test to update customer 1 score to 501 and customer 100001 score to 502
        /// test to update customer score and get leaderboard
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Simulate_10001_Concurrent_Requests()
        {
            await Simulate_10001_random_Score_Customer();

            // update cusomerid 1 score is 501
            // update cusomerid 100001 score is 502
            string updateurl1 = $"/customer/1/score/501";
            string updateurl2 = $"/customer/100001/score/502";
            var response1 = await _client.PostAsync(updateurl1, null);
            response1.EnsureSuccessStatusCode();
            var response2 = await _client.PostAsync(updateurl2, null);
            response2.EnsureSuccessStatusCode();




            //get first 1 to 10
            string leaderboard = $"/leaderboard?start=1&end=10";
            var response = await _client.GetAsync(leaderboard);
            response.EnsureSuccessStatusCode();
            var customers = await response.Content.ReadFromJsonAsync<List<CustomerVO>>();
            foreach (var item in customers)
            {
                Trace.WriteLine($"CusomerID:{item.CustomerId},Score:{item.Score},Rank:{item.Rank}");
            }

        }

        



        #endregion

        /// <summary>
        /// Test to simulate 10001  random score  between 1 to 500
        /// </summary>
        /// <returns></returns>
        private async Task Simulate_10001_random_Score_Customer()
        {
            int totalRequests = 10001;
            List<Task<HttpResponseMessage>> tasks = new List<Task<HttpResponseMessage>>();
            Stopwatch stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < totalRequests; i++)
            {
                int customerId = i + 1; // customer id starts from 1
                int score = new Random().Next(1, 500);
                string url = $"/customer/{customerId}/score/{score}";

                tasks.Add(_client.PostAsync(url, null));
            }

            await Task.WhenAll(tasks);
            stopwatch.Stop();

            int successCount = tasks.Count(t => t.Result.IsSuccessStatusCode);
            int errorCount = totalRequests - successCount;

            Trace.WriteLine($"Total Requests: {totalRequests}");
            Trace.WriteLine($"Success: {successCount}");
            Trace.WriteLine($"Errors: {errorCount}");
            Trace.WriteLine($"Time Taken: {stopwatch.ElapsedMilliseconds} ms");
            Assert.AreEqual(totalRequests, successCount, "Some requests failed.");
        }
    }
}
