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
    [Required, StringLength(80, MinimumLength = 2)]
    public string Nombres { get; init; } = "";

    // Obligatorio: entre 2 y 80 caracteres.
    [Required, StringLength(80, MinimumLength = 2)]
    public string Apellidos { get; init; } = "";

    // Obligatorio: exactamente 5 o 6 dígitos.
    [Required, RegularExpression(@"^[0-9]{5,6}$")]
    public string Cmp { get; init; } = "";

    // Exige al menos una especialidad; el servicio verifica que exista.
    [Required, MinLength(1)]
    public int[] EspecialidadIds { get; init; } = [];

    // Habilitado por defecto para nuevas reservas.
    public bool Activo { get; init; } = true;
}
