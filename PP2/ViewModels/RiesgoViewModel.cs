using CommunityToolkit.Mvvm.ComponentModel;
using RegistroAsistencia.Models;
using RegistroAsistencia.Services;
using System.Collections.ObjectModel;

namespace RegistroAsistencia.ViewModels;

public class RiesgoViewModel : ObservableObject
{
    private readonly RiesgoService _riesgoService;

    public ObservableCollection<string> Comisiones { get; } = new()
    {
        "Comisión A",
        "Comisión B"
    };

    public ObservableCollection<ResultadoRiesgo> Resultados { get; }
        = new();

    private string comisionSeleccionada = "Comisión A";

    public string ComisionSeleccionada
    {
        get => comisionSeleccionada;

        set
        {
            if (SetProperty(ref comisionSeleccionada, value))
            {
                CargarRiesgos();
            }
        }
    }

    public RiesgoViewModel()
    {
        _riesgoService = new RiesgoService();

        CargarRiesgos();
    }

    public void CargarRiesgos()
    {
        Resultados.Clear();

        var estudiantesComision = DatosSistema.Estudiantes
            .Where(e => e.Comision == ComisionSeleccionada)
            .ToList();

        foreach (Estudiante estudiante in estudiantesComision)
        {
            // Asistencias del estudiante
            var asistenciasAlumno = DatosSistema.Asistencias
                .Where(a =>
                    a.Estudiante.Legajo == estudiante.Legajo)
                .ToList();

            int totalClases =
                asistenciasAlumno.Count;

            int ausencias =
                asistenciasAlumno.Count(a =>
                    a.Estado == EstadoAsistencia.Ausente);

            int presentes =
                asistenciasAlumno.Count(a =>
                    a.Estado == EstadoAsistencia.Presente);

            // Entregas del estudiante
            var entregasAlumno = DatosSistema.Entregas
                .Where(e =>
                    e.Estudiante.Legajo == estudiante.Legajo)
                .ToList();

            int totalEntregas =
                entregasAlumno.Count;

            int entregados =
                entregasAlumno.Count(e =>
                    e.Estado == EstadoEntrega.Entregado);

            int noEntregados =
                entregasAlumno.Count(e =>
                    e.Estado == EstadoEntrega.NoEntregado);

            int entregadosFueraDeTermino =
                entregasAlumno.Count(e =>
                    e.Estado ==
                    EstadoEntrega.EntregadoFueraDeTermino);

            // Riesgo combinado de H6
            decimal porcentajeRiesgo =
                _riesgoService.CalcularRiesgo(estudiante);

            Resultados.Add(new ResultadoRiesgo
            {
                Estudiante = estudiante,

                TotalClases = totalClases,
                Ausencias = ausencias,
                Presentes = presentes,

                TotalEntregas = totalEntregas,
                Entregados = entregados,
                NoEntregados = noEntregados,
                EntregadosFueraDeTermino =
                    entregadosFueraDeTermino,

                PorcentajeRiesgo =
                    porcentajeRiesgo
            });
        }

        // Mayor riesgo primero
        var ordenados = Resultados
            .OrderByDescending(r =>
                r.PorcentajeRiesgo)
            .ToList();

        Resultados.Clear();

        foreach (var item in ordenados)
        {
            Resultados.Add(item);
        }
    }
}