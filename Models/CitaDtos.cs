namespace GestionCitasAPI.Models;

/// <summary>
/// DTO de salida que representa una Cita Médica con información enriquecida y auditoría.
/// </summary>
public record CitaItemDto(
    int Id,
    int PacienteId,
    string NombrePaciente,
    int MedicoId,
    string NombreMedico,
    string Especialidad,
    DateTime FechaHora,
    string Motivo,
    string Estado,
    DateTime FechaCreacion,
    DateTime? FechaActualizacion
);

/// <summary>
/// DTO de entrada para agendar una nueva Cita Médica (POST).
/// </summary>
public record CrearCitaDto(
    int PacienteId,
    int MedicoId,
    DateTime FechaHora,
    string Motivo
);

