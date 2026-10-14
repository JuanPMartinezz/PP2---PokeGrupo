using CommunityToolkit.Mvvm.ComponentModel;
using RegistroAsistencia.Models;
using RegistroAsistencia.Services;
using System.Collections.ObjectModel;

namespace RegistroAsistencia.ViewModels;

public class FichaEstudianteViewModel : ObservableObject
{
    private readonly RiesgoService _riesgoService;

    public ObservableCollection<Estudiante> Estudiantes
        => DatosSistema.Estudiantes;

    public ObservableCollection<Asistencia> Historial { get; }
        = new();

    private Estudiante? estudianteSeleccionado;

    public Estudiante? EstudianteSeleccionado
    {
        get => estudianteSeleccionado;

        set
        {
            if (SetProperty(
                ref estudianteSeleccionado,
                value))
            {
                CargarFicha();
            }
        }
    }

    private int presentes;

    public int Presentes
    {
        get => presentes;

        private set => SetProperty(
            ref presentes,
            value);
    }

    private int ausencias;

    public int Ausencias
    {
        get => ausencias;

        private set => SetProperty(
            ref ausencias,
            value);
    }

    private int totalClases;

    public int TotalClases
    {
        get => totalClases;

        private set => SetProperty(
            ref totalClases,
            value);
    }

    private decimal porcentajeAusencias;

    public decimal PorcentajeAusencias
    {
        get => porcentajeAusencias;

        private set => SetProperty(
            ref porcentajeAusencias,
            value);
    }

    private string mensaje = "Seleccione un estudiante.";

    public string Mensaje
    {
        get => mensaje;

        private set => SetProperty(
            ref mensaje,
            value);
    }

    public FichaEstudianteViewModel()
    {
        _riesgoService = new RiesgoService();
    }

    public void CargarFicha()
    {
        Historial.Clear();

        if (EstudianteSeleccionado is null)
        {
            Presentes = 0;
            Ausencias = 0;
            TotalClases = 0;
            PorcentajeAusencias = 0;

            Mensaje = "Seleccione un estudiante.";

            return;
        }

        List<Asistencia> asistenciasEstudiante =
            DatosSistema.Asistencias
                .Where(a =>
                    a.Estudiante.Legajo ==
                    EstudianteSeleccionado.Legajo)
                .OrderBy(a => a.Clase.Fecha)
                .ToList();

        foreach (Asistencia asistencia
                 in asistenciasEstudiante)
        {
            Historial.Add(asistencia);
        }

        Presentes = asistenciasEstudiante.Count(a =>
            a.Estado == EstadoAsistencia.Presente);

        Ausencias = asistenciasEstudiante.Count(a =>
            a.Estado == EstadoAsistencia.Ausente);

        TotalClases = asistenciasEstudiante.Count;

        PorcentajeAusencias =
            _riesgoService.CalcularPorcentajeAusencias(
                EstudianteSeleccionado);

        Mensaje = TotalClases == 0
            ? "El estudiante todavía no tiene asistencias registradas."
            : "Ficha actualizada correctamente.";
    }
}