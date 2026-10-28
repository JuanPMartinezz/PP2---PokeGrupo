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

    public const int LargoMaximoPersona = 40;

    public const int LargoMaximoNombre = 60;

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

        return Persona(nombre, "El nombre") ??
               Persona(apellido, "El apellido") ??
               (string.IsNullOrWhiteSpace(comision)
                   ? "Elegí la comisión del alumno"
                   : null);
    }

    // Nombres y apellidos: letras, espacios, puntos, guiones y apóstrofes,
    // hasta LargoMaximoPersona caracteres
    private static string? Persona(string valor, string etiqueta)
    {
        string limpio = valor.Trim();

        if (limpio.Length > LargoMaximoPersona)
        {
            return $"{etiqueta} no puede tener más de " +
                   $"{LargoMaximoPersona} caracteres";
        }

        return limpio.All(EsCaracterDeNombre)
            ? null
            : $"{etiqueta} solo puede tener letras, espacios, puntos, " +
              "guiones y apóstrofes";
    }

    private static bool EsCaracterDeNombre(char c)
    {
        return char.IsLetter(c) || c is ' ' or '.' or '-' or '\'';
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
        if (string.IsNullOrWhiteSpace(nombre))
        {
            return "Escribí el nombre de la comisión";
        }

        return nombre.Trim().Length > LargoMaximoNombre
            ? $"El nombre de la comisión no puede tener más de {LargoMaximoNombre} caracteres"
            : null;
    }

    public static string? NombreTrabajo(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            return "Escribí el nombre del trabajo";
        }

        return nombre.Trim().Length > LargoMaximoNombre
            ? $"El nombre del trabajo no puede tener más de {LargoMaximoNombre} caracteres"
            : null;
    }

    // La entrega se cierra cuando el trabajo ya venció (o vence hoy):
    // antes, los alumnos todavía pueden entregarlo
    public static string? CierreDeEntrega(DateTime fechaDelTrabajo)
    {
        return fechaDelTrabajo.Date > DateTime.Today
            ? $"El trabajo vence el {Formatos.FechaCorta(fechaDelTrabajo)}: " +
              "todavía no se puede cerrar la entrega"
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
