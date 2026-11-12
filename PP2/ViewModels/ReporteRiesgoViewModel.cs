

using CommunityToolkit.Mvvm.ComponentModel;
using RegistroAsistencia.Models;
using RegistroAsistencia.Services;

namespace RegistroAsistencia.ViewModels;

/// <summary>
/// Reporte general de riesgo de una comisión (H12).
/// Reutiliza los resultados calculados por RiesgoService.
/// </summary>
public class ReporteRiesgoViewModel : ObservableObject, IActualizable
{
    private readonly RiesgoService _riesgo;
    private readonly string _comision;

    public string Comision => _comision;

    private int totalEstudiantes;
    public int TotalEstudiantes
    {
        get => totalEstudiantes;
        set => SetProperty(ref totalEstudiantes, value);
    }

    private int estudiantesEnRiesgo;
    public int EstudiantesEnRiesgo
    {
        get => estudiantesEnRiesgo;
        set => SetProperty(ref estudiantesEnRiesgo, value);
    }

    private decimal porcentajeEnRiesgo;
    public decimal PorcentajeEnRiesgo
    {
        get => porcentajeEnRiesgo;
        set => SetProperty(ref porcentajeEnRiesgo, value);
    }

    private bool sinEstudiantes;
    public bool SinEstudiantes
    {
        get => sinEstudiantes;
        set => SetProperty(ref sinEstudiantes, value);
    }

    private bool sinDatos;
    public bool SinDatos
    {
        get => sinDatos;
        set => SetProperty(ref sinDatos, value);
    }

    private string resumen = "";
    public string Resumen
    {
        get => resumen;
        set => SetProperty(ref resumen, value);
    }

    public ReporteRiesgoViewModel(
        string comision,
        RiesgoService riesgo)
    {
        _comision = comision;
        _riesgo = riesgo;

        Actualizar();
    }

    public void Actualizar()
    {
        List<ResultadoRiesgo> resultados =
            _riesgo.ObtenerResultados(_comision);

        TotalEstudiantes = resultados.Count;
        SinEstudiantes = TotalEstudiantes == 0;

        SinDatos = !SinEstudiantes &&
            resultados.All(r =>
                r.NivelRiesgo == NivelesRiesgo.SinDatos);

        EstudiantesEnRiesgo = resultados.Count(r =>
            r.NivelRiesgo == NivelesRiesgo.Alto);

        PorcentajeEnRiesgo = TotalEstudiantes == 0 || SinDatos
            ? 0
            : Math.Round(
                (decimal)EstudiantesEnRiesgo / TotalEstudiantes * 100,
                1);

        Resumen = SinEstudiantes
            ? "Esta comisión todavía no tiene estudiantes."
            : SinDatos
                ? "Todavía no hay datos suficientes para calcular el riesgo."
                : $"{EstudiantesEnRiesgo} de {TotalEstudiantes} estudiantes tienen riesgo alto.";
    }
}

