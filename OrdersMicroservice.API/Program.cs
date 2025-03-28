using eCommerce.OrdersMicroService.API.Middleware;
using eCommerce.OrdersMicroservice.BusinessLogicLayer;
using eCommerce.OrdersMicroservice.DataAccessLayer;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add DI services DAL and BLL
builder.Services.AddBusinessLayer(builder.Configuration);
builder.Services.AddDataAccessLayer(builder.Configuration);

// Add Controllers
builder.Services.AddControllers();

// FluentValidations
builder.Services.AddFluentValidationAutoValidation();

//Cors
builder.Services.AddCors(options =>
{
  options.AddDefaultPolicy(builder =>
  {
    builder.WithOrigins("http://localhost:4200")
    .AllowAnyMethod()
    .AllowAnyHeader();
  });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

// Add Exception Extention Middleware
app.UseExceptionHandlingMiddleware();

// Add Controller route
app.UseRouting();

// Auth
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Endpoints
app.MapControllers();

app.Run();


