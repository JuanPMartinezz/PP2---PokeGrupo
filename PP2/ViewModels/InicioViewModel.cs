using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RegistroAsistencia.Services;
using System.Collections.ObjectModel;

namespace RegistroAsistencia.ViewModels;

// Una comisión en la pantalla de inicio, con su botón
public sealed class ItemComision
{
    public ItemComision(
        string nombre,
        string detalle,
        IAsyncRelayCommand abrir)
    {
        Nombre = nombre;
        Detalle = detalle;
        Abrir = abrir;
    }

    public string Nombre { get; }

    public string Detalle { get; }

    public IAsyncRelayCommand Abrir { get; }
}

/// <summary>
/// Pantalla de inicio: un botón por comisión, agregar comisión,
/// agregar alumno y administrar datos.
/// </summary>
public partial class InicioViewModel : ObservableObject, IActualizable
{
    private readonly INavegacion _navegacion;

    public ObservableCollection<ItemComision> Comisiones { get; } = new();

    [ObservableProperty]
    private bool sinComisiones;

    public InicioViewModel(INavegacion navegacion)
    {
        _navegacion = navegacion;

        Actualizar();
    }

    public void Actualizar()
    {
        Comisiones.Clear();

        foreach (string nombre in DatosSistema.Comisiones)
        {
            Comisiones.Add(CrearItem(nombre));
        }

        SinComisiones = Comisiones.Count == 0;
    }

    private ItemComision CrearItem(string nombre)
    {
        int cantidad = DatosSistema.Estudiantes
            .Count(e => e.Comision == nombre);

        var abrir = new AsyncRelayCommand(() =>
            _navegacion.IrAAsync(Destino.Comision, nombre));

        return new ItemComision(nombre, TextoAlumnos(cantidad), abrir);
    }

    private static string TextoAlumnos(int cantidad)
    {
        return cantidad == 1
            ? "1 alumno"
            : $"{cantidad} alumnos";
    }

    [RelayCommand]
    private Task NuevaComision()
    {
        return _navegacion.IrAAsync(Destino.NuevaComision);
    }

    [RelayCommand]
    private Task NuevoAlumno()
    {
        return _navegacion.IrAAsync(Destino.AlumnoNuevo);
    }

    [RelayCommand]
    private Task Administrar()
    {
        return _navegacion.IrAAsync(Destino.Administrar);
    }
}
