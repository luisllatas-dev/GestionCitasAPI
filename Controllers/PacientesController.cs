namespace GestionCitasAPI.Controllers;

using GestionCitasAPI.Models;
using GestionCitasAPI.Services;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Controlador REST delgado (Thin Controller) para la gestión de Pacientes.
/// Delega toda la lógica y persistencia a IPacienteServicio.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PacientesController(IPacienteServicio servicio) : ControllerBase
{
    /// <summary>
    /// GET /api/pacientes -> Retorna 200 OK con la lista de todos los pacientes.
    /// </summary>
    [HttpGet]
    public ActionResult<List<PacienteItemDto>> ObtenerTodos()
    {
        return Ok(servicio.ListarTodos());
    }

    /// <summary>
    /// GET /api/pacientes/{id} -> Retorna 200 OK si existe, o 404 NotFound si no se encuentra.
    /// </summary>
    [HttpGet("{id:int}")]
    public ActionResult<PacienteItemDto> ObtenerPorId(int id)
    {
        var paciente = servicio.BuscarPorId(id);
        if (paciente is null)
            return NotFound(new { mensaje = $"El paciente con ID {id} no está registrado." });

        return Ok(paciente);
    }

    /// <summary>
    /// POST /api/pacientes -> Retorna 201 Created con la cabecera Location apuntando al nuevo recurso.
    /// </summary>
    [HttpPost]
    public ActionResult<PacienteItemDto> Crear([FromBody] CrearPacienteDto dto)
    {
        var nuevo = servicio.Registrar(dto);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = nuevo.Id }, nuevo);
    }

    /// <summary>
    /// PUT /api/pacientes/{id} -> Retorna 200 OK con el paciente actualizado (incluyendo FechaActualizacion) o 404 NotFound.
    /// </summary>
    [HttpPut("{id:int}")]
    public ActionResult<PacienteItemDto> Actualizar(int id, [FromBody] CrearPacienteDto dto)
    {
        var actualizado = servicio.Actualizar(id, dto);
        if (actualizado is null)
            return NotFound(new { mensaje = $"No se pudo actualizar: El paciente con ID {id} no existe." });

        return Ok(actualizado);
    }

    /// <summary>
    /// DELETE /api/pacientes/{id} -> Retorna 204 NoContent si se eliminó, o 404 NotFound.
    /// </summary>
    [HttpDelete("{id:int}")]
    public IActionResult Eliminar(int id)
    {
        bool eliminado = servicio.Eliminar(id);
        if (!eliminado)
            return NotFound(new { mensaje = $"No se pudo eliminar: El paciente con ID {id} no existe." });

        return NoContent();
    }
}
