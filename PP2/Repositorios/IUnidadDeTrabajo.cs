namespace RegistroAsistencia.Repositorios;

/// <summary>
/// Agrupa varias operaciones en una sola: o se guardan todas juntas o,
/// si algo falla, no se guarda ninguna. Sirve para las cargas grandes,
/// que así también son mucho más rápidas.
/// </summary>
public interface IUnidadDeTrabajo
{
    T EnTransaccion<T>(Func<T> operacion);
}
