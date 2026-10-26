using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RegistroAsistencia.Services;
using System.Collections.ObjectModel;

namespace RegistroAsistencia.ViewModels;

/// <summary>
/// Administración de datos: renombrar, vaciar o eliminar una comisión y
/// borrar todo. Las acciones que borran piden confirmación.
/// </summary>
public partial class AdministrarViewModel : ObservableObject
{
    private readonly IDialogos _dialogos;

    public ObservableCollection<string> Comisiones =>
        DatosSistema.Comisiones;

    [ObservableProperty]
    private string? comisionSeleccionada;

    [ObservableProperty]
    private string mensaje = "";

    public AdministrarViewModel(IDialogos dialogos)
    {
        _dialogos = dialogos;

        comisionSeleccionada = Comisiones.FirstOrDefault();
    }

    private bool HayComision()
    {
        if (ComisionSeleccionada is not null)
        {
            return true;
        }

        Mensaje = "Elegí una comisión";

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

        if (nuevo is null)
        {
            return;
        }

        AplicarRenombre(ComisionSeleccionada!, nuevo);
    }

    private void AplicarRenombre(string actual, string nuevo)
    {
        try
        {
            DatosSistema.RenombrarComision(actual, nuevo);

            ComisionSeleccionada = nuevo.Trim();

            Mensaje = $"{actual} ahora se llama {nuevo.Trim()}";
        }
        catch (InvalidOperationException ex)
        {
            Mensaje = ex.Message;
        }
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
            DatosSistema.VaciarComision(ComisionSeleccionada!);

            Mensaje = $"{ComisionSeleccionada} quedó sin alumnos";
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

            DatosSistema.EliminarComision(eliminada);

            ComisionSeleccionada = Comisiones.FirstOrDefault();

            Mensaje = $"Se eliminó {eliminada}";
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
            DatosSistema.BorrarTodosLosRegistros();

            ComisionSeleccionada = Comisiones.FirstOrDefault();

            Mensaje = "Se borraron todos los datos";
        }
    }
}
