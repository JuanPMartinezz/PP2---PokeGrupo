using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RegistroAsistencia.Models;
using RegistroAsistencia.Services;
using System.Collections.ObjectModel;
using RegistroAsistencia.Services;

namespace RegistroAsistencia.ViewModels;

public partial class ComisionViewModel : ObservableObject
{
    public string Nombre { get; set; } = "";

    public string Apellido { get; set; } = "";

    public string Legajo { get; set; } = "";

    public string Mensaje { get; set; } = "";

    public ObservableCollection<Estudiante> Estudiantes
     => DatosSistema.Estudiantes;

    public int CantidadAlumnos => Estudiantes.Count;

    [RelayCommand]
    private void AgregarAlumno()
    {
        if (Estudiantes.Any(e => e.Legajo == Legajo))
        {
            Mensaje = "Ya existe un alumno con ese legajo";
            OnPropertyChanged(nameof(Mensaje));
            return;
        }

        Estudiantes.Add(new Estudiante
        {
            Nombre = Nombre,
            Apellido = Apellido,
            Legajo = Legajo
        });

        OnPropertyChanged(nameof(CantidadAlumnos));

        var ordenados = Estudiantes
            .OrderBy(e => e.Apellido)
            .ToList();

        Estudiantes.Clear();

        foreach (var estudiante in ordenados)
        {
            Estudiantes.Add(estudiante);
        }

        Nombre = "";
        Apellido = "";
        Legajo = "";

        Mensaje = "Alumno agregado";

        OnPropertyChanged(nameof(Nombre));
        OnPropertyChanged(nameof(Apellido));
        OnPropertyChanged(nameof(Legajo));
        OnPropertyChanged(nameof(Mensaje));
    }
}