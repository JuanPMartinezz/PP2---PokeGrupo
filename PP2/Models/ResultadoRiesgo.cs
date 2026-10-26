namespace RegistroAsistencia.Models;

public class ResultadoRiesgo
{
    public Estudiante Estudiante { get; set; } = new();

    // Asistencias
    public int TotalClases { get; set; }

    public int Ausencias { get; set; }

    public int Presentes { get; set; }

    // Entregas
    public int TotalEntregas { get; set; }

    public int Entregados { get; set; }

    public int NoEntregados { get; set; }

    public int EntregadosFueraDeTermino { get; set; }

    // Riesgo
    public decimal PorcentajeRiesgo { get; set; }

    // Nivel según los umbrales de la comisión (H7):
    // Alto, Medio, Bajo o Sin datos
    public string NivelRiesgo { get; set; } = string.Empty;

    // Contactos (H11): del más reciente al más antiguo
    public List<ContactoEstudiante> Contactos { get; set; } = new();

    public int CantidadContactos => Contactos.Count;

    public bool Contactado => Contactos.Count > 0;

    public DateTime? UltimoContacto =>
        Contactos.Max(c => (DateTime?)c.Fecha);

    public string EstadoContacto =>
        Contactado ? "Contactado" : "Sin contactar";

    public string UltimoContactoTexto =>
        UltimoContacto is DateTime fecha
            ? $"Último contacto: {fecha:dd/MM/yyyy}"
            : "Todavía no se lo contactó";

    // Texto corto para la lista: "Alto · 77%" o "Sin datos"
    public string RiesgoTexto =>
        NivelRiesgo == NivelesRiesgo.SinDatos
            ? NivelesRiesgo.SinDatos
            : $"{NivelRiesgo} · {PorcentajeRiesgo:0}%";

    // Verdadero si nunca se lo contactó o si el último contacto
    // fue antes de la fecha límite indicada
    public bool SinContactoDesde(DateTime limite)
    {
        return UltimoContacto is not DateTime ultimo ||
               ultimo.Date < limite.Date;
    }
}
