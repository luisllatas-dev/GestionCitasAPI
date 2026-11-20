namespace GestionCitasAPI.Services;

using GestionCitasAPI.Models;

public interface IPacienteServicio
{
    List<PacienteItemDto> ListarTodos();
    PacienteItemDto? BuscarPorId(int id);
    PacienteItemDto Registrar(CrearPacienteDto dto);
    PacienteItemDto? Actualizar(int id, CrearPacienteDto dto);
    bool Eliminar(int id);
}

/// <summary>
/// Servicio en memoria para el CRUD Básico de Pacientes con Auditoría (RN-PAC-06).
/// </summary>
public class PacienteServicioEnMemoria : IPacienteServicio
{
    private readonly List<PacienteItemDto> _pacientes = new()
    {
        new(1, "Juan Carlos", "Pérez Gómez", "71234567", "987654321", "juan.perez@correo.com", DateTime.Now.AddDays(-5), null),
        new(2, "María Elena", "Torres Díaz", "45678912", "912345678", "maria.torres@correo.com", DateTime.Now.AddDays(-3), null)
    };

    public List<PacienteItemDto> ListarTodos() => _pacientes.ToList();

    public PacienteItemDto? BuscarPorId(int id) => _pacientes.FirstOrDefault(p => p.Id == id);

    public PacienteItemDto Registrar(CrearPacienteDto dto)
    {
        int nuevoId = _pacientes.Any() ? _pacientes.Max(p => p.Id) + 1 : 1;

        // RN-PAC-06 (Auditoría): FechaCreacion = DateTime.Now, FechaActualizacion = null
        var nuevo = new PacienteItemDto(
            Id: nuevoId,
            Nombres: dto.Nombres,
            Apellidos: dto.Apellidos,
            Dni: dto.Dni,
            Telefono: dto.Telefono,
            Correo: dto.Correo,
            FechaCreacion: DateTime.Now,
            FechaActualizacion: null
        );

        _pacientes.Add(nuevo);
        return nuevo;
    }

    public PacienteItemDto? Actualizar(int id, CrearPacienteDto dto)
    {
        int index = _pacientes.FindIndex(p => p.Id == id);
        if (index == -1) return null;

        var existente = _pacientes[index];

        // RN-PAC-06 (Auditoría): Conserva FechaCreacion original y asigna FechaActualizacion = DateTime.Now
        // Usamos la expresión 'with' propia de los record en C#
        var actualizado = existente with
        {
            Nombres = dto.Nombres,
            Apellidos = dto.Apellidos,
            Dni = dto.Dni,
            Telefono = dto.Telefono,
            Correo = dto.Correo,
            FechaActualizacion = DateTime.Now
        };

        _pacientes[index] = actualizado;
        return actualizado;
    }

    public bool Eliminar(int id)
    {
        var item = BuscarPorId(id);
        if (item is null) return false;
        return _pacientes.Remove(item);
    }
}

