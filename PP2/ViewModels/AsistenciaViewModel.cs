using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RegistroAsistencia.Models;
using RegistroAsistencia.Services;
using System.Collections.ObjectModel;

namespace RegistroAsistencia.ViewModels;

public partial class AsistenciaViewModel : ObservableObject
{
    public ObservableCollection<Asistencia> Asistencias => DatosSistema.Asistencias;
    public ObservableCollection<Estudiante> Estudiantes => DatosSistema.Estudiantes;

    public Estudiante? AlumnoSeleccionado { get; set; }
    public DateTime FechaClase { get; set; } = DateTime.Today;
    public string Mensaje { get; set; } = "";

    [RelayCommand]
    private void RegistrarPresente() => RegistrarAsistencia(EstadoAsistencia.Presente);

    [RelayCommand]
    private void RegistrarAusente() => RegistrarAsistencia(EstadoAsistencia.Ausente);

    private void RegistrarAsistencia(EstadoAsistencia estado)
    {
        if (AlumnoSeleccionado is null)
        {
            Mensaje = "Seleccione un alumno";
            OnPropertyChanged(nameof(Mensaje));
            return;
        }

        if (DatosSistema.ExisteAsistencia(AlumnoSeleccionado.Legajo, FechaClase))
        {
            Mensaje = "La asistencia ya fue registrada para ese alumno y fecha";
            OnPropertyChanged(nameof(Mensaje));
            return;
        }

        var asistencia = new Asistencia
        {
            Estudiante = AlumnoSeleccionado,
            Clase = new Clase { Fecha = FechaClase.Date },
            Estado = estado
        };

        try
        {
            DatosSistema.AgregarAsistencia(asistencia);
            Mensaje = estado == EstadoAsistencia.Presente
                ? "Presente registrado"
                : "Ausencia registrada";
        }
        catch (Exception ex)
        {
            Mensaje = $"No se pudo guardar la asistencia: {ex.Message}";
        }

        OnPropertyChanged(nameof(Mensaje));
    }
}
