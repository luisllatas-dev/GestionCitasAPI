// Utilidades compartidas: fecha de Lima, validación de DTOs, búsqueda y cruce de intervalos.
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using GestionCitasAPI.Models;
namespace GestionCitasAPI.Services;
public static class ReglasComunes
{
    public const int MinutosCita = 20;
    // Misma zona horaria aunque el servidor se ejecute en otro país. Auditoría y agenda usan Lima.
    public static DateTime Ahora => DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(-5)).DateTime;
    public static string Texto(string texto) => Regex.Replace(texto.Trim(), @"\s+", " ");
    // Permite citas consecutivas: terminar a las 10:00 y comenzar a las 10:00 no es cruce.
    public static bool Cruza(DateTime a, DateTime b, DateTime c, DateTime d) => a < d && c < b;
    public static T Obtener<T>(Dictionary<int,T> datos, int id) => datos.TryGetValue(id, out var valor)
        ? valor : throw new ExcepcionNegocio(404, $"{typeof(T).Name.Replace("ItemDto", "")} con ID {id} no existe.");
    // También valida llamadas desde DatosPrueba, que no pasan por los controladores.
    public static void Validar(object dto)
    {
        var errores = new List<ValidationResult>();
        if (!Validator.TryValidateObject(dto, new ValidationContext(dto), errores, true))
            throw new ExcepcionNegocio(400, string.Join(" ", errores.Select(e => e.ErrorMessage)));
    }
    public static void Exigir(bool condicion, string mensaje, int estado = 400)
    {
        if (!condicion) throw new ExcepcionNegocio(estado, mensaje);
    }
}

