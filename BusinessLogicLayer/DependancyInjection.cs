using eCommerce.ordersMicroservice.BusinessLogicLayer.RabbitMQ;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.HttpClients;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.Mappers;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.Policies;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.RabbitMQ;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.ServiceContracts;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.Services;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.ServicesHelperMethod;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.Validators;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer;

public static class DependancyInjection
{
   public static IServiceCollection AddBusinessLayer(this IServiceCollection services, IConfiguration configuration)
   {

      services.AddAutoMapper(typeof(OrderAddRequestMappingProfile).Assembly);
      services.AddValidatorsFromAssemblyContaining<OrderAddRequestValidator>();
      services.AddScoped<IOrderService, OrderService>();
      services.AddScoped<ValidationHelper>();
      services.AddTransient<IUserMicroservicePolicy, UserMicroservicePolicy>();
      services.AddTransient<IProductMicroservicePolicy, ProductMicroservicePolicy>();
      services.AddScoped<IPollyPolicies, PollyPolicies>();

      // Add HttpClient with communication with microservices
      services.AddHttpClient<UsersMicroserviceClient>(
      client =>
      {
         client.BaseAddress = new Uri($"http://{configuration["UsersMicroserviceName"]}:{configuration["UsersMicroservicePort"]}");
      }).AddPolicyHandler(services.BuildServiceProvider().GetRequiredService<IUserMicroservicePolicy>().GetCombinedPolicy());

      services.AddHttpClient<ProductsMicroserviceClient>(client =>
      {
         client.BaseAddress = new Uri($"http://{configuration["ProductsMicroserviceName"]}:{configuration["ProductsMicroservicePort"]}");
      }).AddPolicyHandler(services.BuildServiceProvider().GetRequiredService<IProductMicroservicePolicy>().GetCombinedPolicy());

      services.AddStackExchangeRedisCache(options => {
         options.Configuration = $"{configuration["REDIS_HOST"]}:{configuration["REDIS_PORT"]}";
      });

      services.AddTransient<IRabbitMQConsumer, RabbitMQConsumer>();
      services.AddTransient<RabbitMQConsumeServicesAction>();
      services.AddHostedService<RabbitMQProductUpdateHostService>();
      services.AddHostedService<RabbitMQProductDeleteHostService>();
      
      
      return services;
   }
}
