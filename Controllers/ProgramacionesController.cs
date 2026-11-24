// Recibe solicitudes HTTP para consultar y administrar turnos médicos.
using GestionCitasAPI.Models;
using GestionCitasAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace GestionCitasAPI.Controllers;

// Activa el comportamiento de API y la validación automática de los DTOs.
[ApiController]

// Dirección base para acceder a las programaciones.
[Route("api/programaciones")]

// Documenta los posibles errores; no los genera ni los captura.
[ProducesResponseType(typeof(ProblemDetails), 400)]
[ProducesResponseType(typeof(ProblemDetails), 404)]
[ProducesResponseType(typeof(ProblemDetails), 409)]
[ProducesResponseType(typeof(ProblemDetails), 500)]
public sealed class ProgramacionesController(IProgramacionServicio servicio) : ControllerBase
{
    // GET: lista turnos; permite filtrar por médico y especialidad desde la URL.
    [HttpGet]
    public ActionResult<List<ProgramacionItemDto>> ObtenerTodos([FromQuery] int? medicoId, [FromQuery] int? especialidadId) => Ok(servicio.ListarTodos(medicoId, especialidadId));

    // GET por ID: consulta un turno y devuelve 200 si existe.
    [HttpGet("{id:int}")]
    public ActionResult<ProgramacionItemDto> ObtenerPorId(int id) => Ok(servicio.BuscarPorId(id));

    // POST: crea un turno y devuelve 201 con sus datos y URL de consulta.
    [HttpPost]
    [ProducesResponseType(typeof(ProgramacionItemDto), 201)]
    public ActionResult<ProgramacionItemDto> Crear(CrearProgramacionDto dto)
    {
        var nuevo = servicio.Registrar(dto);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = nuevo.Id }, nuevo);
    }

    // PUT: actualiza el turno indicado y devuelve sus datos.
    [HttpPut("{id:int}")]
    public ActionResult<ProgramacionItemDto> Actualizar(int id, CrearProgramacionDto dto) => Ok(servicio.Actualizar(id, dto));

    // DELETE: elimina el turno si el servicio lo permite; devuelve 204 sin contenido.
    [HttpDelete("{id:int}")]
    [ProducesResponseType(204)]
    public IActionResult Eliminar(int id) { servicio.Eliminar(id); return NoContent(); }

    // GET: muestra los bloques de 20 minutos y su disponibilidad.
    [HttpGet("{id:int}/disponibilidad")]
    public ActionResult<List<HorarioItemDto>> Disponibilidad(int id) => Ok(servicio.Disponibilidad(id));
}

