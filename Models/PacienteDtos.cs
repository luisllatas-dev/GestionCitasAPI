namespace GestionCitasAPI.Models;

/// <summary>
/// DTO de salida que representa a un Paciente registrado, incluyendo campos de auditoría.
/// </summary>
public record PacienteItemDto(
    int Id,
    string Nombres,
    string Apellidos,
    string Dni,
    string Telefono,
    string Correo,
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
/// DTO de entrada para registrar o actualizar un Paciente (POST / PUT).
/// No solicita Id, NombreCompleto ni fechas de auditoría porque se calculan o gestionan automáticamente.
/// </summary>
public record CrearPacienteDto(
    string Nombres,
    string Apellidos,
    string Dni,
    string Telefono,
    string Correo
);
