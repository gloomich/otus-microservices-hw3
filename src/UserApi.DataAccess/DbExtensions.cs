using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace UserApi.DataAccess
{
    public static class DbExtensions
    {
        public static string ParseConnectionString(this IConfiguration configuration)
        {
            return $"Host={configuration["postgresql-host"]};" +
                $"Port={configuration["postgresql-port"]};" +
                $"Database={configuration["postgresql-db"]};" +
                $"Username={configuration["postgresql-user"]};" +
                $"Password={configuration["postgresql-password"]}";
        }

        public static void RunMigrations(string[] args)
        {            
            var builder = WebApplication.CreateBuilder(args);

            var cs = builder.Configuration.ParseConnectionString();
            Console.WriteLine(cs);
            builder.Services.AddDbContext<UserDbContext>(opt =>
                opt.UseNpgsql(cs));

            var app = builder.Build();

            using IServiceScope serviceScope = app.Services
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope();

            var context = serviceScope.ServiceProvider.GetService<UserDbContext>();
            context?.Database.Migrate();
        }

        public static IServiceCollection UseUserDb(this IServiceCollection services, IConfiguration configuration)
        {
            var cs = configuration.ParseConnectionString();
            Console.WriteLine(cs);
            services.AddDbContext<UserDbContext>(opt =>
                opt.UseNpgsql(cs));            

            return services;
        }
    }
}
