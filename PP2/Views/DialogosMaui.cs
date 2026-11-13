
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

    public async Task<string?> ElegirArchivoDeTextoAsync(string titulo)
    {
        var tipos = new FilePickerFileType(
            new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.Android, new[] { "text/csv", "text/plain" } },
                { DevicePlatform.WinUI, new[] { ".csv", ".txt" } },
                { DevicePlatform.iOS, new[] { "public.comma-separated-values-text" } },
                { DevicePlatform.MacCatalyst, new[] { "public.comma-separated-values-text" } }
            });

        FileResult? archivo = await FilePicker.Default.PickAsync(
            new PickOptions { PickerTitle = titulo, FileTypes = tipos });

        if (archivo is null)
        {
            return null;
        }

        using Stream flujo = await archivo.OpenReadAsync();
        using var lector = new StreamReader(flujo);

        return await lector.ReadToEndAsync();
    }

    public async Task AvisarAsync(string titulo, string mensaje)
    {
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await Shell.Current.DisplayAlertAsync(titulo, mensaje, "Aceptar");
        });
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
        var contenido = CrearContenido(pagina, titulo, mensaje, ejemplo);

        contenido.Children.Add(CrearBotones(
            pagina, aceptar, cancelar, seleccion));

        pagina.Content = new Grid
        {
            Padding = 24,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Fill,
            Children = { CrearTarjeta(contenido) }
        };

        return pagina;
    }

    private static VerticalStackLayout CrearContenido(
        VentanaPokeball pagina,
        string titulo,
        string mensaje,
        string? ejemplo)
    {
        var contenido = new VerticalStackLayout { Spacing = 16, Padding = 20 };

        contenido.Children.Add(new Label
        {
            Text = titulo,
            FontSize = 21,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#252B35")
        });

        contenido.Children.Add(new Label
        {
            Text = mensaje,
            FontSize = 14,
            TextColor = Color.FromArgb("#687383")
        });

        if (ejemplo != null)
        {
            pagina.Entrada = CrearEntrada(ejemplo);
            contenido.Children.Add(pagina.Entrada);
        }

        return contenido;
    }

    private static Entry CrearEntrada(string ejemplo)
    {
        return new Entry
        {
            Placeholder = ejemplo,
            BackgroundColor = Color.FromArgb("#F4F6F9"),
            TextColor = Color.FromArgb("#252B35"),
            HeightRequest = 48
        };
    }

    private static Grid CrearBotones(
        VentanaPokeball pagina,
        string aceptar,
        string cancelar,
        Action<bool, string?> seleccion)
    {
        Button botonCancelar = CrearBoton(cancelar, "#687383");
        Button botonAceptar = CrearBoton(aceptar, "#D9363E");

        botonCancelar.Clicked += async (s, e) =>
            await ResponderAsync(pagina, seleccion, false, null);

        botonAceptar.Clicked += async (s, e) =>
            await ResponderAsync(pagina, seleccion, true, pagina.Entrada?.Text);

        Grid botones = CrearGridDeDosColumnas();
        botones.Add(botonCancelar, 0);
        botones.Add(botonAceptar, 1);

        return botones;
    }

    private static Grid CrearGridDeDosColumnas()
    {
        return new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star)
            },
            ColumnSpacing = 10
        };
    }

    private static async Task ResponderAsync(
        VentanaPokeball pagina,
        Action<bool, string?> seleccion,
        bool aceptado,
        string? texto)
    {
        await pagina.CerrarAsync();
        seleccion(aceptado, texto);
    }

    private static Button CrearBoton(string texto, string color)
    {
        return new Button
        {
            Text = texto,
            BackgroundColor = Color.FromArgb(color),
            TextColor = Colors.White,
            FontAttributes = FontAttributes.Bold,
            CornerRadius = 10,
            HeightRequest = 46
        };
    }

    private static Border CrearTarjeta(VerticalStackLayout contenido)
    {
        return new Border
        {
            BackgroundColor = Colors.White,
            Stroke = Color.FromArgb("#D5DCE5"),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 16 },
            MaximumWidthRequest = 440,
            Content = new VerticalStackLayout
            {
                Spacing = 0,
                Children =
                {
                    CrearEncabezado(),
                    CrearFranja(7, "#252B35"),
                    CrearFranja(3, "#E0444C"),
                    contenido
                }
            }
        };
    }

    private static BoxView CrearFranja(double alto, string color)
    {
        return new BoxView
        {
            HeightRequest = alto,
            Color = Color.FromArgb(color)
        };
    }

    private static Grid CrearEncabezado()
    {
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

        encabezado.Add(CrearPokeball(), 0);
        encabezado.Add(CrearIdentidad(), 1);
        encabezado.Add(CrearCirculos(), 2);

        return encabezado;
    }

    private static Image CrearPokeball()
    {
        return new Image
        {
            Source = "pokeball.png",
            WidthRequest = 48,
            HeightRequest = 48,
            Aspect = Aspect.AspectFit,
            VerticalOptions = LayoutOptions.Center
        };
    }

    private static VerticalStackLayout CrearIdentidad()
    {
        return new VerticalStackLayout
        {
            Spacing = 2,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                CrearEtiqueta("Registro de asistencia", 16, Colors.White, true),
                CrearEtiqueta(
                    "P O K E G R U P O", 9, Color.FromArgb("#FFE2E2"), false)
            }
        };
    }

    private static Label CrearEtiqueta(
        string texto,
        double tamano,
        Color color,
        bool negrita)
    {
        return new Label
        {
            Text = texto,
            FontSize = tamano,
            FontAttributes = negrita
                ? FontAttributes.Bold
                : FontAttributes.None,
            // El texto en negrita es el título; el otro lleva letras espaciadas
            CharacterSpacing = negrita ? 0 : 1,
            TextColor = color
        };
    }

    private static HorizontalStackLayout CrearCirculos()
    {
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

        return circulos;
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
