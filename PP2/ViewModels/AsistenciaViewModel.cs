using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RegistroAsistencia.Models;
using RegistroAsistencia.Services;
using System.Collections.ObjectModel;

namespace RegistroAsistencia.ViewModels;

public partial class AsistenciaViewModel : ObservableObject
{
    public ObservableCollection<Asistencia> Asistencias
        => DatosSistema.Asistencias;

    public ObservableCollection<Estudiante> Estudiantes
        => DatosSistema.Estudiantes;

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

        set => SetProperty(
            ref fechaClase,
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
            DatosSistema.AgregarAsistencia(
                asistencia);

            Mensaje = estado ==
                      EstadoAsistencia.Presente
                ? "Presente registrado"
                : "Ausencia registrada";
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
        }
        catch (Exception ex)
        {
            Mensaje =
                $"No se pudo corregir la asistencia: {ex.Message}";
        }
    }
}