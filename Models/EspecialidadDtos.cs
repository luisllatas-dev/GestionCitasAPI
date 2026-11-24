// EspecialidadDtos.cs: define los datos que recibe y devuelve la API.
using System.ComponentModel.DataAnnotations;

namespace GestionCitasAPI.Models;

// DTO de salida: devuelve la especialidad, su ID y fechas de registro.
public sealed record EspecialidadItemDto(
    int Id,
    string Nombre,
    bool Activo,
    DateTime FechaCreacion,
    DateTime? FechaActualizacion);

// DTO de entrada: recibe los campos para crear o actualizar.
// El servicio verifica que el nombre no esté duplicado.
public sealed class CrearEspecialidadDto
{
    // Nombre obligatorio: entre 2 y 80 caracteres.
    [Required, StringLength(80, MinimumLength = 2)]
    public string Nombre { get; init; } = "";

    // Especialidad habilitada por defecto para nuevas reservas.
    public bool Activo { get; init; } = true;
}
