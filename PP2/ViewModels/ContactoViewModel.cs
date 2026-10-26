using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RegistroAsistencia.Models;
using RegistroAsistencia.Services;
using System.Collections.ObjectModel;

namespace RegistroAsistencia.ViewModels;

public partial class ContactoViewModel : ObservableObject
{
    public ObservableCollection<string> Comisiones { get; } = new()
    {
        "Comisión A",
        "Comisión B"
    };

    public ObservableCollection<Estudiante> Estudiantes { get; }
        = new();

    public ObservableCollection<ContactoEstudiante> Contactos { get; }
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

                CargarEstudiantesComision();
                ConsultarContactos();
            }
        }
    }

    private Estudiante? estudianteSeleccionado;

    public Estudiante? EstudianteSeleccionado
    {
        get => estudianteSeleccionado;

        set
        {
            if (SetProperty(
                ref estudianteSeleccionado,
                value))
            {
                ConsultarContactos();
            }
        }
    }

    private DateTime fechaContacto = DateTime.Today;

    public DateTime FechaContacto
    {
        get => fechaContacto;

        set => SetProperty(
            ref fechaContacto,
            value);
    }

    private string resultado = "";

    public string Resultado
    {
        get => resultado;

        set => SetProperty(
            ref resultado,
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

    public ContactoViewModel()
    {
        CargarEstudiantesComision();
        ConsultarContactos();
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
    private void RegistrarContacto()
    {
        if (EstudianteSeleccionado is null)
        {
            Mensaje =
                "Seleccione un estudiante";

            return;
        }

        string resultadoLimpio =
            Resultado.Trim();

        if (string.IsNullOrWhiteSpace(resultadoLimpio))
        {
            Mensaje =
                "Ingrese el resultado del contacto";

            return;
        }

        if (FechaContacto.Date > DateTime.Today)
        {
            Mensaje =
                "La fecha del contacto no puede ser futura";

            return;
        }

        var nuevoContacto = new ContactoEstudiante
        {
            Estudiante =
                EstudianteSeleccionado,

            Fecha =
                FechaContacto.Date,

            Resultado =
                resultadoLimpio
        };

        try
        {
            DatosSistema.AgregarContacto(
                nuevoContacto);

            Resultado = "";

            Mensaje =
                "Contacto registrado correctamente";

            ConsultarContactos();
        }
        catch (Exception ex)
        {
            Mensaje =
                $"No se pudo guardar el contacto: {ex.Message}";
        }
    }

    [RelayCommand]
    private void ConsultarContactos()
    {
        Contactos.Clear();

        if (EstudianteSeleccionado is null)
        {
            return;
        }

        var contactosEstudiante =
            DatosSistema.Contactos
                .Where(c =>
                    c.Estudiante.Legajo ==
                    EstudianteSeleccionado.Legajo)
                .OrderByDescending(c => c.Fecha)
                .ToList();

        foreach (ContactoEstudiante contacto
                 in contactosEstudiante)
        {
            Contactos.Add(contacto);
        }
    }
}
