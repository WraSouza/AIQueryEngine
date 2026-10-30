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

builder.Services.AddHttpClient<SapApiClient>(client =>
{
    client.BaseAddress = new Uri(SapApiEndpoint.SapApiBaseUrl);

    client.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "fc3b4dcb81f8919595fdaff9bc2927dd");
});

builder.Services.AddScoped<SalesTools>();
builder.Services.AddSingleton<SapTableCatalog>();

builder.Services.AddHttpClient<GeminiClient>(client =>
{
    // Se precisar, configure headers ou base address aqui
    client.DefaultRequestHeaders.Add("x-goog-api-key", builder.Configuration["GeminiApiKey"]);
});

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


