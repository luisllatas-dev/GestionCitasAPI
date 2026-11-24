// Recibe solicitudes HTTP para consultar y administrar pacientes.
using GestionCitasAPI.Models;
using GestionCitasAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace GestionCitasAPI.Controllers;

// Activa el comportamiento de API y la validación automática de los DTOs.
[ApiController]

// Dirección base para acceder a los pacientes.
[Route("api/pacientes")]

// Documenta los posibles errores; no los genera ni los captura.
[ProducesResponseType(typeof(ProblemDetails), 400)]
[ProducesResponseType(typeof(ProblemDetails), 404)]
[ProducesResponseType(typeof(ProblemDetails), 409)]
[ProducesResponseType(typeof(ProblemDetails), 500)]
public sealed class PacientesController(IPacienteServicio servicio) : ControllerBase
{
    // GET: devuelve la lista de pacientes.
    [HttpGet]
    public ActionResult<List<PacienteItemDto>> ObtenerTodos() => Ok(servicio.ListarTodos());

    // GET por ID: consulta un paciente y devuelve 200 si existe.
    [HttpGet("{id:int}")]
    public ActionResult<PacienteItemDto> ObtenerPorId(int id) => Ok(servicio.BuscarPorId(id));

    // POST: registra un paciente y devuelve 201 con sus datos y URL de consulta.
    [HttpPost]
    [ProducesResponseType(typeof(PacienteItemDto), 201)]
    public ActionResult<PacienteItemDto> Crear(CrearPacienteDto dto)
    {
        var nuevo = servicio.Registrar(dto);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = nuevo.Id }, nuevo);
    }

    // PUT: actualiza el paciente indicado y devuelve sus datos.
    [HttpPut("{id:int}")]
    public ActionResult<PacienteItemDto> Actualizar(int id, CrearPacienteDto dto) => Ok(servicio.Actualizar(id, dto));

    // DELETE: elimina al paciente si el servicio lo permite; devuelve 204 sin contenido.
    [HttpDelete("{id:int}")]
    [ProducesResponseType(204)]
    public IActionResult Eliminar(int id) { servicio.Eliminar(id); return NoContent(); }
}
