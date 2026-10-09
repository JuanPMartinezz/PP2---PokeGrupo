using CommunityToolkit.Mvvm.ComponentModel;
using RegistroAsistencia.Models;
using RegistroAsistencia.Services;
using System.Collections.ObjectModel;

namespace RegistroAsistencia.ViewModels;

public class RiesgoViewModel : ObservableObject
{
    private readonly RiesgoService _riesgoService;

    public ObservableCollection<ResultadoRiesgo> Resultados { get; }
        = new();

    public RiesgoViewModel()
    {
        _riesgoService = new RiesgoService();

        CargarRiesgos();
    }

    public void CargarRiesgos()
    {
        Resultados.Clear();

        foreach (Estudiante estudiante in DatosSistema.Estudiantes)
        {
            var asistenciasAlumno =
     DatosSistema.Asistencias
         .Where(a => a.Estudiante == estudiante)
         .ToList();

            int totalClases = asistenciasAlumno.Count;

            int ausencias = asistenciasAlumno.Count(a =>
                a.Estado == EstadoAsistencia.Ausente);

            int presentes = asistenciasAlumno.Count(a =>
                a.Estado == EstadoAsistencia.Presente);

            decimal porcentaje =
                _riesgoService.CalcularPorcentajeAusencias(estudiante);

            Resultados.Add(new ResultadoRiesgo
            {
                Estudiante = estudiante,
                TotalClases = totalClases,
                Ausencias = ausencias,
                Presentes = presentes,
                PorcentajeAusencias = porcentaje
            });
        }

        var ordenados = Resultados
            .OrderByDescending(r => r.PorcentajeAusencias)
            .ToList();

        Resultados.Clear();

        foreach (var item in ordenados)
        {
            Resultados.Add(item);
        }
    }
}