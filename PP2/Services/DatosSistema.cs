using RegistroAsistencia.Models;
using System.Collections.ObjectModel;

namespace RegistroAsistencia.Services;

public static class DatosSistema
{
    public static ObservableCollection<Estudiante> Estudiantes { get; }
        = new();

    public static ObservableCollection<Asistencia> Asistencias { get; }
        = new();
}