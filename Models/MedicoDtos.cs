namespace GestionCitasAPI.Models;

/// <summary>
/// DTO de salida que representa a un Médico registrado, vinculado a una Especialidad por su ID y con campos de auditoría.
/// </summary>
public record MedicoItemDto(
    int Id,
    string Nombres,
    string Apellidos,
    int EspecialidadId,
    string NombreEspecialidad,
    string Cmp,
    DateTime FechaCreacion,
    DateTime? FechaActualizacion
)
{
    /// <summary>
    /// Propiedad calculada automáticamente a partir de Nombres y Apellidos.
    /// Se incluye automáticamente en el JSON de respuesta.
    /// </summary>
    public string NombreCompleto => $"{Nombres} {Apellidos}".Trim();
}

/// <summary>
/// DTO de entrada para registrar o actualizar un Médico (POST / PUT) relacionándolo con EspecialidadId.
/// </summary>
public record CrearMedicoDto(
    string Nombres,
    string Apellidos,
    int EspecialidadId,
    string Cmp
);
