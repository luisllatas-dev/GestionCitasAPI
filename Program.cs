// Punto de entrada: registra servicios, carga la simulación una sola vez y configura Scalar.
using GestionCitasAPI.Middlewares;
using GestionCitasAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// ApiController valida los DTOs antes del servicio. Este bloque uniforma también esos errores automáticos.
builder.Services.Configure<ApiBehaviorOptions>(opciones => opciones.InvalidModelStateResponseFactory = contexto =>
{
    var problema = new ValidationProblemDetails(contexto.ModelState)
    {
        Type = "about:blank", Title = "Datos de entrada inválidos", Status = 400, Instance = contexto.HttpContext.Request.Path
    };
    problema.Extensions["traceId"] = contexto.HttpContext.TraceIdentifier;
    var respuesta = new BadRequestObjectResult(problema);
    respuesta.ContentTypes.Add("application/problem+json");
    return respuesta;
});

// Singleton conserva datos durante toda la ejecución. Todos comparten una sola compuerta de sincronización.
builder.Services.AddSingleton<AlmacenEnMemoria>();
builder.Services.AddSingleton<IPacienteServicio, PacienteServicioEnMemoria>();
builder.Services.AddSingleton<IMedicoServicio, MedicoServicioEnMemoria>();
builder.Services.AddSingleton<IEspecialidadServicio, EspecialidadServicioEnMemoria>();
builder.Services.AddSingleton<IProgramacionServicio, ProgramacionServicioEnMemoria>();
builder.Services.AddSingleton<ICitaServicio, CitaServicioEnMemoria>();

var app = builder.Build();
app.UseMiddleware<ManejadorExcepcionesMiddleware>();

// Respuestas sin cuerpo, por ejemplo una ruta inexistente, también usan ProblemDetails.
app.UseStatusCodePages(async contexto =>
{
    var http = contexto.HttpContext;
    var problema = new ProblemDetails { Type = "about:blank", Status = http.Response.StatusCode, Title = "Solicitud no atendida", Instance = http.Request.Path };
    problema.Extensions["traceId"] = http.TraceIdentifier;
    await http.Response.WriteAsJsonAsync(problema, options: null, contentType: "application/problem+json");
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(opciones => opciones.WithTitle("Gestión de Citas Médicas — 8 reglas esenciales").WithTheme(ScalarTheme.BluePlanet));
}

app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();
app.MapControllers();

// Se carga después de construir los servicios y antes de escuchar solicitudes.
// No se vuelve a ejecutar por cada consulta: así no duplica la información.
DatosPrueba.Cargar(app.Services);
app.Run();
