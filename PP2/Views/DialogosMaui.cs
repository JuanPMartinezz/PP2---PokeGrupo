using RegistroAsistencia.Services;

namespace RegistroAsistencia.Views;

/// <summary>
/// Avisos y preguntas con los cuadros de diálogo de MAUI.
/// </summary>
public sealed class DialogosMaui : IDialogos
{
    public Task<bool> ConfirmarAsync(
        string titulo,
        string mensaje,
        string aceptar,
        string cancelar)
    {
        return Shell.Current.DisplayAlert(
            titulo,
            mensaje,
            aceptar,
            cancelar);
    }

    public Task<string?> PedirTextoAsync(
        string titulo,
        string mensaje,
        string ejemplo)
    {
        return Shell.Current.DisplayPromptAsync(
            titulo,
            mensaje,
            "Aceptar",
            "Cancelar",
            ejemplo);
    }
}
