using Identity.Presentation;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddOpenApi(); 

// modules 
builder.Services.AddIdentityModule(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.

// configuration Scalar 
app.MapOpenApi();
app.MapScalarApiReference("/api-docs", options =>
{
    options.WithTitle("Pos-system API Documentation"); 
}); 

//app.UseHttpsRedirection();

//app.UseAuthorization();

app.MapControllers();

app.Run();
