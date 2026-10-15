using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RegistroAsistencia.Models;
using RegistroAsistencia.Services;
using System.Collections.ObjectModel;

namespace RegistroAsistencia.ViewModels;

public partial class AsistenciaViewModel : ObservableObject
{
    public ObservableCollection<string> Comisiones { get; } = new()
    {
        "Comisión A",
        "Comisión B"
    };

    public ObservableCollection<Estudiante> Estudiantes { get; } = new();

    public ObservableCollection<Asistencia> Asistencias { get; } = new();

    private string comisionSeleccionada = "Comisión A";

    public string ComisionSeleccionada
    {
        get => comisionSeleccionada;

        set
        {
            if (SetProperty(ref comisionSeleccionada, value))
            {
                AlumnoSeleccionado = null;

                CargarEstudiantesComision();
                ConsultarAsistencias();
            }
        }
    }

    private Estudiante? alumnoSeleccionado;

    public Estudiante? AlumnoSeleccionado
    {
        get => alumnoSeleccionado;

        set => SetProperty(
            ref alumnoSeleccionado,
            value);
    }

    private Asistencia? asistenciaSeleccionada;

    public Asistencia? AsistenciaSeleccionada
    {
        get => asistenciaSeleccionada;

        set => SetProperty(
            ref asistenciaSeleccionada,
            value);
    }

    private DateTime fechaClase = DateTime.Today;

    public DateTime FechaClase
    {
        get => fechaClase;

        set
        {
            if (SetProperty(ref fechaClase, value))
            {
                ConsultarAsistencias();
            }
        }
    }

    private string mensaje = "";

    public string Mensaje
    {
        get => mensaje;

        set => SetProperty(
            ref mensaje,
            value);
    }

    public AsistenciaViewModel()
    {
        CargarEstudiantesComision();
        ConsultarAsistencias();
    }

    private void CargarEstudiantesComision()
    {
        Estudiantes.Clear();

        var estudiantes = DatosSistema.Estudiantes
            .Where(e => e.Comision == ComisionSeleccionada)
            .OrderBy(e => e.Apellido)
            .ThenBy(e => e.Nombre)
            .ToList();

        foreach (var estudiante in estudiantes)
        {
            Estudiantes.Add(estudiante);
        }
    }

    [RelayCommand]
    private void ConsultarAsistencias()
    {
        Asistencias.Clear();

        var asistencias = DatosSistema.Asistencias
            .Where(a =>
                a.Clase.Fecha.Date == FechaClase.Date &&
                a.Estudiante.Comision == ComisionSeleccionada)
            .OrderBy(a => a.Estudiante.Apellido)
            .ThenBy(a => a.Estudiante.Nombre)
            .ToList();

        foreach (var asistencia in asistencias)
        {
            Asistencias.Add(asistencia);
        }
    }

    [RelayCommand]
    private void RegistrarPresente()
    {
        RegistrarAsistencia(
            EstadoAsistencia.Presente);
    }

    [RelayCommand]
    private void RegistrarAusente()
    {
        RegistrarAsistencia(
            EstadoAsistencia.Ausente);
    }

    private void RegistrarAsistencia(
        EstadoAsistencia estado)
    {
        if (AlumnoSeleccionado is null)
        {
            Mensaje = "Seleccione un alumno";
            return;
        }

        if (DatosSistema.ExisteAsistencia(
                AlumnoSeleccionado.Legajo,
                FechaClase))
        {
            Mensaje =
                "La asistencia ya fue registrada para ese alumno y fecha";

            return;
        }

        var asistencia = new Asistencia
        {
            Estudiante = AlumnoSeleccionado,

            Clase = new Clase
            {
                Fecha = FechaClase.Date
            },

            Estado = estado
        };

        try
        {
            DatosSistema.AgregarAsistencia(asistencia);

            Mensaje =
                estado == EstadoAsistencia.Presente
                    ? "Presente registrado"
                    : "Ausencia registrada";

            ConsultarAsistencias();
        }
        catch (Exception ex)
        {
            Mensaje =
                $"No se pudo guardar la asistencia: {ex.Message}";
        }
    }

    [RelayCommand]
    private void CorregirAsistencia()
    {
        if (AsistenciaSeleccionada is null)
        {
            Mensaje =
                "Seleccione una asistencia del historial";

            return;
        }

        EstadoAsistencia nuevoEstado =
            AsistenciaSeleccionada.Estado ==
            EstadoAsistencia.Presente
                ? EstadoAsistencia.Ausente
                : EstadoAsistencia.Presente;

        try
        {
            AsistenciaSeleccionada =
                DatosSistema.ActualizarEstadoAsistencia(
                    AsistenciaSeleccionada,
                    nuevoEstado);

            Mensaje =
                $"Asistencia corregida a {nuevoEstado}";

            ConsultarAsistencias();
        }
        catch (Exception ex)
        {
            Mensaje =
                $"No se pudo corregir la asistencia: {ex.Message}";
        }
    }
}