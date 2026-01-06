using Fingerprint.Core.Services;
using Fingerprint.Infrastructure.Services;
using Fingerprint.Infrastructure.Services.Implementations;
using Fingerprint.Infrastructure.Services.Interfaces;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Извлекаем строку подключения правильно
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddScoped<IStorageService, StorageService>(s => new StorageService(connectionString!));
builder.Services.AddScoped<RecognitionService>();

builder.Services.AddOpenApi();

builder.Services.AddControllers();

builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // <-- Add this line
}

app.MapGet("/", () => "Hello World!");

app.MapControllers();

app.Run();