// Archivo creado para compartir listas y una única compuerta entre todos los servicios.
// Evita dependencias circulares y protege una reserva completa frente a solicitudes simultáneas.
using GestionCitasAPI.Models;
namespace GestionCitasAPI.Services;
public sealed class AlmacenEnMemoria
{
    // Solo los servicios del mismo proyecto acceden a estas colecciones; no se exponen como endpoints.
    internal readonly object Compuerta = new();
    internal readonly Dictionary<int, PacienteItemDto> Pacientes = [];
    internal readonly Dictionary<int, MedicoItemDto> Medicos = [];
    internal readonly Dictionary<int, EspecialidadItemDto> Especialidades = [];
    internal readonly Dictionary<int, ProgramacionItemDto> Programaciones = [];
    internal readonly Dictionary<int, CitaItemDto> Citas = [];
    // Conservamos los rangos del proyecto original. No usamos Max+1 para evitar reutilizar IDs borrados.
    internal int UltimoPaciente, UltimoMedico = 100, UltimaEspecialidad, UltimaProgramacion, UltimaCita = 1000;
}
