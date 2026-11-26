 // Recibe solicitudes HTTP para consultar y administrar citas.
using GestionCitasAPI.Models;
using GestionCitasAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace GestionCitasAPI.Controllers;

// Activa el comportamiento de API y la validación automática de los DTOs.
[ApiController]

// Dirección base para acceder a las citas.
[Route("api/citas")]

// Documenta los posibles errores; no los genera ni los captura.
[ProducesResponseType(typeof(ProblemDetails), 400)]
[ProducesResponseType(typeof(ProblemDetails), 404)]
[ProducesResponseType(typeof(ProblemDetails), 409)]
[ProducesResponseType(typeof(ProblemDetails), 500)]
public sealed class CitasController(ICitaServicio servicio) : ControllerBase
{
    // GET: devuelve la lista de citas.
    [HttpGet]
    public ActionResult<List<CitaItemDto>> ObtenerTodos() => Ok(servicio.ListarTodas());

    // GET por ID: consulta una cita y devuelve 200 si existe.
    [HttpGet("{id:int}")]
    public ActionResult<CitaItemDto> ObtenerPorId(int id) => Ok(servicio.BuscarPorId(id));

    // POST: registra una cita y devuelve 201 con sus datos y URL de consulta.
    [HttpPost]
    [ProducesResponseType(typeof(CitaItemDto), 201)]
    public ActionResult<CitaItemDto> Crear(CrearCitaDto dto)
    {
        var nuevo = servicio.Registrar(dto);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = nuevo.Id }, nuevo);
    }

    // PUT: cambia turno, hora y motivo; conserva al paciente.
    [HttpPut("{id:int}")]
    public ActionResult<CitaItemDto> Actualizar(int id, ActualizarCitaDto dto) => Ok(servicio.Actualizar(id, dto));

    // DELETE: borra la cita de la memoria y devuelve 204 sin contenido.
    [HttpDelete("{id:int}")]
    [ProducesResponseType(204)]
    public IActionResult Eliminar(int id) { servicio.Eliminar(id); return NoContent(); }

    // PATCH: cancela la cita sin borrarla y libera el horario si el servicio lo permite.
    [HttpPatch("{id:int}/cancelacion")]
    public ActionResult<CitaItemDto> Cancelar(int id, CancelarCitaDto dto) => Ok(servicio.Cancelar(id, dto));

    // GET de prueba: provoca un error 500 para comprobar el middleware.
    [HttpGet("simular-error-500")]
    public IActionResult SimularError([FromServices] IWebHostEnvironment entorno)
    {
        // Fuera del entorno de desarrollo, rechaza la prueba con un 404.
        if (!entorno.IsDevelopment()) throw new ExcepcionNegocio(404, "Recurso no disponible.");

        // El middleware captura este fallo y oculta su detalle en la respuesta.
        throw new Exception("Fallo interno simulado para comprobar la protección del middleware.");
    }
}
