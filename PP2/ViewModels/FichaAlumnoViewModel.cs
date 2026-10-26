using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RegistroAsistencia.Models;
using RegistroAsistencia.Services;
using System.Collections.ObjectModel;

namespace RegistroAsistencia.ViewModels;

/// <summary>
/// Ficha de un alumno (H9): datos completos, estadísticas, riesgo y
/// contactos. Desde acá se registra un contacto, se editan los datos o
/// se elimina al alumno.
/// </summary>
public partial class FichaAlumnoViewModel : ObservableObject, IActualizable
{
    private readonly string _legajo;

    private readonly INavegacion _navegacion;

    private readonly IDialogos _dialogos;

    private readonly RiesgoService _riesgo;

    private readonly NominaService _nomina;

    public ObservableCollection<string> Contactos { get; } = new();

    [ObservableProperty]
    private string nombreCompleto = "";

    [ObservableProperty]
    private string datos = "";

    [ObservableProperty]
    private string riesgoTexto = "";

    [ObservableProperty]
    private string nivel = "";

    [ObservableProperty]
    private string asistenciaTexto = "";

    [ObservableProperty]
    private string entregasTexto = "";

    [ObservableProperty]
    private string contactoTexto = "";

    public FichaAlumnoViewModel(
        string legajo,
        INavegacion navegacion,
        IDialogos dialogos,
        RiesgoService riesgo,
        NominaService nomina)
    {
        _legajo = legajo;
        _navegacion = navegacion;
        _dialogos = dialogos;
        _riesgo = riesgo;
        _nomina = nomina;

        Actualizar();
    }

    public void Actualizar()
    {
        Estudiante? estudiante = _nomina.Buscar(_legajo);

        if (estudiante is null)
        {
            return;
        }

        ResultadoRiesgo r = _riesgo.ObtenerResultado(estudiante);

        NombreCompleto = $"{estudiante.Apellido}, {estudiante.Nombre}";
        Datos = $"Legajo {estudiante.Legajo} · {estudiante.Comision}";
        RiesgoTexto = r.RiesgoTexto;
        Nivel = r.NivelRiesgo;
        AsistenciaTexto = TextoAsistencia(r);
        EntregasTexto = TextoEntregas(r);
        ContactoTexto = r.UltimoContactoTexto;

        CargarContactos(r);
    }

    private static string TextoAsistencia(ResultadoRiesgo r)
    {
        return r.TotalClases == 0
            ? "Asistencia: todavía no se tomó asistencia en la comisión"
            : $"Asistencia: {r.Presentes} presentes y {r.Ausencias} ausencias " +
              $"en {r.TotalClases} clases dictadas";
    }

    private static string TextoEntregas(ResultadoRiesgo r)
    {
        return r.TotalEntregas == 0
            ? "Entregas: todavía no hay entregas registradas"
            : $"Entregas: {r.Entregados} entregadas, " +
              $"{r.EntregadosFueraDeTermino} fuera de término y " +
              $"{r.NoEntregados} sin entregar de {r.TotalEntregas}";
    }

    private void CargarContactos(ResultadoRiesgo r)
    {
        Contactos.Clear();

        foreach (ContactoEstudiante c in r.Contactos
                     .OrderByDescending(c => c.Fecha))
        {
            Contactos.Add(
                $"{Formatos.FechaCorta(c.Fecha)} · {c.EstadoTexto}: {c.Resultado}");
        }
    }

    [RelayCommand]
    private Task RegistrarContacto()
    {
        return _navegacion.IrAAsync(Destino.Contacto, _legajo);
    }

    [RelayCommand]
    private Task Editar()
    {
        return _navegacion.IrAAsync(Destino.AlumnoEditar, _legajo);
    }

    [RelayCommand]
    private async Task Eliminar()
    {
        bool acepta = await _dialogos.ConfirmarAsync(
            "Eliminar alumno",
            $"Se va a eliminar a {NombreCompleto} con sus asistencias, entregas y contactos. No se puede deshacer.",
            "Eliminar",
            "Cancelar");

        if (!acepta)
        {
            return;
        }

        _nomina.Eliminar(_legajo);

        await _navegacion.VolverAsync();
    }
}
