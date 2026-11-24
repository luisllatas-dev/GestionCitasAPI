// ARCHIVO DE INGRESO DE DATA: edita aquí los nombres y las listas de ejemplo.
// Se creó para separar la información ficticia de las reglas de los servicios.
// Se usan los mismos servicios que usa Scalar: si un dato de prueba incumple una regla, el arranque lo detectará.
// Los nombres son ficticios; DNI, CMP y teléfonos son valores de prueba, no personas verificadas.
using GestionCitasAPI.Models;
namespace GestionCitasAPI.Services;
public static class DatosPrueba
{
    /// <summary>Precarga una vez: especialidades, pacientes, médicos, turnos y finalmente citas.</summary>
    public static void Cargar(IServiceProvider proveedor)
    {
        var especialidades = proveedor.GetRequiredService<IEspecialidadServicio>();
        var pacientes = proveedor.GetRequiredService<IPacienteServicio>();
        var medicos = proveedor.GetRequiredService<IMedicoServicio>();
        var turnos = proveedor.GetRequiredService<IProgramacionServicio>();
        var citas = proveedor.GetRequiredService<ICitaServicio>();
        // Mañana evita que la simulación caduque por tener fechas escritas manualmente.
        var manana = DateOnly.FromDateTime(ReglasComunes.Ahora).AddDays(1);
        foreach (var nombre in new[] { "Medicina General", "Pediatría", "Cardiología", "Dermatología" })
            especialidades.Registrar(new() { Nombre = nombre }); // IDs 1, 2, 3, 4.

        // Lista de pacientes: aquí puedes agregar más filas. El ID se asigna automáticamente.
        var listaPacientes = new (string Nombres, string Apellidos)[]
        {
            ("Juan Carlos", "Pérez Gómez"), ("María Elena", "Torres Díaz"),
            ("Carlos Enrique", "Ríos López"), ("Valentina", "Mendoza Ruiz"),
            ("Rosa Elena", "Quispe Huamán"), ("Miguel Ángel", "Castro Medina"),
            ("Carmen Julia", "Díaz Romero"), ("Pedro Luis", "Herrera Soto"),
            ("María Fernanda", "Navarro Chávez"), ("Jorge Ricardo", "Salazar Campos"),
            ("Sofía Alejandra", "Paredes Núñez"), ("Ricardo Manuel", "Aguilar Cárdenas")
        };
        for (int i = 0; i < listaPacientes.Length; i++)
            pacientes.Registrar(new() { Nombres = listaPacientes[i].Nombres, Apellidos = listaPacientes[i].Apellidos,
                Dni = $"{90000001+i}", Telefono = $"{900000001+i}", Correo = $"paciente{i+1}@example.com", Activo = i != 11 });

        // Los IDs de médicos conservan el rango 101 del proyecto original. El CMP se envía sin el prefijo CMP-.
        var listaMedicos = new CrearMedicoDto[]
        {
            new() { Nombres = "Roberto", Apellidos = "Sánchez Mendoza", Cmp = "45890", EspecialidadIds = [3, 1] },
            new() { Nombres = "Ana Lucía", Apellidos = "Vargas Quispe", Cmp = "62310", EspecialidadIds = [2] },
            new() { Nombres = "Elena Patricia", Apellidos = "Vega Castillo", Cmp = "100003", EspecialidadIds = [3] },
            new() { Nombres = "Marco Antonio", Apellidos = "Rojas Fernández", Cmp = "100004", EspecialidadIds = [4] },
            new() { Nombres = "Lucía Isabel", Apellidos = "Sánchez Paredes", Cmp = "100005", EspecialidadIds = [1] },
            new() { Nombres = "Daniel Eduardo", Apellidos = "Flores Aguilar", Cmp = "100006", EspecialidadIds = [3] }
        };
        foreach (var medico in listaMedicos) medicos.Registrar(medico);

        // Lista de turnos: solo se admiten especialidades que el médico ya tenga asignadas.
        var listaTurnos = new CrearProgramacionDto[]
        {
            new() { MedicoId = 101, EspecialidadId = 3, Fecha = manana, HoraInicio = new(9,0), HoraFin = new(11,0) },
            new() { MedicoId = 102, EspecialidadId = 2, Fecha = manana, HoraInicio = new(9,0), HoraFin = new(11,0) },
            new() { MedicoId = 103, EspecialidadId = 3, Fecha = manana, HoraInicio = new(14,0), HoraFin = new(16,10) },
            new() { MedicoId = 101, EspecialidadId = 1, Fecha = manana, HoraInicio = new(14,0), HoraFin = new(16,0) },
            new() { MedicoId = 101, EspecialidadId = 3, Fecha = manana.AddDays(1), HoraInicio = new(9,0), HoraFin = new(11,0), Activo = false },
            new() { MedicoId = 104, EspecialidadId = 4, Fecha = manana, HoraInicio = new(9,0), HoraFin = new(11,0) },
            new() { MedicoId = 105, EspecialidadId = 1, Fecha = manana, HoraInicio = new(9,0), HoraFin = new(11,0) },
            new() { MedicoId = 106, EspecialidadId = 3, Fecha = manana, HoraInicio = new(9,0), HoraFin = new(11,0) },
            new() { MedicoId = 101, EspecialidadId = 3, Fecha = manana.AddDays(2), HoraInicio = new(9,0), HoraFin = new(10,10) },
            new() { MedicoId = 102, EspecialidadId = 2, Fecha = manana.AddDays(2), HoraInicio = new(14,0), HoraFin = new(16,0) }
        };
        foreach (var turno in listaTurnos) turnos.Registrar(turno);

        // Las citas comienzan en 1001. Su fecha y profesional se obtienen del turno elegido.
        var listaCitas = new CrearCitaDto[]
        {
            new() { PacienteId = 1, ProgramacionId = 1, HoraInicio = new(9,20), Motivo = "Control cardiológico preventivo" },
            new() { PacienteId = 4, ProgramacionId = 2, HoraInicio = new(10,0), Motivo = "Control de crecimiento y desarrollo" },
            new() { PacienteId = 3, ProgramacionId = 3, HoraInicio = new(14,0), Motivo = "Control de presión arterial" },
            new() { PacienteId = 2, ProgramacionId = 4, HoraInicio = new(14,20), Motivo = "Evaluación médica general" },
            new() { PacienteId = 5, ProgramacionId = 6, HoraInicio = new(9,0), Motivo = "Evaluación de manchas en la piel" },
            new() { PacienteId = 6, ProgramacionId = 7, HoraInicio = new(9,40), Motivo = "Consulta por molestias digestivas" },
            new() { PacienteId = 7, ProgramacionId = 8, HoraInicio = new(10,20), Motivo = "Revisión de electrocardiograma" },
            new() { PacienteId = 8, ProgramacionId = 1, HoraInicio = new(10,40), Motivo = "Chequeo preventivo" }
        };
        foreach (var cita in listaCitas) citas.Registrar(cita);
        citas.Cancelar(1008, new() { Motivo = "El paciente solicitó cancelar por motivos laborales" });
    }
}

