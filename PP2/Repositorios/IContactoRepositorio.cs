using RegistroAsistencia.Models;

namespace RegistroAsistencia.Repositorios;

public interface IContactoRepositorio
{
    // Los contactos de todos los alumnos de la comisión
    List<ContactoEstudiante> ObtenerDeComision(string comision);

    void Agregar(ContactoEstudiante contacto);
}
