// ProgramacionDtos.cs: define los datos de turnos y horarios disponibles.
using System.ComponentModel.DataAnnotations;

namespace GestionCitasAPI.Models;

// DTO de salida: devuelve el turno del médico, su ID y fechas de registro.
public sealed record ProgramacionItemDto(
    int Id,
    int MedicoId,
    int EspecialidadId,
    DateOnly Fecha,
    TimeOnly HoraInicio,
    TimeOnly HoraFin,
    bool Activo,
    DateTime FechaCreacion,
    DateTime? FechaActualizacion);

// DTO de entrada: recibe los campos para crear o actualizar un turno.
// El servicio valida disponibilidad, fechas futuras y cruces de horarios.
public sealed class CrearProgramacionDto
{
    // ID mayor que cero; el servicio verifica que el médico exista.
    [Range(1, int.MaxValue)]
    public int MedicoId { get; init; } = 0;

    // ID mayor que cero; el servicio verifica su relación con el médico.
    [Range(1, int.MaxValue)]
    public int EspecialidadId { get; init; } = 0;

    // Fecha obligatoria del turno, sin hora.
    [Required]
    public DateOnly? Fecha { get; init; } = null;

    // Hora obligatoria de inicio de atención.
    [Required]
    public TimeOnly? HoraInicio { get; init; } = null;

    // Hora obligatoria de fin; el servicio verifica que sea posterior al inicio.
    [Required]
    public TimeOnly? HoraFin { get; init; } = null;

    // Turno habilitado por defecto para nuevas reservas.
    public bool Activo { get; init; } = true;
}

// DTO de salida: muestra un bloque de 20 minutos y si está disponible.
// Se calcula desde el turno y las citas; no se guarda como otra agenda.
public sealed record HorarioItemDto(
    TimeOnly HoraInicio,
    TimeOnly HoraFin,
    bool Disponible);

