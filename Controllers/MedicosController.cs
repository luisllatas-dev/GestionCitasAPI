// Recibe solicitudes HTTP para consultar y administrar médicos.
using GestionCitasAPI.Models;
using GestionCitasAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace GestionCitasAPI.Controllers;

// Activa el comportamiento de API y la validación automática de los DTOs.
[ApiController]

// Dirección base para acceder a los médicos.
[Route("api/medicos")]

// Documenta los posibles errores; no los genera ni los captura.
[ProducesResponseType(typeof(ProblemDetails), 400)]
[ProducesResponseType(typeof(ProblemDetails), 404)]
[ProducesResponseType(typeof(ProblemDetails), 409)]
[ProducesResponseType(typeof(ProblemDetails), 500)]
public sealed class MedicosController(IMedicoServicio servicio) : ControllerBase
{
    // GET: devuelve la lista de médicos.
    [HttpGet]
    public ActionResult<List<MedicoItemDto>> ObtenerTodos() => Ok(servicio.ListarTodos());

    // GET por ID: consulta un médico y devuelve 200 si existe.
    [HttpGet("{id:int}")]
    public ActionResult<MedicoItemDto> ObtenerPorId(int id) => Ok(servicio.BuscarPorId(id));

    // POST: registra un médico y devuelve 201 con sus datos y URL de consulta.
    [HttpPost]
    [ProducesResponseType(typeof(MedicoItemDto), 201)]
    public ActionResult<MedicoItemDto> Crear(CrearMedicoDto dto)
    {
        var nuevo = servicio.Registrar(dto);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = nuevo.Id }, nuevo);
    }

    // PUT: actualiza el médico indicado y devuelve sus datos.
    [HttpPut("{id:int}")]
    public ActionResult<MedicoItemDto> Actualizar(int id, CrearMedicoDto dto) => Ok(servicio.Actualizar(id, dto));

    // DELETE: elimina al médico si el servicio lo permite; devuelve 204 sin contenido.
    [HttpDelete("{id:int}")]
    [ProducesResponseType(204)]
    public IActionResult Eliminar(int id) { servicio.Eliminar(id); return NoContent(); }

    // GET: consulta los médicos que tienen asignada la especialidad indicada.
    [HttpGet("por-especialidad/{especialidadId:int}")]
    public ActionResult<List<MedicoItemDto>> PorEspecialidad(int especialidadId, [FromServices] IEspecialidadServicio especialidades)
    {
        // Verifica que la especialidad exista antes de filtrar.
        especialidades.BuscarPorId(especialidadId);

        // Selecciona los médicos cuya lista de especialidades contiene ese ID.
        return Ok(servicio.ListarTodos().Where(m => m.EspecialidadIds.Contains(especialidadId)).ToList());
    }
}
