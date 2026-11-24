namespace GestionCitasAPI.Controllers;

using GestionCitasAPI.Models;
using GestionCitasAPI.Services;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Controlador REST delgado (Thin Controller) para la gestión de Especialidades Médicas.
/// Delega toda la lógica y persistencia a IEspecialidadServicio.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EspecialidadesController(IEspecialidadServicio servicio) : ControllerBase
{
    /// <summary>
    /// GET /api/especialidades -> Retorna 200 OK con la lista de todas las especialidades.
    /// </summary>
    [HttpGet]
    public ActionResult<List<EspecialidadItemDto>> ObtenerTodas()
    {
        return Ok(servicio.ListarTodas());
    }

    /// <summary>
    /// GET /api/especialidades/{id} -> Retorna 200 OK si existe, o 404 NotFound si no se encuentra.
    /// </summary>
    [HttpGet("{id:int}")]
    public ActionResult<EspecialidadItemDto> ObtenerPorId(int id)
    {
        var especialidad = servicio.BuscarPorId(id);
        if (especialidad is null)
            return NotFound(new { mensaje = $"La especialidad con ID {id} no está registrada." });

        return Ok(especialidad);
    }

    /// <summary>
    /// POST /api/especialidades -> Retorna 201 Created con la cabecera Location apuntando al nuevo recurso.
    /// </summary>
    [HttpPost]
    public ActionResult<EspecialidadItemDto> Crear([FromBody] CrearEspecialidadDto dto)
    {
        var nueva = servicio.Registrar(dto);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = nueva.Id }, nueva);
    }

    /// <summary>
    /// PUT /api/especialidades/{id} -> Retorna 200 OK con la especialidad actualizada (incluyendo FechaActualizacion) o 404 NotFound.
    /// </summary>
    [HttpPut("{id:int}")]
    public ActionResult<EspecialidadItemDto> Actualizar(int id, [FromBody] CrearEspecialidadDto dto)
    {
        var actualizada = servicio.Actualizar(id, dto);
        if (actualizada is null)
            return NotFound(new { mensaje = $"No se pudo actualizar: La especialidad con ID {id} no existe." });

        return Ok(actualizada);
    }

    /// <summary>
    /// DELETE /api/especialidades/{id} -> Retorna 204 NoContent si se eliminó, o 404 NotFound.
    /// </summary>
    [HttpDelete("{id:int}")]
    public IActionResult Eliminar(int id)
    {
        bool eliminada = servicio.Eliminar(id);
        if (!eliminada)
            return NotFound(new { mensaje = $"No se pudo eliminar: La especialidad con ID {id} no existe." });

        return NoContent();
    }
}
