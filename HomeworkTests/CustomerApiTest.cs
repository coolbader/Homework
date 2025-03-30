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
            await UpdateCustomer_1_Score_500();
            await Top10RankList();
        }
        [TestMethod]
        public async Task Update_Customer_Score()
        {
            for (int i = 0; i < 10; i++)
            {
                await UpdateCustomer_1_Score_500();
            }
            await Task.Delay(1000);
            var result = await Top10RankList();
            Assert.AreEqual(5000, result[0].Score);
            Assert.AreEqual(1, result[0].Rank);
            Assert.AreEqual(1, result[0].CustomerId);
        }

        private async Task UpdateCustomer_1_Score_500()
        {
            string url = $"/customer/1/score/500";
            var response = await _client.PostAsync(url, null);
            response.EnsureSuccessStatusCode();
        }

        private async Task<List<CustomerVO>> Top10RankList()
        {
            string leaderboard = $"/leaderboard?start=1&end=10";
            var response = await _client.GetAsync(leaderboard);
            response.EnsureSuccessStatusCode();
            var customers = await response.Content.ReadFromJsonAsync<List<CustomerVO>>();
            foreach (var item in customers)
            {
                Trace.WriteLine($"CusomerID:{item.CustomerId},Score:{item.Score},Rank:{item.Rank}");
            }
            return customers;
        }

        [TestMethod]
        public async Task Data_Accuracy() {
            await Simulate_Score_Customer(100001, 0);
            string updateurl1 = $"/DataAccuracy";
            var response1 = await _client.GetAsync(updateurl1);
            response1.EnsureSuccessStatusCode();
            var result = await response1.Content.ReadAsStringAsync();
            Trace.WriteLine($"{updateurl1} {result}");
            await Task.Delay(10);
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
            await Simulate_Score_Customer(10001, 0);

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

        [TestMethod]
        public async Task Simulate_100001_Concurrent_Requests()
        {
            await Simulate_Score_Customer(100001, 0);

            // update cusomerid 1 score is 501
            // update cusomerid 100001 score is 502
            string updateurl1 = $"/customer/1/score/501";
            string updateurl2 = $"/customer/100001/score/502";
            var response1 = await _client.PostAsync(updateurl1, null);
            response1.EnsureSuccessStatusCode();
            var response2 = await _client.PostAsync(updateurl2, null);
            response2.EnsureSuccessStatusCode();

            await this.GetCustomerCount();
            await this.GetRankCount();


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


        private async Task GetCustomerCount()
        {
            string url = "/GetCusomerCount";
            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var customerCount = await response.Content.ReadAsStringAsync();
            Trace.WriteLine($"GetCustomerCount:{customerCount}");
        }
        private async Task GetRankCount()
        {
            string url = "/GetRankCount";
            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var customerCount = await response.Content.ReadAsStringAsync();
            Trace.WriteLine($"GetRankCount:{customerCount}");
        }
        /// <summary>
        /// Test to simulate 10001  random score  between 1 to 500
        /// </summary>
        /// <returns></returns>
        private async Task Simulate_Score_Customer(int totalRequests, int scores)
        {
            List<Task<HttpResponseMessage>> tasks = new List<Task<HttpResponseMessage>>();
            Stopwatch stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < totalRequests; i++)
            {
                int customerId = i + 1; // customer id starts from 1
                int score = scores;
                if (scores == 0)
                {
                    score = new Random().Next(1, 500);
                }
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
