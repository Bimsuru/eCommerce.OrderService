using eCommerce.OrdersMicroservice.BusinessLogicLayer.HttpClients;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.Mappers;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.Policies;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.ServiceContracts;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.Services;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.ServicesHelperMethod;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer;

public static class DependancyInjection
{
   public static IServiceCollection AddBusinessLayer(this IServiceCollection services)
   {

      services.AddAutoMapper(typeof(OrderAddRequestMappingProfile).Assembly);
      services.AddValidatorsFromAssemblyContaining<OrderAddRequestValidator>();
      services.AddScoped<IOrderService, OrderService>();
      services.AddScoped<ValidationHelper>();
      services.AddTransient<IUserMicroservicePolicy, UserMicroservicePolicy>();
      
      return services;
   }
}
