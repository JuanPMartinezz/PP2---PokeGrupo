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
public partial class MenuViewModel : ObservableObject
{
    private readonly INavegacion _navegacion;

    public ObservableCollection<string> Comisiones { get; } = new();

    [ObservableProperty]
    private string? comisionSeleccionada;

    [ObservableProperty]
    private string mensaje = "";

    public MenuViewModel(INavegacion navegacion)
    {
        _navegacion = navegacion;

        Actualizar();
    }

    // Vuelve a leer las comisiones y mantiene la elegida si sigue
    public void Actualizar()
    {
        string? elegida = ComisionSeleccionada;

        Comisiones.Clear();

        foreach (string nombre in DatosSistema.Comisiones)
        {
            Comisiones.Add(nombre);
        }

        ComisionSeleccionada = elegida is not null &&
                               Comisiones.Contains(elegida)
            ? elegida
            : Comisiones.FirstOrDefault();

        Mensaje = "";
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
            Mensaje = "Elegí una comisión";
            return;
        }

        await _navegacion.VolverAlInicioAsync();
        await _navegacion.IrAAsync(destino, ComisionSeleccionada);
    }
}
