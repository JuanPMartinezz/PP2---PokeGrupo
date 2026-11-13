namespace RegistroAsistencia.Services;

/// <summary>
/// Avisos y preguntas al usuario. Los ViewModels piden confirmaciones
/// a través de esta interfaz y no conocen ninguna pantalla concreta.
/// </summary>
public interface IDialogos
{
    Task<bool> ConfirmarAsync(
        string titulo,
        string mensaje,
        string aceptar,
        string cancelar);

    // Devuelve el texto escrito, o null si el usuario canceló
    Task<string?> PedirTextoAsync(
        string titulo,
        string mensaje,
        string ejemplo);

    // Deja elegir un archivo de texto y devuelve su contenido, o null si
    // el usuario canceló
    Task<string?> ElegirArchivoDeTextoAsync(string titulo);

    Task AvisarAsync(string titulo, string mensaje);
}
