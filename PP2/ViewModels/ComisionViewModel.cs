using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RegistroAsistencia.Models;
using RegistroAsistencia.Services;
using System.Collections.ObjectModel;

namespace RegistroAsistencia.ViewModels;

public partial class ComisionViewModel : ObservableObject
{
    public string Nombre { get; set; } = "";
    public string Apellido { get; set; } = "";
    public string Legajo { get; set; } = "";
    public string Mensaje { get; set; } = "";

    public ObservableCollection<string> Comisiones { get; } = new()
    {
        "Comisión A",
        "Comisión B"
    };

    private string comisionSeleccionada = "Comisión A";

    public string ComisionSeleccionada
    {
        get => comisionSeleccionada;

        set
        {
            if (SetProperty(ref comisionSeleccionada, value))
            {
                CargarEstudiantesComision();
            }
        }
    }

    public ObservableCollection<Estudiante> Estudiantes { get; } = new();

    public int CantidadAlumnos => Estudiantes.Count;

    public ComisionViewModel()
    {
        CargarEstudiantesComision();
    }

    private void CargarEstudiantesComision()
    {
        Estudiantes.Clear();

        var estudiantesComision = DatosSistema.Estudiantes
            .Where(e => e.Comision == ComisionSeleccionada)
            .OrderBy(e => e.Apellido)
            .ThenBy(e => e.Nombre)
            .ToList();

        foreach (var estudiante in estudiantesComision)
        {
            Estudiantes.Add(estudiante);
        }

        OnPropertyChanged(nameof(CantidadAlumnos));
    }

    [RelayCommand]
    private void AgregarAlumno()
    {
        Nombre = Nombre.Trim();
        Apellido = Apellido.Trim();
        Legajo = Legajo.Trim();

        if (string.IsNullOrWhiteSpace(Nombre) ||
            string.IsNullOrWhiteSpace(Apellido) ||
            string.IsNullOrWhiteSpace(Legajo))
        {
            Mensaje = "Complete nombre, apellido y legajo";

            OnPropertyChanged(nameof(Mensaje));

            return;
        }

        if (DatosSistema.ExisteEstudiante(Legajo))
        {
            Mensaje = "Ya existe un alumno con ese legajo";

            OnPropertyChanged(nameof(Mensaje));

            return;
        }

        var nuevo = new Estudiante
        {
            Nombre = Nombre,
            Apellido = Apellido,
            Legajo = Legajo,
            Comision = ComisionSeleccionada
        };

        try
        {
            DatosSistema.AgregarEstudiante(nuevo);

            CargarEstudiantesComision();

            Nombre = "";
            Apellido = "";
            Legajo = "";

            Mensaje =
                $"Alumno agregado a {ComisionSeleccionada}";
        }
        catch (Exception ex)
        {
            Mensaje =
                $"No se pudo guardar el alumno: {ex.Message}";
        }

        OnPropertyChanged(nameof(Nombre));
        OnPropertyChanged(nameof(Apellido));
        OnPropertyChanged(nameof(Legajo));
        OnPropertyChanged(nameof(Mensaje));
        OnPropertyChanged(nameof(CantidadAlumnos));
    }

    [RelayCommand]
    private void BorrarTodosLosRegistros()
    {
        try
        {
            DatosSistema.BorrarTodosLosRegistros();

            CargarEstudiantesComision();

            Mensaje =
                "Se eliminaron todos los alumnos, asistencias, trabajos, entregas, calendario y contactos.";
        }
        catch (Exception ex)
        {
            Mensaje =
                $"Error al borrar los registros: {ex.Message}";
        }

        OnPropertyChanged(nameof(CantidadAlumnos));
        OnPropertyChanged(nameof(Mensaje));
    }
}