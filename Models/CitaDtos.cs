/// CitaDtos.cs: define los datos para consultar, crear, actualizar y cancelar citas.
using System.ComponentModel.DataAnnotations;

namespace GestionCitasAPI.Models;

// DTO de salida: devuelve la cita con datos del paciente, médico y estado.
public sealed record CitaItemDto(
    int Id,
    int PacienteId,
    string NombrePaciente,
    int ProgramacionId,
    int MedicoId,
    string NombreMedico,
    string Especialidad,
    DateTime FechaHora,
    DateTime FechaHoraFin,
    string Motivo,
    string Estado,
    string? MotivoCancelacion,
    DateTime FechaCreacion,
    DateTime? FechaActualizacion);

// DTO de entrada: recibe los datos para reservar.
// El servicio valida disponibilidad y calcula el fin de la cita.
public sealed class CrearCitaDto
{
    // ID mayor que cero; el servicio verifica al paciente.
    [Range(1, int.MaxValue)]
    public int PacienteId { get; init; } = 0;

    // Turno elegido: determina el médico, especialidad y fecha.
    [Range(1, int.MaxValue)]
    public int ProgramacionId { get; init; } = 0;

    // Hora obligatoria; debe corresponder a un bloque disponible.
    [Required]
    public TimeOnly? HoraInicio { get; init; } = null;

    // Motivo obligatorio: entre 3 y 250 caracteres.
    [Required, StringLength(250, MinimumLength = 3)]
    public string Motivo { get; init; } = "";
}

// DTO de actualización: permite cambiar turno, hora y motivo.
// El paciente de la cita se mantiene.
public sealed class ActualizarCitaDto
{
    // Turno al que se desea trasladar la cita.
    [Range(1, int.MaxValue)]
    public int ProgramacionId { get; init; }

    // Nueva hora solicitada; el servicio verifica disponibilidad.
    [Required]
    public TimeOnly? HoraInicio { get; init; }

    // Motivo actualizado, obligatorio.
    [Required, StringLength(250, MinimumLength = 3)]
    public string Motivo { get; init; } = "";
}

// DTO de cancelación: recibe el motivo.
// El servicio cambia el estado y libera el horario.
public sealed class CancelarCitaDto
{
    // Explicación obligatoria: entre 3 y 250 caracteres.
    [Required, StringLength(250, MinimumLength = 3)]
    public string Motivo { get; init; } = "";
}
