namespace RegistroAsistencia.Models;

/// <summary>
/// Texto con el que se muestra cada estado. Se define en un solo lugar.
/// </summary>
public static class EstadoTextos
{
    public static string Texto(this EstadoAsistencia estado)
    {
        return estado == EstadoAsistencia.Presente
            ? "Presente"
            : "Ausente";
    }

    public static string Texto(this EstadoEntrega estado)
    {
        return estado switch
        {
            EstadoEntrega.Entregado => "Entregado",
            EstadoEntrega.EntregadoFueraDeTermino => "Fuera de término",
            _ => "No entregado"
        };
    }
}
