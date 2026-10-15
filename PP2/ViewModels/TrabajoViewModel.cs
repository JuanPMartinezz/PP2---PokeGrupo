using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RegistroAsistencia.Models;
using RegistroAsistencia.Services;
using System.Collections.ObjectModel;

namespace RegistroAsistencia.ViewModels;

public partial class TrabajoViewModel : ObservableObject
{
    public ObservableCollection<string> Comisiones { get; } = new()
    {
        "Comisión A",
        "Comisión B"
    };

    public ObservableCollection<EstadoEntrega> EstadosEntrega { get; }
        = new(
            Enum.GetValues<EstadoEntrega>());

    public ObservableCollection<Estudiante> Estudiantes { get; }
        = new();

    public ObservableCollection<Trabajo> Trabajos
        => DatosSistema.Trabajos;

    public ObservableCollection<Entrega> Entregas { get; }
        = new();

    private string comisionSeleccionada = "Comisión A";

    public string ComisionSeleccionada
    {
        get => comisionSeleccionada;

        set
        {
            if (SetProperty(
                ref comisionSeleccionada,
                value))
            {
                EstudianteSeleccionado = null;
                EntregaSeleccionada = null;

                CargarEstudiantesComision();
                ConsultarEntregas();
            }
        }
    }

    private string nombreTrabajo = "";

    public string NombreTrabajo
    {
        get => nombreTrabajo;

        set => SetProperty(
            ref nombreTrabajo,
            value);
    }

    private Trabajo? trabajoSeleccionado;

    public Trabajo? TrabajoSeleccionado
    {
        get => trabajoSeleccionado;

        set
        {
            if (SetProperty(
                ref trabajoSeleccionado,
                value))
            {
                EntregaSeleccionada = null;
                ConsultarEntregas();
            }
        }
    }

    private Estudiante? estudianteSeleccionado;

    public Estudiante? EstudianteSeleccionado
    {
        get => estudianteSeleccionado;

        set => SetProperty(
            ref estudianteSeleccionado,
            value);
    }

    private EstadoEntrega estadoSeleccionado
        = EstadoEntrega.Entregado;

    public EstadoEntrega EstadoSeleccionado
    {
        get => estadoSeleccionado;

        set => SetProperty(
            ref estadoSeleccionado,
            value);
    }

    private Entrega? entregaSeleccionada;

    public Entrega? EntregaSeleccionada
    {
        get => entregaSeleccionada;

        set
        {
            if (SetProperty(
                ref entregaSeleccionada,
                value) &&
                value is not null)
            {
                EstadoSeleccionado = value.Estado;
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

    public TrabajoViewModel()
    {
        CargarEstudiantesComision();
        ConsultarEntregas();
    }

    private void CargarEstudiantesComision()
    {
        Estudiantes.Clear();

        var estudiantesComision =
            DatosSistema.Estudiantes
                .Where(e =>
                    e.Comision == ComisionSeleccionada)
                .OrderBy(e => e.Apellido)
                .ThenBy(e => e.Nombre)
                .ToList();

        foreach (Estudiante estudiante
                 in estudiantesComision)
        {
            Estudiantes.Add(estudiante);
        }
    }

    [RelayCommand]
    private void AgregarTrabajo()
    {
        string nombreLimpio =
            NombreTrabajo.Trim();

        if (string.IsNullOrWhiteSpace(nombreLimpio))
        {
            Mensaje =
                "Ingrese el nombre del trabajo";

            return;
        }

        bool trabajoDuplicado =
            DatosSistema.Trabajos.Any(t =>
                t.Nombre.Equals(
                    nombreLimpio,
                    StringComparison.OrdinalIgnoreCase));

        if (trabajoDuplicado)
        {
            Mensaje =
                "Ya existe un trabajo con ese nombre";

            return;
        }

        var nuevoTrabajo = new Trabajo
        {
            Nombre = nombreLimpio
        };

        try
        {
            DatosSistema.AgregarTrabajo(
                nuevoTrabajo);

            NombreTrabajo = "";

            TrabajoSeleccionado =
                nuevoTrabajo;

            Mensaje =
                "Trabajo agregado correctamente";
        }
        catch (Exception ex)
        {
            Mensaje =
                $"No se pudo guardar el trabajo: {ex.Message}";
        }
    }

    [RelayCommand]
    private void RegistrarEntrega()
    {
        if (TrabajoSeleccionado is null)
        {
            Mensaje =
                "Seleccione un trabajo";

            return;
        }

        if (EstudianteSeleccionado is null)
        {
            Mensaje =
                "Seleccione un estudiante";

            return;
        }

        if (DatosSistema.ExisteEntrega(
                EstudianteSeleccionado.Legajo,
                TrabajoSeleccionado.Id))
        {
            Mensaje =
                "La entrega ya fue registrada para ese estudiante y trabajo";

            return;
        }

        var nuevaEntrega = new Entrega
        {
            Estudiante =
                EstudianteSeleccionado,

            Trabajo =
                TrabajoSeleccionado,

            Estado =
                EstadoSeleccionado
        };

        try
        {
            DatosSistema.AgregarEntrega(
                nuevaEntrega);

            Mensaje =
                "Entrega registrada correctamente";

            ConsultarEntregas();
        }
        catch (Exception ex)
        {
            Mensaje =
                $"No se pudo guardar la entrega: {ex.Message}";
        }
    }

    [RelayCommand]
    private void ConsultarEntregas()
    {
        Entregas.Clear();

        if (TrabajoSeleccionado is null)
        {
            return;
        }

        var entregasFiltradas =
            DatosSistema.Entregas
                .Where(e =>
                    e.Trabajo.Id ==
                    TrabajoSeleccionado.Id &&
                    e.Estudiante.Comision ==
                    ComisionSeleccionada)
                .OrderBy(e =>
                    e.Estudiante.Apellido)
                .ThenBy(e =>
                    e.Estudiante.Nombre)
                .ToList();

        foreach (Entrega entrega
                 in entregasFiltradas)
        {
            Entregas.Add(entrega);
        }
    }

    [RelayCommand]
    private void ActualizarEntrega()
    {
        if (EntregaSeleccionada is null)
        {
            Mensaje =
                "Seleccione una entrega registrada";

            return;
        }

        try
        {
            Entrega entregaActualizada =
                DatosSistema.ActualizarEstadoEntrega(
                    EntregaSeleccionada,
                    EstadoSeleccionado);

            Mensaje =
                $"Entrega actualizada a {EstadoSeleccionado}";

            ConsultarEntregas();

            EntregaSeleccionada =
                Entregas.FirstOrDefault(e =>
                    e.Estudiante.Legajo ==
                    entregaActualizada.Estudiante.Legajo &&
                    e.Trabajo.Id ==
                    entregaActualizada.Trabajo.Id);
        }
        catch (Exception ex)
        {
            Mensaje =
                $"No se pudo actualizar la entrega: {ex.Message}";
        }
    }
}