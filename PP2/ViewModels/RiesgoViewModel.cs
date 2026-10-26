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

    // Filtro por contacto (H11)
    public const string FiltroTodos = "Todos los alumnos";
    public const string FiltroSoloContactados = "Solo contactados";
    public const string FiltroSoloSinContactar = "Solo sin contactar";

    public ObservableCollection<string> FiltrosContacto { get; } = new()
    {
        FiltroTodos,
        FiltroSoloContactados,
        FiltroSoloSinContactar
    };

    private string filtroContactoSeleccionado = FiltroTodos;

    public string FiltroContactoSeleccionado
    {
        get => filtroContactoSeleccionado;

        set
        {
            if (SetProperty(ref filtroContactoSeleccionado, value))
            {
                CargarRiesgos();
            }
        }
    }

    private string resumenMostrados = "";

    public string ResumenMostrados
    {
        get => resumenMostrados;
        private set => SetProperty(ref resumenMostrados, value);
    }

    private string resumenContactos = "";

    public string ResumenContactos
    {
        get => resumenContactos;
        private set => SetProperty(ref resumenContactos, value);
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

            // Contactos del estudiante (H11)
            var contactosAlumno = DatosSistema.Contactos
                .Where(c =>
                    c.Estudiante.Legajo == estudiante.Legajo)
                .OrderByDescending(c => c.Fecha)
                .ToList();

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
                    porcentajeRiesgo,

                NivelRiesgo =
                    _riesgoService.ObtenerNivel(
                        porcentajeRiesgo,
                        totalClases + totalEntregas),

                Contactos =
                    contactosAlumno
            });
        }

        // Mayor riesgo primero
        var ordenados = Resultados
            .OrderByDescending(r =>
                r.PorcentajeRiesgo)
            .ToList();

        // El resumen cuenta a toda la comisión, sin importar el filtro
        int contactados =
            ordenados.Count(r => r.Contactado);

        ResumenContactos =
            $"Contactados: {contactados} de {ordenados.Count}";

        IEnumerable<ResultadoRiesgo> visibles = ordenados;

        if (FiltroContactoSeleccionado == FiltroSoloContactados)
        {
            visibles = ordenados.Where(r => r.Contactado);
        }
        else if (FiltroContactoSeleccionado == FiltroSoloSinContactar)
        {
            visibles = ordenados.Where(r => !r.Contactado);
        }

        Resultados.Clear();

        foreach (var item in visibles)
        {
            Resultados.Add(item);
        }

        ResumenMostrados =
            $"Mostrando {Resultados.Count} de {ordenados.Count} estudiantes";
    }
}