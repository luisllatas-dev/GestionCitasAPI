namespace GestionCitasAPI.Controllers;

using GestionCitasAPI.Models;
using GestionCitasAPI.Services;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Controlador REST delgado (Thin Controller) para la gestión de Médicos.
/// Delega toda la lógica y persistencia a IMedicoServicio.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class MedicosController(IMedicoServicio servicio) : ControllerBase
{
    /// <summary>
    /// GET /api/medicos -> Retorna 200 OK con la lista de todos los médicos.
    /// </summary>
    [HttpGet]
    public ActionResult<List<MedicoItemDto>> ObtenerTodos()
    {
        return Ok(servicio.ListarTodos());
    }

    /// <summary>
    /// GET /api/medicos/{id} -> Retorna 200 OK si existe, o 404 NotFound si no se encuentra.
    /// </summary>
    [HttpGet("{id:int}")]
    public ActionResult<MedicoItemDto> ObtenerPorId(int id)
    {
        var medico = servicio.BuscarPorId(id);
        if (medico is null)
            return NotFound(new { mensaje = $"El médico con ID {id} no está registrado." });

        return Ok(medico);
    }

    /// <summary>
    /// POST /api/medicos -> Retorna 201 Created con la cabecera Location apuntando al nuevo recurso.
    /// </summary>
    [HttpPost]
    public ActionResult<MedicoItemDto> Crear([FromBody] CrearMedicoDto dto)
    {
        var nuevo = servicio.Registrar(dto);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = nuevo.Id }, nuevo);
    }

    /// <summary>
    /// PUT /api/medicos/{id} -> Retorna 200 OK con el médico actualizado (incluyendo FechaActualizacion) o 404 NotFound.
    /// </summary>
    [HttpPut("{id:int}")]
    public ActionResult<MedicoItemDto> Actualizar(int id, [FromBody] CrearMedicoDto dto)
    {
        var actualizado = servicio.Actualizar(id, dto);
        if (actualizado is null)
            return NotFound(new { mensaje = $"No se pudo actualizar: El médico con ID {id} no existe." });

        return Ok(actualizado);
    }

    /// <summary>
    /// DELETE /api/medicos/{id} -> Retorna 204 NoContent si se eliminó, o 404 NotFound.
    /// </summary>
    [HttpDelete("{id:int}")]
    public IActionResult Eliminar(int id)
    {
        bool eliminado = servicio.Eliminar(id);
        if (!eliminado)
            return NotFound(new { mensaje = $"No se pudo eliminar: El médico con ID {id} no existe." });

        return NoContent();
    }
}
