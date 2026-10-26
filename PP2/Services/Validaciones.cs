namespace RegistroAsistencia.Services;

/// <summary>
/// Reglas de validación de los formularios que no necesitan consultar
/// datos. Devuelven el motivo del rechazo, escrito para quien usa la
/// aplicación, o null si todo está bien.
/// </summary>
public static class Validaciones
{
    public const int LegajoMinimo = 3;

    public const int LegajoMaximo = 8;

    public static string? Alumno(
        string nombre,
        string apellido,
        string legajo,
        string? comision)
    {
        if (string.IsNullOrWhiteSpace(nombre) ||
            string.IsNullOrWhiteSpace(apellido) ||
            string.IsNullOrWhiteSpace(legajo))
        {
            return "Completá nombre, apellido y legajo";
        }

        return string.IsNullOrWhiteSpace(comision)
            ? "Elegí la comisión del alumno"
            : null;
    }

    // El legajo son solo números, de 3 a 8 dígitos
    public static string? Legajo(string legajo)
    {
        string limpio = legajo.Trim();

        bool valido = limpio.Length >= LegajoMinimo &&
                      limpio.Length <= LegajoMaximo &&
                      limpio.All(c => c is >= '0' and <= '9');

        return valido
            ? null
            : $"El legajo debe tener entre {LegajoMinimo} y {LegajoMaximo} " +
              "dígitos, solo números";
    }

    public static string? NombreComision(string nombre)
    {
        return string.IsNullOrWhiteSpace(nombre)
            ? "Escribí el nombre de la comisión"
            : null;
    }

    public static string? FechaContacto(DateTime fecha)
    {
        return fecha.Date > DateTime.Today
            ? "La fecha del contacto no puede ser futura"
            : null;
    }

    public static string? FechaAsistencia(DateTime fecha)
    {
        return fecha.Date > DateTime.Today
            ? "La fecha de la asistencia no puede ser futura"
            : null;
    }
}
