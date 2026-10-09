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

    public Estudiante AlumnoSeleccionado { get; set; }

    public DateTime FechaClase { get; set; }
        = DateTime.Today;

    public string Mensaje { get; set; } = "";

    [RelayCommand]
    private void RegistrarPresente()
    {
        if (AlumnoSeleccionado == null)
        {
            Mensaje = "Seleccione un alumno";
            OnPropertyChanged(nameof(Mensaje));
            return;
        }

        bool yaExiste = Asistencias.Any(a =>
            a.Estudiante == AlumnoSeleccionado &&
            a.Clase.Fecha.Date == FechaClase.Date);

        if (yaExiste)
        {
            Mensaje = "La asistencia ya fue registrada";
            OnPropertyChanged(nameof(Mensaje));
            return;
        }

        Asistencias.Add(new Asistencia
        {
            Estudiante = AlumnoSeleccionado,
            Clase = new Clase
            {
                Fecha = FechaClase
            },
            Estado = EstadoAsistencia.Presente
        });

        Mensaje = "Presente registrado";
        OnPropertyChanged(nameof(Mensaje));
    }

    [RelayCommand]
    private void RegistrarAusente()
    {
        if (AlumnoSeleccionado == null)
        {
            Mensaje = "Seleccione un alumno";
            OnPropertyChanged(nameof(Mensaje));
            return;
        }

        bool yaExiste = Asistencias.Any(a =>
            a.Estudiante == AlumnoSeleccionado &&
            a.Clase.Fecha.Date == FechaClase.Date);

        if (yaExiste)
        {
            Mensaje = "La asistencia ya fue registrada";
            OnPropertyChanged(nameof(Mensaje));
            return;
        }

        Asistencias.Add(new Asistencia
        {
            Estudiante = AlumnoSeleccionado,
            Clase = new Clase
            {
                Fecha = FechaClase
            },
            Estado = EstadoAsistencia.Ausente
        });

        Mensaje = "Ausencia registrada";
        OnPropertyChanged(nameof(Mensaje));
    }
}