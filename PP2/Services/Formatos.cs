using System.Globalization;

namespace RegistroAsistencia.Services;

/// <summary>
/// Formato de fechas para mostrar. Se define en un solo lugar.
/// </summary>
public static class Formatos
{
    private static readonly CultureInfo Cultura = new("es-AR");

    // Ejemplo: "Lunes 26 de octubre"
    public static string FechaLarga(DateTime fecha)
    {
        string texto = fecha.ToString("dddd d 'de' MMMM", Cultura);

        return char.ToUpper(texto[0], Cultura) + texto.Substring(1);
    }

    // Ejemplo: "26/10/2026"
    public static string FechaCorta(DateTime fecha)
    {
        return fecha.ToString("dd/MM/yyyy", Cultura);
    }
}
