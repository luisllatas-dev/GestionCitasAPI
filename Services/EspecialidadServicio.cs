// Administra especialidades en memoria. Se creó/ajustó para RN01 (duplicados) y RN08 (referencias).
using GestionCitasAPI.Models;
namespace GestionCitasAPI.Services;
/// <summary>Contrato que permite al controlador trabajar sin depender de la implementación de almacenamiento.</summary>
public interface IEspecialidadServicio
{
    List<EspecialidadItemDto> ListarTodos();
    EspecialidadItemDto BuscarPorId(int id);
    EspecialidadItemDto Registrar(CrearEspecialidadDto dto);
    EspecialidadItemDto Actualizar(int id, CrearEspecialidadDto dto);
    void Eliminar(int id);
}
public sealed class EspecialidadServicioEnMemoria(AlmacenEnMemoria almacen) : IEspecialidadServicio
{
    /// <summary>Devuelve una lista nueva para no entregar la colección interna.</summary>
    public List<EspecialidadItemDto> ListarTodos() { lock (almacen.Compuerta) return almacen.Especialidades.Values.ToList(); }
    public EspecialidadItemDto BuscarPorId(int id) { lock (almacen.Compuerta) return ReglasComunes.Obtener(almacen.Especialidades, id); }
    /// <summary>Comprueba formatos y duplicados antes de reservar un ID.</summary>
    private void Validar(CrearEspecialidadDto dto, int id = 0)
    {
        ReglasComunes.Validar(dto);
        var nombre = ReglasComunes.Texto(dto.Nombre);
        ReglasComunes.Exigir(nombre.Length >= 2, "El nombre debe tener al menos 2 caracteres.");
        ReglasComunes.Exigir(!almacen.Especialidades.Values.Any(e => e.Id != id && e.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase)), "La especialidad ya existe.", 409);
    }
    public EspecialidadItemDto Registrar(CrearEspecialidadDto dto)
    {
        lock (almacen.Compuerta)
        {
            Validar(dto);
            var nuevo = new EspecialidadItemDto(++almacen.UltimaEspecialidad, ReglasComunes.Texto(dto.Nombre), dto.Activo, ReglasComunes.Ahora, null);
            almacen.Especialidades.Add(nuevo.Id, nuevo);
            return nuevo;
        }
    }
    /// <summary>PUT conserva ID y fecha de creación; actualiza los demás datos y la auditoría.</summary>
    public EspecialidadItemDto Actualizar(int id, CrearEspecialidadDto dto)
    {
        lock (almacen.Compuerta)
        {
            var anterior = BuscarPorId(id);
            Validar(dto, id);
            var nuevo = new EspecialidadItemDto(id, ReglasComunes.Texto(dto.Nombre), dto.Activo, anterior.FechaCreacion, ReglasComunes.Ahora);
            almacen.Especialidades[id] = nuevo;
            return nuevo;
        }
    }
    /// <summary>RN08: rechaza el borrado cuando existen referencias, para no dejar datos huérfanos.</summary>
    public void Eliminar(int id)
    {
        lock (almacen.Compuerta)
        {
            BuscarPorId(id);
            ReglasComunes.Exigir(!(almacen.Medicos.Values.Any(m => m.EspecialidadIds.Contains(id)) || almacen.Programaciones.Values.Any(p => p.EspecialidadId == id)), "El registro tiene datos asociados y no puede eliminarse.", 409);
            almacen.Especialidades.Remove(id);
        }
    }
}
