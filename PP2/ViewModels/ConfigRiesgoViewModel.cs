using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RegistroAsistencia.Models;
using RegistroAsistencia.Services;
using System.Globalization;

namespace RegistroAsistencia.ViewModels;

/// <summary>
/// Configuración del cálculo de riesgo de una comisión (H7): cuánto pesa
/// la asistencia contra las entregas, cuánto riesgo suma cada estado de
/// entrega y desde qué porcentaje el riesgo es medio o alto.
/// </summary>
public partial class ConfigRiesgoViewModel : ObservableObject
{
    private readonly string _comision;

    public string Comision => _comision;

    // Se escriben como texto para poder avisar cuando no es un número
    [ObservableProperty]
    private string pesoAsistencia = "";

    [ObservableProperty]
    private string riesgoFueraDeTermino = "";

    [ObservableProperty]
    private string riesgoNoEntregado = "";

    [ObservableProperty]
    private string riesgoMedio = "";

    [ObservableProperty]
    private string riesgoAlto = "";

    [ObservableProperty]
    private string mensaje = "";

    public ConfigRiesgoViewModel(string comision)
    {
        _comision = comision;

        Mostrar(DatosSistema.ObtenerUmbrales(comision));
    }

    private void Mostrar(UmbralesRiesgo u)
    {
        PesoAsistencia = u.PesoAsistencia.ToString("0.##", Cultura);
        RiesgoFueraDeTermino = u.RiesgoFueraDeTermino.ToString("0.##", Cultura);
        RiesgoNoEntregado = u.RiesgoNoEntregado.ToString("0.##", Cultura);
        RiesgoMedio = u.RiesgoMedio.ToString("0.##", Cultura);
        RiesgoAlto = u.RiesgoAlto.ToString("0.##", Cultura);
    }

    private static readonly CultureInfo Cultura = CultureInfo.InvariantCulture;

    // Acepta coma o punto como separador decimal
    private static bool TryNumero(string texto, out decimal valor)
    {
        return decimal.TryParse(
            texto.Trim().Replace(',', '.'),
            NumberStyles.Number,
            Cultura,
            out valor);
    }

    private UmbralesRiesgo? Leer()
    {
        bool validos =
            TryNumero(PesoAsistencia, out decimal peso) &
            TryNumero(RiesgoFueraDeTermino, out decimal fuera) &
            TryNumero(RiesgoNoEntregado, out decimal no) &
            TryNumero(RiesgoMedio, out decimal medio) &
            TryNumero(RiesgoAlto, out decimal alto);

        if (!validos)
        {
            return null;
        }

        return new UmbralesRiesgo
        {
            PesoAsistencia = peso,
            RiesgoFueraDeTermino = fuera,
            RiesgoNoEntregado = no,
            RiesgoMedio = medio,
            RiesgoAlto = alto
        };
    }

    [RelayCommand]
    private void Guardar()
    {
        UmbralesRiesgo? umbrales = Leer();

        if (umbrales is null)
        {
            Mensaje = "Completá todos los campos con números";
            return;
        }

        try
        {
            DatosSistema.GuardarUmbrales(_comision, umbrales);

            Mensaje = $"Configuración guardada para {_comision}";
        }
        catch (InvalidOperationException ex)
        {
            Mensaje = ex.Message;
        }
    }

    [RelayCommand]
    private void Restablecer()
    {
        DatosSistema.RestablecerUmbrales(_comision);

        Mostrar(DatosSistema.ObtenerUmbrales(_comision));

        Mensaje = "Se volvió a los valores por defecto";
    }
}
