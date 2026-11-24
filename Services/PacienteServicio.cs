// Administra pacientes en memoria. Se creó/ajustó para RN01 (duplicados) y RN08 (referencias).
using GestionCitasAPI.Models;
namespace GestionCitasAPI.Services;
/// <summary>Contrato que permite al controlador trabajar sin depender de la implementación de almacenamiento.</summary>
public interface IPacienteServicio
{
    List<PacienteItemDto> ListarTodos();
    PacienteItemDto BuscarPorId(int id);
    PacienteItemDto Registrar(CrearPacienteDto dto);
    PacienteItemDto Actualizar(int id, CrearPacienteDto dto);
    void Eliminar(int id);
}
public sealed class PacienteServicioEnMemoria(AlmacenEnMemoria almacen) : IPacienteServicio
{
    /// <summary>Devuelve una lista nueva para no entregar la colección interna.</summary>
    public List<PacienteItemDto> ListarTodos() { lock (almacen.Compuerta) return almacen.Pacientes.Values.ToList(); }
    public PacienteItemDto BuscarPorId(int id) { lock (almacen.Compuerta) return ReglasComunes.Obtener(almacen.Pacientes, id); }
    /// <summary>Comprueba formatos y duplicados antes de reservar un ID.</summary>
    private void Validar(CrearPacienteDto dto, int id = 0)
    {
        ReglasComunes.Validar(dto);
        ReglasComunes.Exigir(dto.Nombres.Trim().Length >= 2 && dto.Apellidos.Trim().Length >= 2, "Complete nombres y apellidos.");
        ReglasComunes.Exigir(!almacen.Pacientes.Values.Any(p => p.Id != id && p.Dni == dto.Dni), "El DNI ya está registrado.", 409);
    }
    public PacienteItemDto Registrar(CrearPacienteDto dto)
    {
        lock (almacen.Compuerta)
        {
            Validar(dto);
            var nuevo = new PacienteItemDto(++almacen.UltimoPaciente, dto.Nombres.Trim(), dto.Apellidos.Trim(), dto.Dni, dto.Telefono, dto.Correo?.Trim(), dto.Activo, ReglasComunes.Ahora, null);
            almacen.Pacientes.Add(nuevo.Id, nuevo);
            return nuevo;
        }
    }
    /// <summary>PUT conserva ID y fecha de creación; actualiza los demás datos y la auditoría.</summary>
    public PacienteItemDto Actualizar(int id, CrearPacienteDto dto)
    {
        lock (almacen.Compuerta)
        {
            var anterior = BuscarPorId(id);
            Validar(dto, id);
            var nuevo = new PacienteItemDto(id, dto.Nombres.Trim(), dto.Apellidos.Trim(), dto.Dni, dto.Telefono, dto.Correo?.Trim(), dto.Activo, anterior.FechaCreacion, ReglasComunes.Ahora);
            almacen.Pacientes[id] = nuevo;
            return nuevo;
        }
    }
    /// <summary>RN08: rechaza el borrado cuando existen referencias, para no dejar datos huérfanos.</summary>
    public void Eliminar(int id)
    {
        lock (almacen.Compuerta)
        {
            BuscarPorId(id);
            ReglasComunes.Exigir(!(almacen.Citas.Values.Any(c => c.PacienteId == id)), "El registro tiene datos asociados y no puede eliminarse.", 409);
            almacen.Pacientes.Remove(id);
        }
    }
}
