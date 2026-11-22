using GestionCitasAPI.Middlewares;
using GestionCitasAPI.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Contenedor de Inversión de Control (IoC) - Registro de Servicios
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Registramos los servicios como Singleton para que las listas en memoria persistan mientras la API esté ejecutándose
builder.Services.AddSingleton<IPacienteServicio, PacienteServicioEnMemoria>();
builder.Services.AddSingleton<IMedicoServicio, MedicoServicioEnMemoria>();
builder.Services.AddSingleton<ICitaServicio, CitaServicioEnMemoria>();

var app = builder.Build();

// 2. Pipeline de Middlewares (El manejador global de excepciones va al inicio para envolver toda la petición)
app.UseMiddleware<ManejadorExcepcionesMiddleware>();

// 3. Documentación Interactiva OpenAPI + Scalar (/scalar/v1)
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Gestión de Citas Médicas API - .NET 10")
               .WithTheme(ScalarTheme.BluePlanet);
    });
}

// 4. Mapeo de Controladores REST
app.MapControllers();

app.Run();
