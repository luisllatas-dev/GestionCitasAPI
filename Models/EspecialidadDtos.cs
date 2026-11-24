namespace GestionCitasAPI.Models;

/// <summary>
/// DTO de salida que representa una Especialidad Médica, incluyendo estado Activo y campos de auditoría.
/// </summary>
public record EspecialidadItemDto(
    int Id,
    string Nombre,
    string Descripcion,
    bool Activo,
    DateTime FechaCreacion,
    DateTime? FechaActualizacion
);

/// <summary>
/// DTO de entrada para registrar o actualizar una Especialidad Médica (POST / PUT).
/// Por defecto Activo es true si no se especifica.
/// </summary>
public record CrearEspecialidadDto(
    string Nombre,
    string Descripcion,
    bool Activo = true
);
