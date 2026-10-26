using RegistroAsistencia.Models;

namespace RegistroAsistencia.Repositorios;

/// <summary>
/// Guarda las comisiones y la configuración de riesgo de cada una.
/// </summary>
public interface IComisionRepositorio
{
    List<string> ObtenerTodas();

    void Agregar(string nombre);

    // Alumnos, calendario y configuración la referencian por su
    // identificador, así que el nombre se cambia en un solo lugar
    void Renombrar(string actual, string nuevo);

    // Elimina la comisión con su calendario y su configuración.
    // Los alumnos se eliminan aparte.
    void Eliminar(string nombre);

    // Nulo si la comisión todavía no configuró nada
    UmbralesRiesgo? ObtenerUmbrales(string comision);

    void GuardarUmbrales(string comision, UmbralesRiesgo umbrales);
}
