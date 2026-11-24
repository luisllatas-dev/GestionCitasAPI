// Administra medicos en memoria. Se creó/ajustó para RN01 (duplicados) y RN08 (referencias).
using GestionCitasAPI.Models;
namespace GestionCitasAPI.Services;
/// <summary>Contrato que permite al controlador trabajar sin depender de la implementación de almacenamiento.</summary>
public interface IMedicoServicio
{
    List<MedicoItemDto> ListarTodos();
    MedicoItemDto BuscarPorId(int id);
    MedicoItemDto Registrar(CrearMedicoDto dto);
    MedicoItemDto Actualizar(int id, CrearMedicoDto dto);
    void Eliminar(int id);
}
public sealed class MedicoServicioEnMemoria(AlmacenEnMemoria almacen) : IMedicoServicio
{
    /// <summary>Devuelve una lista nueva para no entregar la colección interna.</summary>
    public List<MedicoItemDto> ListarTodos() { lock (almacen.Compuerta) return almacen.Medicos.Values.ToList(); }
    public MedicoItemDto BuscarPorId(int id) { lock (almacen.Compuerta) return ReglasComunes.Obtener(almacen.Medicos, id); }
    /// <summary>Comprueba formatos y duplicados antes de reservar un ID.</summary>
    private void Validar(CrearMedicoDto dto, int id = 0)
    {
        ReglasComunes.Validar(dto);
        ReglasComunes.Exigir(dto.Nombres.Trim().Length >= 2 && dto.Apellidos.Trim().Length >= 2, "Complete nombres y apellidos.");
        ReglasComunes.Exigir(!almacen.Medicos.Values.Any(m => m.Id != id && m.Cmp == dto.Cmp), "El CMP ya está registrado.", 409);
        ReglasComunes.Exigir(dto.EspecialidadIds.Distinct().Count() == dto.EspecialidadIds.Length, "No repita especialidades.");
        foreach (var eid in dto.EspecialidadIds) ReglasComunes.Obtener(almacen.Especialidades, eid);
        // RN08: no quitar una asociación que ya tenga turnos, pues rompería su significado.
        ReglasComunes.Exigir(!almacen.Programaciones.Values.Any(p => p.MedicoId == id && !dto.EspecialidadIds.Contains(p.EspecialidadId)), "Una especialidad del médico tiene turnos asociados.", 409);
    }
    public MedicoItemDto Registrar(CrearMedicoDto dto)
    {
        lock (almacen.Compuerta)
        {
            Validar(dto);
            var nuevo = new MedicoItemDto(++almacen.UltimoMedico, dto.Nombres.Trim(), dto.Apellidos.Trim(), dto.Cmp, dto.EspecialidadIds.ToArray(), dto.Activo, ReglasComunes.Ahora, null);
            almacen.Medicos.Add(nuevo.Id, nuevo);
            return nuevo;
        }
    }
    /// <summary>PUT conserva ID y fecha de creación; actualiza los demás datos y la auditoría.</summary>
    public MedicoItemDto Actualizar(int id, CrearMedicoDto dto)
    {
        lock (almacen.Compuerta)
        {
            var anterior = BuscarPorId(id);
            Validar(dto, id);
            var nuevo = new MedicoItemDto(id, dto.Nombres.Trim(), dto.Apellidos.Trim(), dto.Cmp, dto.EspecialidadIds.ToArray(), dto.Activo, anterior.FechaCreacion, ReglasComunes.Ahora);
            almacen.Medicos[id] = nuevo;
            return nuevo;
        }
    }
    /// <summary>RN08: rechaza el borrado cuando existen referencias, para no dejar datos huérfanos.</summary>
    public void Eliminar(int id)
    {
        lock (almacen.Compuerta)
        {
            BuscarPorId(id);
            ReglasComunes.Exigir(!(almacen.Programaciones.Values.Any(p => p.MedicoId == id)), "El registro tiene datos asociados y no puede eliminarse.", 409);
            almacen.Medicos.Remove(id);
        }
    }
}
