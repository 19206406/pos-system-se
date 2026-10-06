using Identity.Presentation;
using PosSystem.Api.ExceptionHandling;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// shared exception 
builder.Services.AddGlobalExceptionHandling(); 

// modules 
builder.Services.AddIdentityModule(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();

app.UseStatusCodePages(); 

// configuration Scalar 
app.MapOpenApi();
app.MapScalarApiReference("/api-docs", options =>
{
    options.WithTitle("Pos-system API Documentation"); 
});

app.MapControllers();

app.Run();
