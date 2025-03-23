using eCommerce.OrdersMicroservice.DataAccessLayer.Repositories;
using eCommerce.OrdersMicroservice.DataAccessLayer.RepositoryContracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace eCommerce.OrdersMicroservice.DataAccessLayer;

public static class DependancyInjection
{
   public static IServiceCollection AddDataAccessLayer(this IServiceCollection services, IConfiguration configuration)
   {
      string connectionStringTemplate = configuration.GetConnectionString("MongoDB")!;

      string connectionString = connectionStringTemplate
      .Replace("$MONGO_HOST", Environment.GetEnvironmentVariable("MONGODB_HOST"))
      .Replace("$MONGO_PORT", Environment.GetEnvironmentVariable("MONGODB_PORT"))
      .Replace("$MONGO_DATABASE", Environment.GetEnvironmentVariable("MONGODB_DATABASE"));

          services.AddSingleton<IMongoClient>(new MongoClient(connectionString));

    services.AddScoped<IMongoDatabase>(provider =>
    {
      IMongoClient client = provider.GetRequiredService<IMongoClient>();
      return client.GetDatabase(Environment.GetEnvironmentVariable("MONGODB_DATABASE"));
    });

    services.AddScoped<IOrderRepository, OrderRepository>();

      return services;
   }
}
