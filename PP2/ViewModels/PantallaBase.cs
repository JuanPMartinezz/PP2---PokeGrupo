using CommunityToolkit.Mvvm.ComponentModel;

namespace RegistroAsistencia.ViewModels;

/// <summary>
/// Base de las pantallas que muestran un mensaje. El mensaje es de error
/// o de aviso: la pantalla lo pinta distinto según EsError.
/// </summary>
public abstract partial class PantallaBase : ObservableObject
{
    [ObservableProperty]
    private string mensaje = "";

    [ObservableProperty]
    private bool esError;

    // Mensaje de que todo salió bien
    protected void Informar(string texto)
    {
        Mensaje = texto;
        EsError = false;
    }

    // Mensaje que explica por qué no se pudo hacer algo
    protected void Rechazar(string texto)
    {
        Mensaje = texto;
        EsError = true;
    }

    protected void LimpiarMensaje()
    {
        Informar("");
    }
}
