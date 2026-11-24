// ExcepcionNegocio.cs: representa errores esperados, como un horario ocupado.
namespace GestionCitasAPI.Models;

// Hereda de Exception y entrega al middleware el mensaje y código HTTP.
public sealed class ExcepcionNegocio(int estado, string mensaje)
    : Exception(mensaje)
{
    // Código HTTP que devolverá la API, por ejemplo 400, 404 o 409.
    public int Estado { get; } = estado;
}
