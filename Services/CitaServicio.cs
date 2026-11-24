// Reemplaza el CRUD sin validaciones por reservas coherentes con RN04–RN07.
// La fecha, médico y especialidad se derivan del turno: el cliente no puede enviar datos contradictorios.
using GestionCitasAPI.Models;
namespace GestionCitasAPI.Services;
public interface ICitaServicio
{
    List<CitaItemDto> ListarTodas();
    CitaItemDto BuscarPorId(int id);
    CitaItemDto Registrar(CrearCitaDto dto);
    CitaItemDto Actualizar(int id, ActualizarCitaDto dto);
    CitaItemDto Cancelar(int id, CancelarCitaDto dto);
    void Eliminar(int id);
}
public sealed class CitaServicioEnMemoria(AlmacenEnMemoria almacen) : ICitaServicio
{
    public List<CitaItemDto> ListarTodas() { lock (almacen.Compuerta) return almacen.Citas.Values.OrderBy(c => c.FechaHora).ToList(); }
    public CitaItemDto BuscarPorId(int id) { lock (almacen.Compuerta) return ReglasComunes.Obtener(almacen.Citas, id); }
    private (PacienteItemDto paciente, MedicoItemDto medico, EspecialidadItemDto especialidad, DateTime inicio) ValidarReserva(int pacienteId, int programacionId, TimeOnly hora, int excluir = 0)
    {
        var paciente = ReglasComunes.Obtener(almacen.Pacientes, pacienteId);
        var p = ReglasComunes.Obtener(almacen.Programaciones, programacionId);
        var medico = ReglasComunes.Obtener(almacen.Medicos, p.MedicoId);
        var especialidad = ReglasComunes.Obtener(almacen.Especialidades, p.EspecialidadId);
        ReglasComunes.Exigir(paciente.Activo && p.Activo && medico.Activo && especialidad.Activo, "Paciente, médico, especialidad y programación deben estar activos.", 409);
        var inicio = p.Fecha.ToDateTime(hora); var fin = inicio.AddMinutes(20);
        var diferencia = hora.ToTimeSpan() - p.HoraInicio.ToTimeSpan();
        ReglasComunes.Exigir(inicio > ReglasComunes.Ahora, "No se puede reservar en el pasado.");
        ReglasComunes.Exigir(diferencia.Ticks >= 0 && diferencia.Ticks % TimeSpan.FromMinutes(20).Ticks == 0 && fin <= p.Fecha.ToDateTime(p.HoraFin), "Seleccione un bloque completo de 20 minutos dentro del turno.");
        ReglasComunes.Exigir(!almacen.Citas.Values.Any(c => c.Id != excluir && c.Estado == "Programada" && (c.PacienteId == pacienteId || c.MedicoId == medico.Id) && ReglasComunes.Cruza(inicio, fin, c.FechaHora, c.FechaHoraFin)), "El médico o el paciente ya tiene una cita en ese horario.", 409);
        return (paciente, medico, especialidad, inicio);
    }
    public CitaItemDto Registrar(CrearCitaDto dto)
    {
        // La misma compuerta cubre consulta y guardado: dos solicitudes no reservan el mismo espacio.
        lock (almacen.Compuerta)
        {
            ReglasComunes.Validar(dto); MotivoValido(dto.Motivo);
            var v = ValidarReserva(dto.PacienteId, dto.ProgramacionId, dto.HoraInicio!.Value);
            var nueva = new CitaItemDto(++almacen.UltimaCita, dto.PacienteId, v.paciente.NombreCompleto, dto.ProgramacionId, v.medico.Id, v.medico.NombreCompleto, v.especialidad.Nombre, v.inicio, v.inicio.AddMinutes(20), dto.Motivo.Trim(), "Programada", null, ReglasComunes.Ahora, null);
            almacen.Citas.Add(nueva.Id, nueva); return nueva;
        }
    }
    public CitaItemDto Actualizar(int id, ActualizarCitaDto dto)
    {
        lock (almacen.Compuerta)
        {
            var c = BuscarPorId(id); ReglasComunes.Validar(dto); MotivoValido(dto.Motivo); Editable(c);
            var v = ValidarReserva(c.PacienteId, dto.ProgramacionId, dto.HoraInicio!.Value, id);
            // Recién después de validar se sustituye la cita. Un error deja el original intacto.
            var nueva = c with { ProgramacionId = dto.ProgramacionId, MedicoId = v.medico.Id, NombreMedico = v.medico.NombreCompleto, Especialidad = v.especialidad.Nombre, FechaHora = v.inicio, FechaHoraFin = v.inicio.AddMinutes(20), Motivo = dto.Motivo.Trim(), FechaActualizacion = ReglasComunes.Ahora };
            almacen.Citas[id] = nueva; return nueva;
        }
    }
    public CitaItemDto Cancelar(int id, CancelarCitaDto dto)
    {
        lock (almacen.Compuerta)
        {
            var c = BuscarPorId(id); ReglasComunes.Validar(dto); MotivoValido(dto.Motivo);
            if (c.Estado == "Cancelada") return c; // Repetir la solicitud conserva la primera cancelación.
            Editable(c);
            var nueva = c with { Estado = "Cancelada", MotivoCancelacion = dto.Motivo.Trim(), FechaActualizacion = ReglasComunes.Ahora };
            almacen.Citas[id] = nueva; return nueva;
        }
    }
    private static void Editable(CitaItemDto c) => ReglasComunes.Exigir(c.Estado == "Programada" && c.FechaHora > ReglasComunes.Ahora, "Solo se modifican o cancelan citas programadas que todavía no comenzaron.", 409);
    private static void MotivoValido(string motivo) => ReglasComunes.Exigir(motivo.Trim().Length >= 3, "El motivo debe tener al menos 3 caracteres útiles.");
    /// <summary>DELETE demuestra borrado físico; para conservar historia se debe usar cancelación.</summary>
    public void Eliminar(int id) { lock (almacen.Compuerta) { BuscarPorId(id); almacen.Citas.Remove(id); } }
}
