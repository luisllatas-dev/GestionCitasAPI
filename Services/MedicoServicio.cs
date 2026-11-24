namespace GestionCitasAPI.Services;

using GestionCitasAPI.Models;

public interface IMedicoServicio
{
    List<MedicoItemDto> ListarTodos();
    MedicoItemDto? BuscarPorId(int id);
    MedicoItemDto Registrar(CrearMedicoDto dto);
    MedicoItemDto? Actualizar(int id, CrearMedicoDto dto);
    bool Eliminar(int id);
}

/// <summary>
/// Servicio en memoria para el CRUD Básico de Médicos con Auditoría (RN-MED-04).
/// Recibe IEspecialidadServicio por Inyección de Dependencias para obtener el nombre de la especialidad.
/// </summary>
public class MedicoServicioEnMemoria(IEspecialidadServicio especialidadServicio) : IMedicoServicio
{
    private readonly List<MedicoItemDto> _medicos = new()
    {
        new(101, "Roberto", "Sánchez Mendoza", 1, "Cardiología", "CMP-45890", DateTime.Now.AddDays(-10), null),
        new(102, "Ana Lucía", "Vargas Quispe", 2, "Pediatría", "CMP-62310", DateTime.Now.AddDays(-7), null)
    };

    public List<MedicoItemDto> ListarTodos() => _medicos.ToList();

    public MedicoItemDto? BuscarPorId(int id) => _medicos.FirstOrDefault(m => m.Id == id);

    public MedicoItemDto Registrar(CrearMedicoDto dto)
    {
        var especialidad = especialidadServicio.BuscarPorId(dto.EspecialidadId);
        int nuevoId = _medicos.Any() ? _medicos.Max(m => m.Id) + 1 : 101;

        // RN-MED-04 (Auditoría): FechaCreacion = DateTime.Now, FechaActualizacion = null
        var nuevo = new MedicoItemDto(
            Id: nuevoId,
            Nombres: dto.Nombres,
            Apellidos: dto.Apellidos,
            EspecialidadId: dto.EspecialidadId,
            NombreEspecialidad: especialidad?.Nombre ?? "Especialidad no especificada",
            Cmp: dto.Cmp,
            FechaCreacion: DateTime.Now,
            FechaActualizacion: null
        );

        _medicos.Add(nuevo);
        return nuevo;
    }

    public MedicoItemDto? Actualizar(int id, CrearMedicoDto dto)
    {
        int index = _medicos.FindIndex(m => m.Id == id);
        if (index == -1) return null;

        var existente = _medicos[index];
        var especialidad = especialidadServicio.BuscarPorId(dto.EspecialidadId);

        // RN-MED-04 (Auditoría): Conserva FechaCreacion y actualiza FechaActualizacion
        var actualizado = existente with
        {
            Nombres = dto.Nombres,
            Apellidos = dto.Apellidos,
            EspecialidadId = dto.EspecialidadId,
            NombreEspecialidad = especialidad?.Nombre ?? existente.NombreEspecialidad,
            Cmp = dto.Cmp,
            FechaActualizacion = DateTime.Now
        };

        _medicos[index] = actualizado;
        return actualizado;
    }

    public bool Eliminar(int id)
    {
        var item = BuscarPorId(id);
        if (item is null) return false;
        return _medicos.Remove(item);
    }
}
