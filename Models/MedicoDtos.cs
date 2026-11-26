// MedicoDtos.cs: define los datos que recibe y devuelve la API.
using System.ComponentModel.DataAnnotations;

namespace GestionCitasAPI.Models;

// DTO de salida: devuelve los datos del médico, su ID y fechas.
// sealed impide que otro record herede de este.
public sealed record MedicoItemDto(
    int Id,
    string Nombres,
    string Apellidos,
    string Cmp,
    IReadOnlyList<int> EspecialidadIds,
    bool Activo,
    DateTime FechaCreacion,
    DateTime? FechaActualizacion)
{
    // Une nombres y apellidos automáticamente.
    public string NombreCompleto => $"{Nombres} {Apellidos}";
}

// DTO de entrada: recibe los campos para crear o actualizar.
// El ID y las fechas los administra el servidor.
public sealed class CrearMedicoDto
{
    // Obligatorio: entre 2 y 80 caracteres.
    [Required(ErrorMessage = "Los nombres del médico son obligatorios.")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "Los nombres del médico deben tener entre 2 y 80 caracteres.")]
    public string Nombres { get; init; } = "";

    // Obligatorio: entre 2 y 80 caracteres.
    [Required(ErrorMessage = "Los apellidos del médico son obligatorios.")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "Los apellidos del médico deben tener entre 2 y 80 caracteres.")]
    public string Apellidos { get; init; } = "";

    // Obligatorio: exactamente 5 o 6 dígitos.
    [Required(ErrorMessage = "El código CMP es obligatorio.")]
    [RegularExpression(@"^[0-9]{5,6}$", ErrorMessage = "El código CMP debe tener entre 5 y 6 dígitos numéricos.")]
    public string Cmp { get; init; } = "";

    // Exige al menos una especialidad; el servicio verifica que exista.
    [Required(ErrorMessage = "Debe indicar al menos una especialidad.")]
    [MinLength(1, ErrorMessage = "El médico debe tener al menos una especialidad asignada.")]
    public int[] EspecialidadIds { get; init; } = [];

    // Habilitado por defecto para nuevas reservas.
    public bool Activo { get; init; } = true;
}
