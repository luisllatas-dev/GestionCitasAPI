// Se agrega porque una cita necesita un turno autorizado: implementa RN02, RN03, RN05 y RN08.
using GestionCitasAPI.Models;
namespace GestionCitasAPI.Services;
public interface IProgramacionServicio
{
    List<ProgramacionItemDto> ListarTodos(int? medicoId = null, int? especialidadId = null);
    ProgramacionItemDto BuscarPorId(int id);
    ProgramacionItemDto Registrar(CrearProgramacionDto dto);
    ProgramacionItemDto Actualizar(int id, CrearProgramacionDto dto);
    void Eliminar(int id);
    List<HorarioItemDto> Disponibilidad(int id);
}
public sealed class ProgramacionServicioEnMemoria(AlmacenEnMemoria almacen) : IProgramacionServicio
{
    public List<ProgramacionItemDto> ListarTodos(int? medicoId = null, int? especialidadId = null)
    {
        lock (almacen.Compuerta)
        {
            if (medicoId.HasValue) ReglasComunes.Obtener(almacen.Medicos, medicoId.Value);
            if (especialidadId.HasValue) ReglasComunes.Obtener(almacen.Especialidades, especialidadId.Value);
            return almacen.Programaciones.Values.Where(p => (!medicoId.HasValue || p.MedicoId == medicoId) && (!especialidadId.HasValue || p.EspecialidadId == especialidadId)).OrderBy(p => p.Fecha).ThenBy(p => p.HoraInicio).ToList();
        }
    }
    public ProgramacionItemDto BuscarPorId(int id) { lock (almacen.Compuerta) return ReglasComunes.Obtener(almacen.Programaciones, id); }
    private void Validar(CrearProgramacionDto dto, int id = 0)
    {
        ReglasComunes.Validar(dto);
        var medico = ReglasComunes.Obtener(almacen.Medicos, dto.MedicoId);
        var especialidad = ReglasComunes.Obtener(almacen.Especialidades, dto.EspecialidadId);
        ReglasComunes.Exigir(medico.Activo && especialidad.Activo, "Médico y especialidad deben estar activos.", 409);
        ReglasComunes.Exigir(medico.EspecialidadIds.Contains(dto.EspecialidadId), "El médico no tiene asignada esa especialidad.", 409);
        var inicio = dto.Fecha!.Value.ToDateTime(dto.HoraInicio!.Value);
        var fin = dto.Fecha.Value.ToDateTime(dto.HoraFin!.Value);
        ReglasComunes.Exigir(inicio > ReglasComunes.Ahora, "El turno debe comenzar en el futuro, hora de Lima.");
        ReglasComunes.Exigir((fin - inicio).TotalMinutes >= ReglasComunes.MinutosCita, "El turno debe durar al menos 20 minutos dentro del mismo día.");
        ReglasComunes.Exigir(inicio.Ticks % TimeSpan.TicksPerMinute == 0 && fin.Ticks % TimeSpan.TicksPerMinute == 0, "Use minutos completos, sin segundos.");
        ReglasComunes.Exigir(!dto.Activo || !almacen.Programaciones.Values.Any(p => p.Id != id && p.Activo && p.MedicoId == dto.MedicoId && ReglasComunes.Cruza(inicio, fin, p.Fecha.ToDateTime(p.HoraInicio), p.Fecha.ToDateTime(p.HoraFin))), "El médico tiene un turno que se cruza, incluso si es otra especialidad.", 409);
    }
    public ProgramacionItemDto Registrar(CrearProgramacionDto dto)
    {
        lock (almacen.Compuerta)
        {
            Validar(dto);
            var nuevo = new ProgramacionItemDto(++almacen.UltimaProgramacion, dto.MedicoId, dto.EspecialidadId, dto.Fecha!.Value, dto.HoraInicio!.Value, dto.HoraFin!.Value, dto.Activo, ReglasComunes.Ahora, null);
            almacen.Programaciones.Add(nuevo.Id, nuevo); return nuevo;
        }
    }
    public ProgramacionItemDto Actualizar(int id, CrearProgramacionDto dto)
    {
        lock (almacen.Compuerta)
        {
            var anterior = BuscarPorId(id); SinCitas(id); Validar(dto, id);
            var nuevo = new ProgramacionItemDto(id, dto.MedicoId, dto.EspecialidadId, dto.Fecha!.Value, dto.HoraInicio!.Value, dto.HoraFin!.Value, dto.Activo, anterior.FechaCreacion, ReglasComunes.Ahora);
            almacen.Programaciones[id] = nuevo; return nuevo;
        }
    }
    // También bloquea turnos con citas canceladas, ya que estas conservan su referencia histórica.
    private void SinCitas(int id) => ReglasComunes.Exigir(!almacen.Citas.Values.Any(c => c.ProgramacionId == id), "El turno tiene citas asociadas; no puede modificarse ni eliminarse.", 409);
    public void Eliminar(int id) { lock (almacen.Compuerta) { BuscarPorId(id); SinCitas(id); almacen.Programaciones.Remove(id); } }
    /// <summary>Calcula solo bloques completos; los minutos sobrantes no crean una cita adicional.</summary>
    public List<HorarioItemDto> Disponibilidad(int id)
    {
        lock (almacen.Compuerta)
        {
            var p = BuscarPorId(id);
            var medico = ReglasComunes.Obtener(almacen.Medicos, p.MedicoId);
            var especialidad = ReglasComunes.Obtener(almacen.Especialidades, p.EspecialidadId);
            var salida = new List<HorarioItemDto>();
            var limite = p.Fecha.ToDateTime(p.HoraFin);
            for (var inicio = p.Fecha.ToDateTime(p.HoraInicio); inicio.AddMinutes(20) <= limite; inicio = inicio.AddMinutes(20))
            {
                var fin = inicio.AddMinutes(20);
                var libre = p.Activo && medico.Activo && especialidad.Activo && inicio > ReglasComunes.Ahora && !almacen.Citas.Values.Any(c => c.Estado == "Programada" && c.MedicoId == p.MedicoId && ReglasComunes.Cruza(inicio, fin, c.FechaHora, c.FechaHoraFin));
                salida.Add(new(TimeOnly.FromDateTime(inicio), TimeOnly.FromDateTime(fin), libre));
            }
            return salida;
        }
    }
}
