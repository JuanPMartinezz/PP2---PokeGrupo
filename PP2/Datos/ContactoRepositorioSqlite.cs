using RegistroAsistencia.Models;
using RegistroAsistencia.Repositorios;

namespace RegistroAsistencia.Datos;

public sealed class ContactoRepositorioSqlite : IContactoRepositorio
{
    private readonly BaseDatos _db;

    public ContactoRepositorioSqlite(BaseDatos db)
    {
        _db = db;
    }

    public List<ContactoEstudiante> ObtenerDeComision(string comision)
    {
        if (_db.IdDeComision(comision) is not int id)
        {
            return new();
        }

        var estudiantes = _db.Conexion.Table<EstudianteDb>()
            .Where(e => e.ComisionId == id)
            .ToList()
            .ToDictionary(e => e.Legajo, e => e.AEstudiante(comision));

        return _db.Conexion.Query<ContactoDb>(
                "SELECT c.* FROM Contactos c " +
                "JOIN Estudiantes e ON e.Legajo = c.LegajoEstudiante " +
                "WHERE e.ComisionId = ?",
                id)
            .Select(fila => ADominio(fila, estudiantes[fila.LegajoEstudiante]))
            .ToList();
    }

    public void Agregar(ContactoEstudiante contacto)
    {
        _db.Conexion.Insert(new ContactoDb
        {
            LegajoEstudiante = contacto.Estudiante.Legajo,
            Fecha = Conversiones.GuardarFecha(contacto.Fecha),
            Atendio = contacto.Atendio,
            Resultado = contacto.Resultado
        });
    }

    private static ContactoEstudiante ADominio(
        ContactoDb fila,
        Estudiante estudiante)
    {
        return new ContactoEstudiante
        {
            Estudiante = estudiante,
            Fecha = Conversiones.LeerFecha(fila.Fecha),

            // Los contactos anteriores no guardaban este dato:
            // se los considera atendidos
            Atendio = fila.Atendio ?? true,

            Resultado = fila.Resultado
        };
    }
}
