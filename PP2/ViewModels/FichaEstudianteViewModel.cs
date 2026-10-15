using CommunityToolkit.Mvvm.ComponentModel;
using RegistroAsistencia.Models;
using RegistroAsistencia.Services;
using System.Collections.ObjectModel;

namespace RegistroAsistencia.ViewModels;

public class FichaEstudianteViewModel : ObservableObject
{
    private readonly RiesgoService _riesgoService;

    public ObservableCollection<string> Comisiones { get; } = new()
    {
        "Comisión A",
        "Comisión B"
    };

    public ObservableCollection<Estudiante> Estudiantes { get; } = new();

    public ObservableCollection<Asistencia> Historial { get; } = new();

    public ObservableCollection<Entrega> HistorialEntregas { get; } = new();

    private string comisionSeleccionada = "Comisión A";

    public string ComisionSeleccionada
    {
        get => comisionSeleccionada;
        set
        {
            if (SetProperty(ref comisionSeleccionada, value))
            {
                EstudianteSeleccionado = null;
                CargarEstudiantesComision();
            }
        }
    }

    private Estudiante? estudianteSeleccionado;

    public Estudiante? EstudianteSeleccionado
    {
        get => estudianteSeleccionado;
        set
        {
            if (SetProperty(ref estudianteSeleccionado, value))
            {
                CargarFicha();
            }
        }
    }

    private int presentes;

    public int Presentes
    {
        get => presentes;
        private set => SetProperty(ref presentes, value);
    }

    private int ausencias;

    public int Ausencias
    {
        get => ausencias;
        private set => SetProperty(ref ausencias, value);
    }

    private int totalClases;

    public int TotalClases
    {
        get => totalClases;
        private set => SetProperty(ref totalClases, value);
    }

    private int entregados;

    public int Entregados
    {
        get => entregados;
        private set => SetProperty(ref entregados, value);
    }

    private int noEntregados;

    public int NoEntregados
    {
        get => noEntregados;
        private set => SetProperty(ref noEntregados, value);
    }

    private int entregadosFueraDeTermino;

    public int EntregadosFueraDeTermino
    {
        get => entregadosFueraDeTermino;
        private set => SetProperty(ref entregadosFueraDeTermino, value);
    }

    private int totalEntregas;

    public int TotalEntregas
    {
        get => totalEntregas;
        private set => SetProperty(ref totalEntregas, value);
    }

    private decimal porcentajeRiesgo;

    public decimal PorcentajeRiesgo
    {
        get => porcentajeRiesgo;
        private set => SetProperty(ref porcentajeRiesgo, value);
    }

    private string mensaje = "Seleccione un estudiante.";

    public string Mensaje
    {
        get => mensaje;
        private set => SetProperty(ref mensaje, value);
    }

    public FichaEstudianteViewModel()
    {
        _riesgoService = new RiesgoService();
        CargarEstudiantesComision();
    }

    private void CargarEstudiantesComision()
    {
        Estudiantes.Clear();

        List<Estudiante> estudiantesComision =
            DatosSistema.Estudiantes
                .Where(e => e.Comision == ComisionSeleccionada)
                .OrderBy(e => e.Apellido)
                .ThenBy(e => e.Nombre)
                .ToList();

        foreach (Estudiante estudiante in estudiantesComision)
        {
            Estudiantes.Add(estudiante);
        }
    }

    public void CargarFicha()
    {
        Historial.Clear();
        HistorialEntregas.Clear();

        if (EstudianteSeleccionado is null)
        {
            LimpiarFicha();
            Mensaje = "Seleccione un estudiante.";
            return;
        }

        CargarAsistencias();
        CargarEntregas();

        PorcentajeRiesgo =
            _riesgoService.CalcularRiesgo(EstudianteSeleccionado);

        Mensaje = TotalClases == 0 && TotalEntregas == 0
            ? "El estudiante todavía no tiene registros."
            : "Ficha actualizada correctamente.";
    }

    private void CargarAsistencias()
    {
        if (EstudianteSeleccionado is null)
        {
            return;
        }

        List<Asistencia> asistenciasEstudiante =
            DatosSistema.Asistencias
                .Where(a =>
                    a.Estudiante.Legajo ==
                    EstudianteSeleccionado.Legajo)
                .OrderBy(a => a.Clase.Fecha)
                .ToList();

        foreach (Asistencia asistencia in asistenciasEstudiante)
        {
            Historial.Add(asistencia);
        }

        Presentes = asistenciasEstudiante.Count(a =>
            a.Estado == EstadoAsistencia.Presente);

        Ausencias = asistenciasEstudiante.Count(a =>
            a.Estado == EstadoAsistencia.Ausente);

        TotalClases = asistenciasEstudiante.Count;
    }

    private void CargarEntregas()
    {
        if (EstudianteSeleccionado is null)
        {
            return;
        }

        List<Entrega> entregasEstudiante =
            DatosSistema.Entregas
                .Where(e =>
                    e.Estudiante.Legajo ==
                    EstudianteSeleccionado.Legajo)
                .OrderBy(e => e.Trabajo.Nombre)
                .ToList();

        foreach (Entrega entrega in entregasEstudiante)
        {
            HistorialEntregas.Add(entrega);
        }

        Entregados = entregasEstudiante.Count(e =>
            e.Estado == EstadoEntrega.Entregado);

        NoEntregados = entregasEstudiante.Count(e =>
            e.Estado == EstadoEntrega.NoEntregado);

        EntregadosFueraDeTermino = entregasEstudiante.Count(e =>
            e.Estado == EstadoEntrega.EntregadoFueraDeTermino);

        TotalEntregas = entregasEstudiante.Count;
    }

    private void LimpiarFicha()
    {
        Presentes = 0;
        Ausencias = 0;
        TotalClases = 0;

        Entregados = 0;
        NoEntregados = 0;
        EntregadosFueraDeTermino = 0;
        TotalEntregas = 0;

        PorcentajeRiesgo = 0;
    }
}