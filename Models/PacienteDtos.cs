// PacienteDtos.cs: define los datos que recibe y devuelve la API.
using System.ComponentModel.DataAnnotations;

namespace GestionCitasAPI.Models;

// DTO de salida: devuelve los datos del paciente, su ID y fechas.
public sealed record PacienteItemDto(
    int Id,
    string Nombres,
    string Apellidos,
    string Dni,
    string Telefono,
    string? Correo,
    bool Activo,
    DateTime FechaCreacion,
    DateTime? FechaActualizacion)
{
    // Une nombres y apellidos automáticamente.
    public string NombreCompleto => $"{Nombres} {Apellidos}";
}

// DTO de entrada: recibe los campos para crear o actualizar.
// El ID y las fechas los administra el servidor.
public sealed class CrearPacienteDto
{
    // Obligatorio: entre 2 y 80 caracteres.
    [Required, StringLength(80, MinimumLength = 2)]
    public string Nombres { get; init; } = "";

    // Obligatorio: entre 2 y 80 caracteres.
    [Required, StringLength(80, MinimumLength = 2)]
    public string Apellidos { get; init; } = "";

    // Obligatorio: exactamente 8 dígitos.
    [Required, RegularExpression(@"^[0-9]{8}$")]
    public string Dni { get; init; } = "";

    // Obligatorio: exactamente 9 dígitos.
    [Required, RegularExpression(@"^[0-9]{9}$")]
    public string Telefono { get; init; } = "";

    // Opcional: si se proporciona, valida formato y máximo 120 caracteres.
    [EmailAddress, StringLength(120)]
    public string? Correo { get; init; } = null;

    // Habilitado por defecto para nuevas reservas.
    public bool Activo { get; init; } = true;
}
