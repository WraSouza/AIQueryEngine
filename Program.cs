using AIQueryEngine.Clients;
using AIQueryEngine.Helpers;
using AIQueryEngine.Tools;
using Scalar.AspNetCore;
using ServerMCP.Endpoints;
using ServerMCP.Gemini;
using ServerMCP.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddHttpClient<SapApiClient>();

builder.Services.AddScoped<SalesTools>();
builder.Services.AddSingleton<SapTableCatalog>();

builder.Services.AddHttpClient<GeminiClient>();

var app = builder.Build();

app.MapScalarApiReference();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapSalesEndpoints();

app.UseHttpsRedirection();

app.Run();


