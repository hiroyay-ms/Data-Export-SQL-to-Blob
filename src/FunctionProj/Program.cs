using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FunctionProj.Data;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        var connectionString = System.Environment.GetEnvironmentVariable("SQL_CONNECTION_STRING") ?? throw new InvalidOperationException("Connection string 'SQL_CONNECTION_STRING' not found.");
        services.AddDbContext<AdventureWorksContext>(options => options.UseSqlServer(connectionString));
    })
    .Build();

host.Run();
