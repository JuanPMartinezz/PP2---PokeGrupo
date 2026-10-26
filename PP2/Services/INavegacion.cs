namespace RegistroAsistencia.Services;

// Pantallas a las que se puede ir
public enum Destino
{
    Comision,
    Asistencia,
    Entrega,
    Lista,
    Ficha,
    Contacto,
    Calendario,
    ConfigRiesgo,
    NuevaComision,
    AlumnoNuevo,
    AlumnoEditar,
    Administrar
}

/// <summary>
/// Navegación entre pantallas. Los ViewModels piden "ir a" un destino
/// y la aplicación decide qué pantalla abrir.
/// </summary>
public interface INavegacion
{
    // El dato es el nombre de la comisión o el legajo del alumno,
    // según el destino
    Task IrAAsync(Destino destino, string? dato = null);

    Task VolverAsync();
}
