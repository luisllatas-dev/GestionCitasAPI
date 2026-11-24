// ManejadorExcepcionesMiddleware.cs: captura excepciones y devuelve errores en JSON.
using GestionCitasAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace GestionCitasAPI.Middlewares;

// next continúa la solicitud; logger registra lo ocurrido en el servidor.
public sealed class ManejadorExcepcionesMiddleware(
    RequestDelegate next,
    ILogger<ManejadorExcepcionesMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Continúa el procesamiento de la solicitud.
            await next(context);
        }
        catch (OperationCanceledException)
            when (context.RequestAborted.IsCancellationRequested)
        {
            // Si el cliente desconectó, no intenta enviarle una respuesta.
            logger.LogDebug(
                "Solicitud cancelada: {TraceId}", context.TraceIdentifier);
        }
        catch (Exception ex)
        {
            // Si la respuesta ya se está enviando, no se puede reemplazar.
            if (context.Response.HasStarted)
            {
                logger.LogError(ex,
                    "La respuesta ya comenzó. TraceId: {TraceId}",
                    context.TraceIdentifier);
                throw;
            }

            // Determina el código HTTP según el tipo de excepción.
            var estado = ex switch
            {
                ExcepcionNegocio negocio => negocio.Estado,
                BadHttpRequestException peticion => peticion.StatusCode,
                _ => StatusCodes.Status500InternalServerError
            };

            // Registra los fallos internos y los rechazos esperados.
            if (estado >= 500)
                logger.LogError(ex,
                    "Fallo interno en {Ruta}. TraceId: {TraceId}",
                    context.Request.Path, context.TraceIdentifier);
            else
                logger.LogInformation(
                    "Solicitud rechazada con {Estado} en {Ruta}",
                    estado, context.Request.Path);

            // Limpia la respuesta pendiente y asigna el código HTTP.
            context.Response.Clear();
            context.Response.StatusCode = estado;

            // Construye un error uniforme sin revelar detalles internos del 500.
            var problema = new ProblemDetails
            {
                Type = "about:blank",
                Status = estado,
                Title = estado switch
                {
                    400 => "Datos inválidos",
                    404 => "Recurso no encontrado",
                    409 => "Conflicto de negocio",
                    _ => "Error al procesar la solicitud"
                },
                Detail = estado >= 500
                    ? "Ocurrió un error interno. Utilice el traceId para reportarlo."
                    : ex is ExcepcionNegocio
                        ? ex.Message
                        : "La solicitud HTTP no es válida.",
                Instance = context.Request.Path
            };

            // Identifica la solicitud para localizarla en los registros.
            problema.Extensions["traceId"] = context.TraceIdentifier;

            // Envía el error como JSON, visible al probar desde Scalar.
            await context.Response.WriteAsJsonAsync(
                problema,
                options: null,
                contentType: "application/problem+json",
                cancellationToken: context.RequestAborted);
        }
    }
}
