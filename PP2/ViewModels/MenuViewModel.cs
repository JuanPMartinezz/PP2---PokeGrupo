using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RegistroAsistencia.Services;
using System.Collections.ObjectModel;

namespace RegistroAsistencia.ViewModels;

/// <summary>
/// Menú lateral (hamburguesa): se elige la comisión y se abre el
/// calendario, la lista de alumnos, la asistencia o las entregas de esa
/// comisión.
/// </summary>
public partial class MenuViewModel : PantallaBase
{
    private readonly INavegacion _navegacion;

    private readonly ComisionService _comisiones;

    public ObservableCollection<string> Comisiones { get; } = new();

    [ObservableProperty]
    private string? comisionSeleccionada;

    public MenuViewModel(
        INavegacion navegacion,
        ComisionService comisiones)
    {
        _navegacion = navegacion;
        _comisiones = comisiones;

        Actualizar();
    }

    // Vuelve a leer las comisiones y mantiene la elegida si sigue
    public void Actualizar()
    {
        string? elegida = ComisionSeleccionada;

        Comisiones.Clear();

        foreach (string nombre in _comisiones.Todas())
        {
            Comisiones.Add(nombre);
        }

        ComisionSeleccionada = elegida is not null &&
                               Comisiones.Contains(elegida)
            ? elegida
            : Comisiones.FirstOrDefault();

        LimpiarMensaje();
    }

    [RelayCommand]
    private Task Inicio()
    {
        return _navegacion.VolverAlInicioAsync();
    }

    [RelayCommand]
    private Task Calendario() => Abrir(Destino.Calendario);

    [RelayCommand]
    private Task Lista() => Abrir(Destino.Lista);

    [RelayCommand]
    private Task Asistencia() => Abrir(Destino.Asistencia);

    [RelayCommand]
    private Task Entrega() => Abrir(Destino.Entrega);

    private async Task Abrir(Destino destino)
    {
        if (ComisionSeleccionada is null)
        {
            Rechazar("Elegí una comisión");
            return;
        }

        await _navegacion.VolverAlInicioAsync();
        await _navegacion.IrAAsync(destino, ComisionSeleccionada);
    }
}
