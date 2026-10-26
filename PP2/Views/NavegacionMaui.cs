using Microsoft.Extensions.DependencyInjection;
using RegistroAsistencia.Services;
using RegistroAsistencia.ViewModels;

namespace RegistroAsistencia.Views;

/// <summary>
/// Abre cada pantalla con su ViewModel. Los ViewModels se crean con el
/// contenedor de dependencias, que les pasa los servicios que piden; el
/// dato del destino (comisión o legajo) se entrega como argumento.
/// </summary>
public sealed class NavegacionMaui : INavegacion
{
    private readonly IServiceProvider _servicios;

    public NavegacionMaui(IServiceProvider servicios)
    {
        _servicios = servicios;
    }

    public async Task IrAAsync(Destino destino, string? dato = null)
    {
        Page pagina = CrearPagina(destino, dato ?? "");

        await Shell.Current.Navigation.PushAsync(pagina);
    }

    public Task VolverAsync()
    {
        return Shell.Current.Navigation.PopAsync();
    }

    public Task VolverAlInicioAsync()
    {
        Shell.Current.FlyoutIsPresented = false;

        return Shell.Current.Navigation.PopToRootAsync();
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
            Destino.Comision => Armar<ComisionPage, ComisionDetalleViewModel>(dato),
            Destino.Asistencia => Armar<AsistenciaPage, AsistenciaViewModel>(dato),
            Destino.Entrega => Armar<EntregaPage, EntregaViewModel>(dato),
            Destino.Lista => Armar<ListaAlumnosPage, ListaAlumnosViewModel>(dato),
            Destino.Ficha => Armar<FichaAlumnoPage, FichaAlumnoViewModel>(dato),
            Destino.Contacto => Armar<ContactoPage, ContactoViewModel>(dato),
            _ => CrearPaginaAuxiliar(destino, dato)
        };
    }

    private Page CrearPaginaAuxiliar(Destino destino, string dato)
    {
        return destino switch
        {
            Destino.Calendario => Armar<CalendarioPage, CalendarioViewModel>(dato),
            Destino.ConfigRiesgo => Armar<ConfigRiesgoPage, ConfigRiesgoViewModel>(dato),
            Destino.NuevaComision => Armar<NuevaComisionPage, NuevaComisionViewModel>(),
            Destino.AlumnoNuevo => Armar<AlumnoFormPage, AlumnoFormViewModel>(),
            Destino.AlumnoEditar => Armar<AlumnoFormPage, AlumnoFormViewModel>(dato),
            Destino.Administrar => Armar<AdministrarPage, AdministrarViewModel>(),
            _ => throw new ArgumentOutOfRangeException(nameof(destino))
        };
    }

    // Crea la página y su ViewModel; los argumentos son los datos
    // que el ViewModel necesita además de los servicios
    private ContentPage Armar<TPagina, TViewModel>(params object[] argumentos)
        where TPagina : ContentPage, new()
    {
        var pagina = new TPagina
        {
            BindingContext = ActivatorUtilities.CreateInstance<TViewModel>(
                _servicios,
                argumentos)
        };

        ActualizarAlAparecer(pagina);

        return pagina;
    }
}
