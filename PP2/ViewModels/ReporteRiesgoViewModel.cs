using CommunityToolkit.Mvvm.ComponentModel;
using RegistroAsistencia.Models;
using RegistroAsistencia.Services;

namespace RegistroAsistencia.ViewModels;

/// <summary>
/// Reporte de riesgo de una comisión (H12). No calcula nada: muestra el
/// resumen que arma RiesgoService con los mismos resultados del panel.
/// </summary>
public class ReporteRiesgoViewModel : ObservableObject, IActualizable
{
    private readonly RiesgoService _riesgo;

    private readonly string _comision;

    private ResumenComision _datos = new(0, 0, 0, 0, 0);

    public string Comision => _comision;

    public int TotalEstudiantes => _datos.Total;

    public int EstudiantesEnRiesgo => _datos.Alto;

    public int EstudiantesRiesgoMedio => _datos.Medio;

    public int EstudiantesRiesgoBajo => _datos.Bajo;

    public decimal PorcentajeEnRiesgo => _datos.PorcentajeEnRiesgo;

    public bool SinEstudiantes => _datos.SinEstudiantes;

    public bool SinDatos => _datos.SinDatosSuficientes;

    public string Resumen => TextoResumen();

    public ReporteRiesgoViewModel(string comision, RiesgoService riesgo)
    {
        _comision = comision;
        _riesgo = riesgo;

        Actualizar();
    }

    public void Actualizar()
    {
        _datos = _riesgo.ObtenerResumen(_comision);

        // Una cadena vacía avisa que cambiaron todas las propiedades
        OnPropertyChanged(string.Empty);
    }

    private string TextoResumen()
    {
        if (_datos.SinEstudiantes)
        {
            return "Esta comisión todavía no tiene estudiantes.";
        }

        return _datos.SinDatosSuficientes
            ? "Todavía no hay datos suficientes para calcular el riesgo."
            : $"{_datos.Alto} de {_datos.Total} estudiantes tienen riesgo alto.";
    }
}
