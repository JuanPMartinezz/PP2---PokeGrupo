namespace RegistroAsistencia.ViewModels;

/// <summary>
/// Un ViewModel que puede volver a cargar sus datos. La pantalla le
/// avisa cuando aparece, por si se volvió desde otra pantalla que
/// cambió los datos.
/// </summary>
public interface IActualizable
{
    void Actualizar();
}
