namespace GestionCitasAPI.Middlewares;

using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Middleware global de excepciones bajo el estándar internacional RFC 7807 (ProblemDetails).
/// Captura cualquier error no controlado ocurrido en los servicios o controladores y devuelve un JSON uniforme
/// sin exponer jamás el Stack Trace en producción.
/// </summary>
public class ManejadorExcepcionesMiddleware(RequestDelegate next, ILogger<ManejadorExcepcionesMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Deja pasar la petición hacia el siguiente middleware o controlador
            await next(context);
        }
        catch (Exception ex)
        {
            // Si algo falla en cualquier capa inferior, lo registramos en consola y armamos la respuesta RFC 7807
            logger.LogError(ex, "Excepción capturada en {Path}: {Message}", context.Request.Path, ex.Message);
            await ManejarExcepcionAsync(context, ex);
        }
    }

    private static Task ManejarExcepcionAsync(HttpContext context, Exception excepcion)
    {
        context.Response.ContentType = "application/problem+json";

        var problema = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7807",
            Instance = context.Request.Path
        };

        if (excepcion is InvalidOperationException)
        {
            // 400 Bad Request: Violación de una regla de negocio (ej. DNI duplicado, cruce de horario, fecha pasada)
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            problema.Status = StatusCodes.Status400BadRequest;
            problema.Title = "Operación de Negocio Inválida";
            problema.Detail = excepcion.Message;
        }
        else
        {
            // 500 Internal Server Error: Falla inesperada de infraestructura o código
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            problema.Status = StatusCodes.Status500InternalServerError;
            problema.Title = "Error Interno en el Servidor";
            problema.Detail = "Ha ocurrido un error inesperado al procesar su solicitud en Gestión de Citas. Por favor, intente más tarde.";
        }

        var opciones = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        return context.Response.WriteAsync(JsonSerializer.Serialize(problema, opciones));
    }
}
