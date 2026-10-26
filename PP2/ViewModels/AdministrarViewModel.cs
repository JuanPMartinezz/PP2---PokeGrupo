using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RegistroAsistencia.Services;
using System.Collections.ObjectModel;

namespace RegistroAsistencia.ViewModels;

/// <summary>
/// Administración de datos: renombrar, vaciar o eliminar una comisión y
/// borrar todo. Las acciones que borran piden confirmación.
/// </summary>
public partial class AdministrarViewModel : PantallaBase
{
    private readonly IDialogos _dialogos;

    private readonly ComisionService _comisiones;

    public ObservableCollection<string> Comisiones { get; } = new();

    [ObservableProperty]
    private string? comisionSeleccionada;

    public AdministrarViewModel(
        IDialogos dialogos,
        ComisionService comisiones)
    {
        _dialogos = dialogos;
        _comisiones = comisiones;

        Recargar(null);
    }

    // Vuelve a leer las comisiones y deja elegida la indicada
    // (o la primera si ya no existe)
    private void Recargar(string? elegida)
    {
        Comisiones.Clear();

        foreach (string nombre in _comisiones.Todas())
        {
            Comisiones.Add(nombre);
        }

        ComisionSeleccionada = elegida is not null &&
                               Comisiones.Contains(elegida)
            ? elegida
            : Comisiones.FirstOrDefault();
    }

    private bool HayComision()
    {
        if (ComisionSeleccionada is not null)
        {
            return true;
        }

        Rechazar("Elegí una comisión");

        return false;
    }

    [RelayCommand]
    private async Task Renombrar()
    {
        if (!HayComision())
        {
            return;
        }

        string? nuevo = await _dialogos.PedirTextoAsync(
            "Renombrar comisión",
            $"Nuevo nombre para {ComisionSeleccionada}",
            "Comisión C");

        if (nuevo is not null)
        {
            AplicarRenombre(ComisionSeleccionada!, nuevo);
        }
    }

    private void AplicarRenombre(string actual, string nuevo)
    {
        string? error = _comisiones.Renombrar(actual, nuevo);

        if (error is not null)
        {
            Rechazar(error);
            return;
        }

        Recargar(nuevo.Trim());

        Informar($"{actual} ahora se llama {nuevo.Trim()}");
    }

    [RelayCommand]
    private async Task Vaciar()
    {
        if (!HayComision())
        {
            return;
        }

        bool acepta = await _dialogos.ConfirmarAsync(
            "Vaciar comisión",
            $"Se van a eliminar todos los alumnos de {ComisionSeleccionada} con sus asistencias, entregas y contactos. La comisión queda. No se puede deshacer.",
            "Vaciar",
            "Cancelar");

        if (acepta)
        {
            _comisiones.Vaciar(ComisionSeleccionada!);

            Informar($"{ComisionSeleccionada} quedó sin alumnos");
        }
    }

    [RelayCommand]
    private async Task Eliminar()
    {
        if (!HayComision())
        {
            return;
        }

        bool acepta = await _dialogos.ConfirmarAsync(
            "Eliminar comisión",
            $"Se va a eliminar {ComisionSeleccionada} con sus alumnos, registros y calendario. No se puede deshacer.",
            "Eliminar",
            "Cancelar");

        if (acepta)
        {
            string eliminada = ComisionSeleccionada!;

            _comisiones.Eliminar(eliminada);

            Recargar(null);

            Informar($"Se eliminó {eliminada}");
        }
    }

    [RelayCommand]
    private async Task BorrarTodo()
    {
        bool acepta = await _dialogos.ConfirmarAsync(
            "Borrar todo",
            "Se van a borrar todas las comisiones, alumnos y registros. La aplicación queda como recién instalada. No se puede deshacer.",
            "Borrar todo",
            "Cancelar");

        if (acepta)
        {
            _comisiones.BorrarTodo();

            Recargar(null);

            Informar("Se borraron todos los datos");
        }
    }
}
