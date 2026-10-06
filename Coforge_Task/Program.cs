using Coforge_Task.Data;
using Coforge_Task.Services;
using Coforge_Task.Services.Implementations;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<GiftCardContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IPaymentService, MockPaymentService>();
builder.Services.AddScoped<IGiftCardProvider, MockGiftCardProvider>();
builder.Services.AddScoped<IGiftCardPurchaseService, GiftCardPurchaseService>();

builder.Services.AddControllers();

// Add Swagger/OpenAPI configuration
builder.Services.AddSwaggerGen();

builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // Enable Swagger UI
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Digital Gift Card Purchase API V1");
        options.RoutePrefix = "swagger"; // Access at /swagger
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
