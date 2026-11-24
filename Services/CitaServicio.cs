namespace GestionCitasAPI.Services;

using GestionCitasAPI.Models;

public interface ICitaServicio
{
    List<CitaItemDto> ListarTodas();
    CitaItemDto? BuscarPorId(int id);
    CitaItemDto Registrar(CrearCitaDto dto);
    CitaItemDto? Actualizar(int id, CrearCitaDto dto);
    bool Eliminar(int id);
}

/// <summary>
/// Servicio en memoria para el CRUD Básico de Citas con Auditoría (RN-CIT-07).
/// Recibe IPacienteServicio e IMedicoServicio por Inyección de Dependencias para obtener sus nombres y especialidad.
/// </summary>
public class CitaServicioEnMemoria(IPacienteServicio pacienteServicio, IMedicoServicio medicoServicio) : ICitaServicio
{
    private readonly List<CitaItemDto> _citas = new()
    {
        new(
            Id: 1001,
            PacienteId: 1,
            NombrePaciente: "Juan Carlos Pérez Gómez",
            MedicoId: 101,
            NombreMedico: "Roberto Sánchez Mendoza",
            Especialidad: "Cardiología",
            FechaHora: DateTime.Now.AddDays(2).Date.AddHours(10),
            Motivo: "Chequeo preventivo anual",
            Estado: "Programada",
            FechaCreacion: DateTime.Now.AddDays(-1),
            FechaActualizacion: null
        )
    };

    public List<CitaItemDto> ListarTodas() => _citas.ToList();

    public CitaItemDto? BuscarPorId(int id) => _citas.FirstOrDefault(c => c.Id == id);

    public CitaItemDto Registrar(CrearCitaDto dto)
    {
        var paciente = pacienteServicio.BuscarPorId(dto.PacienteId);
        var medico = medicoServicio.BuscarPorId(dto.MedicoId);

        int nuevoId = _citas.Any() ? _citas.Max(c => c.Id) + 1 : 1001;

        // RN-CIT-06 y RN-CIT-07: Estado inicial "Programada", FechaCreacion = DateTime.Now
        var nueva = new CitaItemDto(
            Id: nuevoId,
            PacienteId: dto.PacienteId,
            NombrePaciente: paciente?.NombreCompleto ?? "Paciente no especificado",
            MedicoId: dto.MedicoId,
            NombreMedico: medico?.NombreCompleto ?? "Médico no especificado",
            Especialidad: medico?.NombreEspecialidad ?? "Sin especialidad",
            FechaHora: dto.FechaHora,
            Motivo: dto.Motivo,
            Estado: "Programada",
            FechaCreacion: DateTime.Now,
            FechaActualizacion: null
        );

        _citas.Add(nueva);
        return nueva;
    }

    public CitaItemDto? Actualizar(int id, CrearCitaDto dto)
    {
        int index = _citas.FindIndex(c => c.Id == id);
        if (index == -1) return null;

        var existente = _citas[index];
        var paciente = pacienteServicio.BuscarPorId(dto.PacienteId);
        var medico = medicoServicio.BuscarPorId(dto.MedicoId);

        // RN-CIT-07 (Auditoría): Conserva FechaCreacion y actualiza FechaActualizacion
        var actualizada = existente with
        {
            PacienteId = dto.PacienteId,
            NombrePaciente = paciente?.NombreCompleto ?? existente.NombrePaciente,
            MedicoId = dto.MedicoId,
            NombreMedico = medico?.NombreCompleto ?? existente.NombreMedico,
            Especialidad = medico?.NombreEspecialidad ?? existente.Especialidad,
            FechaHora = dto.FechaHora,
            Motivo = dto.Motivo,
            FechaActualizacion = DateTime.Now
        };

        _citas[index] = actualizada;
        return actualizada;
    }

    public bool Eliminar(int id)
    {
        var item = BuscarPorId(id);
        if (item is null) return false;
        return _citas.Remove(item);
    }
}
