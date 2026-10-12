using RegistroAsistencia.Services;

namespace RegistroAsistencia;

/// <summary>Inicializa SQLite antes de mostrar las pantallas que consumen sus datos.</summary>
public sealed class StartupPage : ContentPage
{
    private bool _iniciado;

    public StartupPage()
    {
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
        if (_iniciado) return;
        _iniciado = true;
        InicializarAsync();
    }

    private async void InicializarAsync()
    {
        try
        {
            await Task.Run(DatosSistema.Inicializar);
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (Window is not null)
                    Window.Page = new AppShell();
            });
        }
        catch (Exception ex)
        {
            MainThread.BeginInvokeOnMainThread(() =>
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
            });
        }
    }
}
