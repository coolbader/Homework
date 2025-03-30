using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;

namespace Homework
{
    public class CalcRankService : BackgroundService
    {
        private readonly RankedCustomersManager _rankedCustomers;
        private CancellationToken _cancellationToken;

        public CalcRankService(RankedCustomersManager rankedCustomers)
        {
            _rankedCustomers = rankedCustomers;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _cancellationToken = stoppingToken;
            while (!stoppingToken.IsCancellationRequested)
            {
                List<CustomerData> itemsToProcess = new List<CustomerData>();

                // 从 ConcurrentQueue 中取出所有元素
                while (StaticVariable._queue.TryDequeue(out var customerData))
                {
                    itemsToProcess.Add(customerData);
                }

                if (itemsToProcess.Count > 0)
                {
                    try
                    {
                        _rankedCustomers.BatchSave(itemsToProcess);
                        Console.WriteLine($"Processed {itemsToProcess.Count} customer data.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error in BatchSave: {ex.Message}");
                    }
                }

                await Task.Delay(10, stoppingToken);
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}