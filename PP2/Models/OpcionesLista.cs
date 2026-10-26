namespace RegistroAsistencia.Models;

// Orden de la lista de alumnos
public enum OrdenLista
{
    ApellidoAZ,
    ApellidoZA,
    MayorRiesgo
}

// Filtro por contacto (H11)
public enum FiltroContacto
{
    Todos,
    SinContactar,
    SinContactoReciente
}

// Filtro por nivel de riesgo
public enum FiltroNivel
{
    Todos,
    Alto,
    Medio,
    Bajo
}
