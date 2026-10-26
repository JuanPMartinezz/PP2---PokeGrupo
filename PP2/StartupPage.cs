using Microsoft.Extensions.DependencyInjection;
using RegistroAsistencia.Services;

namespace RegistroAsistencia;

/// <summary>Abre la base de datos y prepara las comisiones antes de mostrar las pantallas.</summary>
public sealed class StartupPage : ContentPage
{
    private readonly IServiceProvider _servicios;

    private bool _iniciado;

    public StartupPage(IServiceProvider servicios)
    {
        _servicios = servicios;

        Content = new VerticalStackLayout
        {
            Padding = 24,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                new ActivityIndicator { IsRunning = true, HorizontalOptions = LayoutOptions.Center },
                new Label { Text = "Preparando la base de datos...", HorizontalOptions = LayoutOptions.Center }
            }
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (_iniciado)
        {
            return;
        }

        _iniciado = true;
        InicializarAsync();
    }

    private async void InicializarAsync()
    {
        try
        {
            await Task.Run(Preparar);

            MainThread.BeginInvokeOnMainThread(AbrirAplicacion);
        }
        catch (Exception ex)
        {
            MainThread.BeginInvokeOnMainThread(() => MostrarError(ex));
        }
    }

    private void Preparar()
    {
        _servicios.GetRequiredService<ComisionService>().Inicializar();
    }

    private void AbrirAplicacion()
    {
        if (Window is not null)
        {
            Window.Page = new AppShell(_servicios);
        }
    }

    private void MostrarError(Exception ex)
    {
        Content = new VerticalStackLayout
        {
            Padding = 24,
            Children =
            {
                new Label { Text = "No se pudo iniciar la base de datos SQLite.", FontAttributes = FontAttributes.Bold },
                new Label { Text = ex.Message }
            }
        };
    }
}
