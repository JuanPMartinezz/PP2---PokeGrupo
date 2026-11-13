using RegistroAsistencia.Models;

namespace RegistroAsistencia.Repositorios;

public interface IEstudianteRepositorio
{
    List<Estudiante> ObtenerTodos();

    List<Estudiante> ObtenerDeComision(string comision);

    Estudiante? Buscar(string legajo);

    void Agregar(Estudiante estudiante);

    void Actualizar(Estudiante estudiante);

    void Eliminar(string legajo);
}