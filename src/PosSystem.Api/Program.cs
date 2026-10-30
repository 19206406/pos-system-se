using Identity.Module;
using PosSystem.Api.ExceptionHandling;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSingleton(TimeProvider.System); 


/// <summary> shared exception </summary>
builder.Services.AddGlobalExceptionHandling(); 

/// <summary> modules </summary>
builder.Services.AddIdentityModule(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages(); 

/// <summary> scalar configuration </summary>
app.MapOpenApi();
app.MapScalarApiReference("/api-docs", options =>
{
    options.WithTitle("Pos-system API Documentation"); 
});

app.MapControllers();

app.Run();
