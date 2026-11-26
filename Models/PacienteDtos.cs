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
    [Required(ErrorMessage = "Los nombres son obligatorios.")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "Los nombres deben tener entre 2 y 80 caracteres.")]
    public string Nombres { get; init; } = "";

    // Obligatorio: entre 2 y 80 caracteres.
    [Required(ErrorMessage = "Los apellidos son obligatorios.")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "Los apellidos deben tener entre 2 y 80 caracteres.")]
    public string Apellidos { get; init; } = "";

    // Obligatorio: exactamente 8 dígitos.
    [Required(ErrorMessage = "El DNI es obligatorio.")]
    [RegularExpression(@"^[0-9]{8}$", ErrorMessage = "El DNI debe tener exactamente 8 dígitos numéricos.")]
    public string Dni { get; init; } = "";

    // Obligatorio: exactamente 9 dígitos.
    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [RegularExpression(@"^[0-9]{9}$", ErrorMessage = "El teléfono debe tener exactamente 9 dígitos numéricos.")]
    public string Telefono { get; init; } = "";

    // Opcional: si se proporciona, valida formato y máximo 120 caracteres.
    [EmailAddress(ErrorMessage = "El correo electrónico no tiene un formato válido.")]
    [StringLength(120, ErrorMessage = "El correo electrónico no puede superar los 120 caracteres.")]
    public string? Correo { get; init; } = null;

    // Habilitado por defecto para nuevas reservas.
    public bool Activo { get; init; } = true;
}
