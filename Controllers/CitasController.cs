namespace GestionCitasAPI.Controllers;

using GestionCitasAPI.Models;
using GestionCitasAPI.Services;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Controlador REST delgado (Thin Controller) para la gestión de Citas Médicas.
/// Delega toda la lógica y persistencia a ICitaServicio.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CitasController(ICitaServicio servicio) : ControllerBase
{
    /// <summary>
    /// GET /api/citas -> Retorna 200 OK con la lista de todas las citas médicas.
    /// </summary>
    [HttpGet]
    public ActionResult<List<CitaItemDto>> ObtenerTodas()
    {
        return Ok(servicio.ListarTodas());
    }

    /// <summary>
    /// GET /api/citas/{id} -> Retorna 200 OK si existe, o 404 NotFound si no se encuentra.
    /// </summary>
    [HttpGet("{id:int}")]
    public ActionResult<CitaItemDto> ObtenerPorId(int id)
    {
        var cita = servicio.BuscarPorId(id);
        if (cita is null)
            return NotFound(new { mensaje = $"La cita médica con ID {id} no existe." });

        return Ok(cita);
    }

    /// <summary>
    /// POST /api/citas -> Retorna 201 Created con la cabecera Location hacia la cita recién agendada.
    /// </summary>
    [HttpPost]
    public ActionResult<CitaItemDto> Crear([FromBody] CrearCitaDto dto)
    {
        var nueva = servicio.Registrar(dto);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = nueva.Id }, nueva);
    }

    /// <summary>
    /// PUT /api/citas/{id} -> Retorna 200 OK con la cita actualizada (incluyendo FechaActualizacion) o 404 NotFound.
    /// </summary>
    [HttpPut("{id:int}")]
    public ActionResult<CitaItemDto> Actualizar(int id, [FromBody] CrearCitaDto dto)
    {
        var actualizada = servicio.Actualizar(id, dto);
        if (actualizada is null)
            return NotFound(new { mensaje = $"No se pudo actualizar: La cita con ID {id} no existe." });

        return Ok(actualizada);
    }

    /// <summary>
    /// DELETE /api/citas/{id} -> Retorna 204 NoContent si se eliminó, o 404 NotFound.
    /// </summary>
    [HttpDelete("{id:int}")]
    public IActionResult Eliminar(int id)
    {
        bool eliminado = servicio.Eliminar(id);
        if (!eliminado)
            return NotFound(new { mensaje = $"No se pudo eliminar: La cita con ID {id} no existe." });

        return NoContent();
    }

    /// <summary>
    /// GET /api/citas/simular-error-500
    /// [DEMO]: Lanza una excepción inesperada para verificar que ManejadorExcepcionesMiddleware responda con RFC 7807 (HTTP 500).
    /// </summary>
    [HttpGet("simular-error-500")]
    public IActionResult SimularFallaServidor()
    {
        throw new Exception("Fallo simulado: Desconexión repentina con el servidor de base de datos de la clínica.");
    }
}
