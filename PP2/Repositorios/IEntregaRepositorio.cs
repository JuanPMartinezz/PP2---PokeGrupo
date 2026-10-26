using RegistroAsistencia.Models;

namespace RegistroAsistencia.Repositorios;

public interface IEntregaRepositorio
{
    // Las entregas de todos los alumnos de la comisión
    List<Entrega> ObtenerDeComision(string comision);

    Entrega? Buscar(string legajo, int trabajoId);

    void Agregar(Entrega entrega);

    void ActualizarEstado(
        string legajo,
        int trabajoId,
        EstadoEntrega estado);

    // Devuelve el trabajo con ese nombre (sin distinguir mayúsculas)
    // y, si no existe, lo crea
    Trabajo ObtenerOCrearTrabajo(string nombre);
}
