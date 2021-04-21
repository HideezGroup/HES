using HES.Core.Interfaces;
using HES.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace HES.Tests.Helpers
{
    public class CustomWebAppFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType ==
                        typeof(DbContextOptions<ApplicationDbContext>));

                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("hes_tests_db");
                });

                var sp = services.BuildServiceProvider();

                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<ApplicationDbContext>();
                    var logger = scopedServices
                        .GetRequiredService<ILogger<CustomWebAppFactory<TStartup>>>();

                    db.Database.EnsureCreated();
                }
            });
        }

        public IEmployeeService GetEmployeeService()
        {
            var scope = Services.CreateScope();
            return scope.ServiceProvider.GetRequiredService<IEmployeeService>();
        }

        public IHardwareVaultService GetHardwareVaultService()
        {
            var scope = Services.CreateScope();
            return scope.ServiceProvider.GetRequiredService<IHardwareVaultService>();
        }

        public IApplicationDbContext GetDbContext()
        {
            var scope = Services.CreateScope();
            return scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        }
    }
}