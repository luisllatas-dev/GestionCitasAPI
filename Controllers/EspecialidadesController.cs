// Recibe solicitudes HTTP para consultar y administrar especialidades.
using GestionCitasAPI.Models;
using GestionCitasAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace GestionCitasAPI.Controllers;

// Activa el comportamiento de API y la validación automática de los DTOs.
[ApiController]

// Dirección base para acceder a las especialidades.
[Route("api/especialidades")]

// Documenta los posibles errores; no los genera ni los captura.
[ProducesResponseType(typeof(ProblemDetails), 400)]
[ProducesResponseType(typeof(ProblemDetails), 404)]
[ProducesResponseType(typeof(ProblemDetails), 409)]
[ProducesResponseType(typeof(ProblemDetails), 500)]
public sealed class EspecialidadesController(IEspecialidadServicio servicio) : ControllerBase
{
    // GET: devuelve la lista de especialidades.
    [HttpGet]
    public ActionResult<List<EspecialidadItemDto>> ObtenerTodos() => Ok(servicio.ListarTodos());

    // GET por ID: consulta una especialidad y devuelve 200 si existe.
    [HttpGet("{id:int}")]
    public ActionResult<EspecialidadItemDto> ObtenerPorId(int id) => Ok(servicio.BuscarPorId(id));

    // POST: registra una especialidad y devuelve 201 con sus datos y URL de consulta.
    [HttpPost]
    [ProducesResponseType(typeof(EspecialidadItemDto), 201)]
    public ActionResult<EspecialidadItemDto> Crear(CrearEspecialidadDto dto)
    {
        var nuevo = servicio.Registrar(dto);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = nuevo.Id }, nuevo);
    }

    // PUT: actualiza la especialidad indicada y devuelve sus datos.
    [HttpPut("{id:int}")]
    public ActionResult<EspecialidadItemDto> Actualizar(int id, CrearEspecialidadDto dto) => Ok(servicio.Actualizar(id, dto));

    // DELETE: elimina la especialidad si el servicio lo permite; devuelve 204 sin contenido.
    [HttpDelete("{id:int}")]
    [ProducesResponseType(204)]
    public IActionResult Eliminar(int id) { servicio.Eliminar(id); return NoContent(); }
}
