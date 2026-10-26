using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RegistroAsistencia.Models;
using RegistroAsistencia.Services;
using System.Globalization;

namespace RegistroAsistencia.ViewModels;

public partial class ConfiguracionViewModel : ObservableObject
{
    // Los valores se manejan como texto para poder detectar
    // cuando el usuario escribe algo que no es un número.

    private string pesoAsistenciaTexto = "";

    public string PesoAsistenciaTexto
    {
        get => pesoAsistenciaTexto;

        set => SetProperty(
            ref pesoAsistenciaTexto,
            value);
    }

    private string riesgoFueraDeTerminoTexto = "";

    public string RiesgoFueraDeTerminoTexto
    {
        get => riesgoFueraDeTerminoTexto;

        set => SetProperty(
            ref riesgoFueraDeTerminoTexto,
            value);
    }

    private string riesgoNoEntregadoTexto = "";

    public string RiesgoNoEntregadoTexto
    {
        get => riesgoNoEntregadoTexto;

        set => SetProperty(
            ref riesgoNoEntregadoTexto,
            value);
    }

    private string riesgoMedioTexto = "";

    public string RiesgoMedioTexto
    {
        get => riesgoMedioTexto;

        set => SetProperty(
            ref riesgoMedioTexto,
            value);
    }

    private string riesgoAltoTexto = "";

    public string RiesgoAltoTexto
    {
        get => riesgoAltoTexto;

        set => SetProperty(
            ref riesgoAltoTexto,
            value);
    }

    private string resumenActual = "";

    public string ResumenActual
    {
        get => resumenActual;

        private set => SetProperty(
            ref resumenActual,
            value);
    }

    private string mensaje = "";

    public string Mensaje
    {
        get => mensaje;

        set => SetProperty(
            ref mensaje,
            value);
    }

    public ConfiguracionViewModel()
    {
        CargarUmbrales();
    }

    [RelayCommand]
    private void CargarUmbrales()
    {
        UmbralesRiesgo actuales =
            DatosSistema.Umbrales;

        PesoAsistenciaTexto =
            Formatear(actuales.PesoAsistencia);

        RiesgoFueraDeTerminoTexto =
            Formatear(actuales.RiesgoFueraDeTermino);

        RiesgoNoEntregadoTexto =
            Formatear(actuales.RiesgoNoEntregado);

        RiesgoMedioTexto =
            Formatear(actuales.RiesgoMedio);

        RiesgoAltoTexto =
            Formatear(actuales.RiesgoAlto);

        ResumenActual =
            $"Asistencia {Formatear(actuales.PesoAsistencia)} % | " +
            $"Entregas {Formatear(actuales.PesoEntregas)} % | " +
            $"Fuera de término {Formatear(actuales.RiesgoFueraDeTermino)} % | " +
            $"No entregado {Formatear(actuales.RiesgoNoEntregado)} % | " +
            $"Riesgo medio desde {Formatear(actuales.RiesgoMedio)} % | " +
            $"Riesgo alto desde {Formatear(actuales.RiesgoAlto)} %";
    }

    [RelayCommand]
    private void GuardarUmbrales()
    {
        if (!TryLeerNumero(
                PesoAsistenciaTexto,
                "el peso de la asistencia",
                out decimal pesoAsistencia))
        {
            return;
        }

        if (!TryLeerNumero(
                RiesgoFueraDeTerminoTexto,
                "el riesgo por entrega fuera de término",
                out decimal riesgoFueraDeTermino))
        {
            return;
        }

        if (!TryLeerNumero(
                RiesgoNoEntregadoTexto,
                "el riesgo por entrega no entregada",
                out decimal riesgoNoEntregado))
        {
            return;
        }

        if (!TryLeerNumero(
                RiesgoMedioTexto,
                "el umbral de riesgo medio",
                out decimal riesgoMedio))
        {
            return;
        }

        if (!TryLeerNumero(
                RiesgoAltoTexto,
                "el umbral de riesgo alto",
                out decimal riesgoAlto))
        {
            return;
        }

        var nuevosUmbrales = new UmbralesRiesgo
        {
            PesoAsistencia =
                pesoAsistencia,

            RiesgoFueraDeTermino =
                riesgoFueraDeTermino,

            RiesgoNoEntregado =
                riesgoNoEntregado,

            RiesgoMedio =
                riesgoMedio,

            RiesgoAlto =
                riesgoAlto
        };

        try
        {
            DatosSistema.GuardarUmbrales(
                nuevosUmbrales);

            CargarUmbrales();

            Mensaje =
                "Umbrales guardados correctamente";
        }
        catch (InvalidOperationException ex)
        {
            // Valor fuera de rango: el mensaje explica cuál es el problema.
            Mensaje = ex.Message;
        }
        catch (Exception ex)
        {
            Mensaje =
                $"No se pudieron guardar los umbrales: {ex.Message}";
        }
    }

    [RelayCommand]
    private void RestablecerValores()
    {
        try
        {
            DatosSistema.RestablecerUmbrales();

            CargarUmbrales();

            Mensaje =
                "Se restablecieron los valores predeterminados";
        }
        catch (Exception ex)
        {
            Mensaje =
                $"No se pudieron restablecer los umbrales: {ex.Message}";
        }
    }

    private bool TryLeerNumero(
        string texto,
        string campo,
        out decimal valor)
    {
        valor = 0;

        if (string.IsNullOrWhiteSpace(texto))
        {
            Mensaje =
                $"El valor ingresado para {campo} no es válido: no puede estar vacío";

            return false;
        }

        // Acepta tanto "33,5" como "33.5".
        string normalizado =
            texto.Trim().Replace(',', '.');

        bool esNumero = decimal.TryParse(
            normalizado,
            NumberStyles.AllowDecimalPoint |
            NumberStyles.AllowLeadingSign,
            CultureInfo.InvariantCulture,
            out valor);

        if (!esNumero)
        {
            Mensaje =
                $"El valor ingresado para {campo} no es válido: debe ser un número";
        }

        return esNumero;
    }

    private static string Formatear(decimal valor)
    {
        return valor.ToString("0.##");
    }
}
