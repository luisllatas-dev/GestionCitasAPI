namespace GestionCitasAPI.Services;

using GestionCitasAPI.Models;

public interface IEspecialidadServicio
{
    List<EspecialidadItemDto> ListarTodas();
    EspecialidadItemDto? BuscarPorId(int id);
    EspecialidadItemDto Registrar(CrearEspecialidadDto dto);
    EspecialidadItemDto? Actualizar(int id, CrearEspecialidadDto dto);
    bool Eliminar(int id);
}

/// <summary>
/// Servicio en memoria para el CRUD Básico de Especialidades Médicas con estado Activo y Auditoría.
/// </summary>
public class EspecialidadServicioEnMemoria : IEspecialidadServicio
{
    private readonly List<EspecialidadItemDto> _especialidades = new()
    {
        new(1, "Cardiología", "Diagnóstico y tratamiento de enfermedades del corazón y sistema circulatorio.", true, DateTime.Now.AddDays(-15), null),
        new(2, "Pediatría", "Atención médica integral de bebés, niños y adolescentes.", true, DateTime.Now.AddDays(-15), null),
        new(3, "Dermatología", "Cuidado y tratamiento de enfermedades de la piel, cabello y uñas.", true, DateTime.Now.AddDays(-12), null)
    };

    public List<EspecialidadItemDto> ListarTodas() => _especialidades.ToList();

    public EspecialidadItemDto? BuscarPorId(int id) => _especialidades.FirstOrDefault(e => e.Id == id);

    public EspecialidadItemDto Registrar(CrearEspecialidadDto dto)
    {
        int nuevoId = _especialidades.Any() ? _especialidades.Max(e => e.Id) + 1 : 1;

        var nueva = new EspecialidadItemDto(
            Id: nuevoId,
            Nombre: dto.Nombre,
            Descripcion: dto.Descripcion,
            Activo: dto.Activo,
            FechaCreacion: DateTime.Now,
            FechaActualizacion: null
        );

        _especialidades.Add(nueva);
        return nueva;
    }

    public EspecialidadItemDto? Actualizar(int id, CrearEspecialidadDto dto)
    {
        int index = _especialidades.FindIndex(e => e.Id == id);
        if (index == -1) return null;

        var existente = _especialidades[index];

        var actualizada = existente with
        {
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            Activo = dto.Activo,
            FechaActualizacion = DateTime.Now
        };

        _especialidades[index] = actualizada;
        return actualizada;
    }

    public bool Eliminar(int id)
    {
        var item = BuscarPorId(id);
        if (item is null) return false;
        return _especialidades.Remove(item);
    }
}
