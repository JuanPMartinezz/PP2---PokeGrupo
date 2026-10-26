namespace RegistroAsistencia.Models;

public class ActividadCalendario
{
    public string Comision { get; set; } = string.Empty;

    public string Cuatrimestre { get; set; } = string.Empty;

    public DateTime Fecha { get; set; }

    public TipoActividad Tipo { get; set; }

    public string Descripcion { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"{Fecha:dd/MM/yyyy} - {Tipo}: {Descripcion}";
    }
}
