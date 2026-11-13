
using RegistroAsistencia.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace RegistroAsistencia.Views;

public sealed class DialogosMaui : IDialogos
{
    public async Task<bool> ConfirmarAsync(
        string titulo,
        string mensaje,
        string aceptar,
        string cancelar)
    {
        var resultado = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        var pagina = CrearVentana(
            titulo,
            mensaje,
            aceptar,
            cancelar,
            null,
            (aceptado, texto) => resultado.TrySetResult(aceptado));

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await Shell.Current.Navigation.PushModalAsync(pagina);
        });

        return await resultado.Task;
    }

    public async Task<string?> PedirTextoAsync(
        string titulo,
        string mensaje,
        string ejemplo)
    {
        var resultado = new TaskCompletionSource<string?>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        var pagina = CrearVentana(
            titulo,
            mensaje,
            "Aceptar",
            "Cancelar",
            ejemplo,
            (aceptado, texto) =>
                resultado.TrySetResult(aceptado ? texto : null));

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await Shell.Current.Navigation.PushModalAsync(pagina);
        });

        return await resultado.Task;
    }

    private VentanaPokeball CrearVentana(
        string titulo,
        string mensaje,
        string aceptar,
        string cancelar,
        string? ejemplo,
        Action<bool, string?> seleccion)
    {
        var pagina = new VentanaPokeball();

        var contenido = new VerticalStackLayout
        {
            Spacing = 16,
            Padding = 20
        };

        var encabezado = new Grid
        {
            BackgroundColor = Color.FromArgb("#C92532"),
            Padding = new Thickness(16, 12),
            ColumnDefinitions =
            {
                new ColumnDefinition(55),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            }
        };

        encabezado.Add(new Image
        {
            Source = "pokeball.png",
            WidthRequest = 48,
            HeightRequest = 48,
            Aspect = Aspect.AspectFit,
            VerticalOptions = LayoutOptions.Center
        }, 0);

        var identidad = new VerticalStackLayout
        {
            Spacing = 2,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                new Label
                {
                    Text = "Registro de asistencia",
                    FontSize = 16,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.White
                },
                new Label
                {
                    Text = "P O K E G R U P O",
                    FontSize = 9,
                    CharacterSpacing = 1,
                    TextColor = Color.FromArgb("#FFE2E2")
                }
            }
        };

        encabezado.Add(identidad, 1);

        var circulos = new HorizontalStackLayout
        {
            Spacing = 6,
            VerticalOptions = LayoutOptions.Center
        };

        foreach (var color in new[] { "#39C9F2", "#FFD83D", "#80D83C" })
        {
            circulos.Children.Add(new Border
            {
                WidthRequest = 17,
                HeightRequest = 17,
                BackgroundColor = Color.FromArgb(color),
                Stroke = Color.FromArgb("#252B35"),
                StrokeThickness = 1,
                StrokeShape = new Ellipse()
            });
        }

        encabezado.Add(circulos, 2);

        var tituloLabel = new Label
        {
            Text = titulo,
            FontSize = 21,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#252B35")
        };

        var mensajeLabel = new Label
        {
            Text = mensaje,
            FontSize = 14,
            TextColor = Color.FromArgb("#687383")
        };

        contenido.Children.Add(tituloLabel);
        contenido.Children.Add(mensajeLabel);

        if (ejemplo != null)
        {
            pagina.Entrada = new Entry
            {
                Placeholder = ejemplo,
                BackgroundColor = Color.FromArgb("#F4F6F9"),
                TextColor = Color.FromArgb("#252B35"),
                HeightRequest = 48
            };

            contenido.Children.Add(pagina.Entrada);
        }

        var botones = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star)
            },
            ColumnSpacing = 10
        };

        var botonCancelar = new Button
        {
            Text = cancelar,
            BackgroundColor = Color.FromArgb("#687383"),
            TextColor = Colors.White,
            FontAttributes = FontAttributes.Bold,
            CornerRadius = 10,
            HeightRequest = 46
        };

        botonCancelar.Clicked += async (s, e) =>
        {
            await pagina.CerrarAsync();
            seleccion(false, null);
        };

        var botonAceptar = new Button
        {
            Text = aceptar,
            BackgroundColor = Color.FromArgb("#D9363E"),
            TextColor = Colors.White,
            FontAttributes = FontAttributes.Bold,
            CornerRadius = 10,
            HeightRequest = 46
        };

        botonAceptar.Clicked += async (s, e) =>
        {
            var texto = pagina.Entrada?.Text;

            await pagina.CerrarAsync();
            seleccion(true, texto);
        };

        botones.Add(botonCancelar, 0);
        botones.Add(botonAceptar, 1);

        contenido.Children.Add(botones);

        var tarjeta = new Border
        {
            BackgroundColor = Colors.White,
            Stroke = Color.FromArgb("#D5DCE5"),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle
            {
                CornerRadius = 16
            },
            MaximumWidthRequest = 440,
            Content = new VerticalStackLayout
            {
                Spacing = 0,
                Children =
                {
                    encabezado,
                    new BoxView
                    {
                        HeightRequest = 7,
                        Color = Color.FromArgb("#252B35")
                    },
                    new BoxView
                    {
                        HeightRequest = 3,
                        Color = Color.FromArgb("#E0444C")
                    },
                    contenido
                }
            }
        };

        pagina.Content = new Grid
        {
            Padding = 24,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Fill,
            Children = { tarjeta }
        };

        return pagina;
    }
}

public sealed class VentanaPokeball : ContentPage
{
    public Entry? Entrada { get; set; }

    public VentanaPokeball()
    {
        BackgroundColor = Color.FromArgb("#99000000");
    }

    public async Task CerrarAsync()
    {
        await Shell.Current.Navigation.PopModalAsync();
    }
}
