namespace RegistroAsistencia.Services;

/// <summary>
/// Cuatrimestres del calendario. El texto de cada cuatrimestre
/// se arma en un solo lugar.
/// </summary>
public static class Cuatrimestres
{
    private const int UltimoMesDelPrimero = 7;

    public static string De(DateTime fecha)
    {
        string numero = fecha.Month <= UltimoMesDelPrimero
            ? "1er"
            : "2do";

        return $"{fecha.Year} - {numero} cuatrimestre";
    }

    public static string Actual()
    {
        return De(DateTime.Today);
    }

    // Cuatrimestres del año anterior, el actual y el siguiente
    public static List<string> Disponibles()
    {
        int anio = DateTime.Today.Year;

        var lista = new List<string>();

        for (int a = anio - 1; a <= anio + 1; a++)
        {
            lista.Add(De(new DateTime(a, 1, 1)));
            lista.Add(De(new DateTime(a, 12, 1)));
        }

        return lista;
    }
}
