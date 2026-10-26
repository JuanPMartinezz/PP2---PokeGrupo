using RegistroAsistencia.Services;
using RegistroAsistencia.ViewModels;

namespace RegistroAsistencia.Views;

/// <summary>
/// Abre cada pantalla con su ViewModel. Es el único lugar que conoce
/// qué página va con qué ViewModel.
/// </summary>
public sealed class NavegacionMaui : INavegacion
{
    private readonly IDialogos _dialogos = new DialogosMaui();

    private readonly RiesgoService _riesgo = new();

    private readonly RegistroService _registro = new();

    public async Task IrAAsync(Destino destino, string? dato = null)
    {
        Page pagina = CrearPagina(destino, dato ?? "");

        await Shell.Current.Navigation.PushAsync(pagina);
    }

    public Task VolverAsync()
    {
        return Shell.Current.Navigation.PopAsync();
    }

    // Cuando una pantalla vuelve a aparecer, recarga sus datos
    public static void ActualizarAlAparecer(ContentPage pagina)
    {
        pagina.Appearing += (_, _) =>
            (pagina.BindingContext as IActualizable)?.Actualizar();
    }

    private Page CrearPagina(Destino destino, string dato)
    {
        return destino switch
        {
            Destino.Comision => Armar(
                new ComisionPage(),
                new ComisionDetalleViewModel(dato, this)),
            Destino.Asistencia => Armar(
                new AsistenciaPage(),
                new AsistenciaViewModel(dato, this, _dialogos, _registro)),
            Destino.Entrega => Armar(
                new EntregaPage(),
                new EntregaViewModel(dato, _dialogos, _registro)),
            Destino.Lista => Armar(
                new ListaAlumnosPage(),
                new ListaAlumnosViewModel(dato, this, _riesgo)),
            Destino.Ficha => Armar(
                new FichaAlumnoPage(),
                new FichaAlumnoViewModel(dato, this, _dialogos, _riesgo)),
            Destino.Contacto => Armar(
                new ContactoPage(),
                new ContactoViewModel(dato, this)),
            _ => CrearPaginaAuxiliar(destino, dato)
        };
    }

    private Page CrearPaginaAuxiliar(Destino destino, string dato)
    {
        return destino switch
        {
            Destino.Calendario => Armar(
                new CalendarioPage(),
                new CalendarioViewModel(dato)),
            Destino.ConfigRiesgo => Armar(
                new ConfigRiesgoPage(),
                new ConfigRiesgoViewModel(dato)),
            Destino.NuevaComision => Armar(
                new NuevaComisionPage(),
                new NuevaComisionViewModel(this)),
            Destino.AlumnoNuevo => Armar(
                new AlumnoFormPage(),
                new AlumnoFormViewModel(this)),
            Destino.AlumnoEditar => Armar(
                new AlumnoFormPage(),
                new AlumnoFormViewModel(this, dato)),
            Destino.Administrar => Armar(
                new AdministrarPage(),
                new AdministrarViewModel(_dialogos)),
            _ => throw new ArgumentOutOfRangeException(nameof(destino))
        };
    }

    private static ContentPage Armar(ContentPage pagina, object viewModel)
    {
        pagina.BindingContext = viewModel;

        ActualizarAlAparecer(pagina);

        return pagina;
    }
}
