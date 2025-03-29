using Customer;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        //  add services to the container
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddSingleton(new CustomerService());
        //lock port to test in local.
        builder.WebHost.UseUrls("http://localhost:5000");
        var app = builder.Build();

        // configration swagger.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        var customerService = app.Services.GetRequiredService<CustomerService>();

        app.MapPost("/customer/{customerid}/score/{score}", (long customerid, decimal score) =>
        {
            try
            {
                var newScore = customerService.UpdateScore(customerid, score);
                return Results.Ok(newScore);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        });

        app.MapGet("/leaderboard", (int start, int end) =>
        {
            try
            {
                var result = customerService.GetCustomersByRank(start, end);
                return Results.Ok(result);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        });

        app.MapGet("/leaderboard/{customerid}", (long customerid, int high = 0, int low = 0) =>
        {
            try
            {
                var result = customerService.GetCustomersAroundCustomer(customerid, high, low);
                return Results.Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        });

        app.Run();

    }
}