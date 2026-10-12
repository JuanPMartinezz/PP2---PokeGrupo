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

    public ObservableCollection<Estudiante> Estudiantes => DatosSistema.Estudiantes;
    public int CantidadAlumnos => Estudiantes.Count;

    [RelayCommand]
    private void AgregarAlumno()
    {
        Nombre = Nombre.Trim();
        Apellido = Apellido.Trim();
        Legajo = Legajo.Trim();

        if (string.IsNullOrWhiteSpace(Nombre) || string.IsNullOrWhiteSpace(Apellido) || string.IsNullOrWhiteSpace(Legajo))
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

        var nuevo = new Estudiante { Nombre = Nombre, Apellido = Apellido, Legajo = Legajo };
        try
        {
            DatosSistema.AgregarEstudiante(nuevo);
            var ordenados = Estudiantes.OrderBy(e => e.Apellido).ThenBy(e => e.Nombre).ToList();
            Estudiantes.Clear();
            foreach (var estudiante in ordenados) Estudiantes.Add(estudiante);

            Nombre = "";
            Apellido = "";
            Legajo = "";
            Mensaje = "Alumno agregado";
        }
        catch (Exception ex)
        {
            Mensaje = $"No se pudo guardar el alumno: {ex.Message}";
        }

        OnPropertyChanged(nameof(CantidadAlumnos));
        OnPropertyChanged(nameof(Nombre));
        OnPropertyChanged(nameof(Apellido));
        OnPropertyChanged(nameof(Legajo));
        OnPropertyChanged(nameof(Mensaje));
    }


    [RelayCommand]
    private void BorrarTodosLosRegistros()
    {
        try
        {
            DatosSistema.BorrarTodosLosRegistros();
            Mensaje = "Se eliminaron todos los alumnos y asistencias.";
        }
        catch (Exception ex)
        {
            Mensaje = $"Error al borrar los registros: {ex.Message}";
        }

        OnPropertyChanged(nameof(CantidadAlumnos));
        OnPropertyChanged(nameof(Mensaje));
    }

}
