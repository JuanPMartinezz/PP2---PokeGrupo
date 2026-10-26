using RegistroAsistencia.Models;

namespace RegistroAsistencia.Repositorios;

public interface IEstudianteRepositorio
{
    List<Estudiante> ObtenerTodos();

    // Ordenados por apellido y nombre
    List<Estudiante> ObtenerDeComision(string comision);

    Estudiante? Buscar(string legajo);

    void Agregar(Estudiante estudiante);

    void Actualizar(Estudiante estudiante);

    // Elimina al estudiante junto con sus asistencias, entregas y contactos
    void Eliminar(string legajo);
}
