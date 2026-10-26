using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RegistroAsistencia.Services;

namespace RegistroAsistencia.ViewModels;

/// <summary>
/// Pantalla de una comisión: la fecha de hoy y los accesos a tomar
/// asistencia, registrar entregas, ver la lista de alumnos, los trabajos
/// y la configuración del riesgo.
/// </summary>
public partial class ComisionDetalleViewModel : ObservableObject, IActualizable
{
    private readonly string _comision;

    private readonly INavegacion _navegacion;

    public string Comision => _comision;

    public string FechaHoy => Formatos.FechaLarga(DateTime.Today);

    [ObservableProperty]
    private string resumenAlumnos = "";

    [ObservableProperty]
    private string estadoAsistencia = "";

    public ComisionDetalleViewModel(
        string comision,
        INavegacion navegacion)
    {
        _comision = comision;
        _navegacion = navegacion;

        Actualizar();
    }

    public void Actualizar()
    {
        int alumnos = DatosSistema.EstudiantesDe(_comision).Count;

        ResumenAlumnos = $"{DateTime.Today.Year} · {alumnos} alumnos";

        EstadoAsistencia = DatosSistema.HayAsistencia(_comision, DateTime.Today)
            ? "Ya se tomó hoy. Podés corregirla"
            : "Todavía no se tomó";
    }

    [RelayCommand]
    private Task TomarAsistencia()
    {
        return _navegacion.IrAAsync(Destino.Asistencia, _comision);
    }

    [RelayCommand]
    private Task RegistrarEntrega()
    {
        return _navegacion.IrAAsync(Destino.Entrega, _comision);
    }

    [RelayCommand]
    private Task VerLista()
    {
        return _navegacion.IrAAsync(Destino.Lista, _comision);
    }

    [RelayCommand]
    private Task VerCalendario()
    {
        return _navegacion.IrAAsync(Destino.Calendario, _comision);
    }

    [RelayCommand]
    private Task ConfigurarRiesgo()
    {
        return _navegacion.IrAAsync(Destino.ConfigRiesgo, _comision);
    }
}
