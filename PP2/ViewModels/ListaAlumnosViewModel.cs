using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RegistroAsistencia.Models;
using RegistroAsistencia.Services;
using System.Collections.ObjectModel;

namespace RegistroAsistencia.ViewModels;

// Un alumno en la lista: solo nombre y riesgo; al tocarlo se abre su ficha
public sealed class ItemAlumno
{
    public ItemAlumno(ResultadoRiesgo resultado, IAsyncRelayCommand abrir)
    {
        Resultado = resultado;
        Abrir = abrir;
    }

    public ResultadoRiesgo Resultado { get; }

    // Riesgo alto y sin contacto reciente: hay que llamarlo
    public bool Llamar => Resultado.ParaLlamar(DateTime.Today);

    public string Nombre =>
        $"{Resultado.Estudiante.Apellido}, {Resultado.Estudiante.Nombre}";

    public string Riesgo => Resultado.RiesgoTexto;

    public string Nivel => Resultado.NivelRiesgo;

    public IAsyncRelayCommand Abrir { get; }
}

/// <summary>
/// Lista de alumnos de una comisión (H8 y H11): se puede cambiar de
/// comisión, ordenar por apellido o por riesgo, y filtrar por contacto
/// y por nivel de riesgo.
/// </summary>
public partial class ListaAlumnosViewModel : ObservableObject, IActualizable
{
    private readonly INavegacion _navegacion;

    private readonly RiesgoService _riesgo;

    public ObservableCollection<string> Comisiones { get; }

    public IReadOnlyList<string> Ordenes { get; } = new[]
    {
        "Apellido A-Z",
        "Apellido Z-A",
        "Mayor riesgo"
    };

    public IReadOnlyList<string> FiltrosContacto { get; } = new[]
    {
        "Todos",
        "Sin contactar",
        $"Sin contacto hace más de {ResultadoRiesgo.DiasSinContactoReciente} días",
        "Para llamar (riesgo alto, sin contacto reciente)"
    };

    public IReadOnlyList<string> FiltrosNivel { get; } = new[]
    {
        "Todos los niveles",
        NivelesRiesgo.Alto,
        NivelesRiesgo.Medio,
        NivelesRiesgo.Bajo
    };

    public ObservableCollection<ItemAlumno> Alumnos { get; } = new();

    [ObservableProperty]
    private string? comisionSeleccionada;

    [ObservableProperty]
    private int ordenIndice;

    [ObservableProperty]
    private int contactoIndice;

    [ObservableProperty]
    private int nivelIndice;

    [ObservableProperty]
    private string resumen = "";

    public ListaAlumnosViewModel(
        string comision,
        INavegacion navegacion,
        RiesgoService riesgo,
        ComisionService comisiones)
    {
        _navegacion = navegacion;
        _riesgo = riesgo;
        Comisiones = new ObservableCollection<string>(comisiones.Todas());
        comisionSeleccionada = comision;

        Actualizar();
    }

    partial void OnComisionSeleccionadaChanged(string? value) => Actualizar();

    partial void OnOrdenIndiceChanged(int value) => Actualizar();

    partial void OnContactoIndiceChanged(int value) => Actualizar();

    partial void OnNivelIndiceChanged(int value) => Actualizar();

    public void Actualizar()
    {
        Alumnos.Clear();

        if (ComisionSeleccionada is null)
        {
            Resumen = "";
            return;
        }

        List<ResultadoRiesgo> todos =
            _riesgo.ObtenerResultados(ComisionSeleccionada);

        List<ResultadoRiesgo> mostrados = ListadoAlumnos.Aplicar(
            todos,
            (OrdenLista)Math.Max(OrdenIndice, 0),
            (FiltroContacto)Math.Max(ContactoIndice, 0),
            (FiltroNivel)Math.Max(NivelIndice, 0),
            DateTime.Today);

        foreach (ResultadoRiesgo r in mostrados)
        {
            Alumnos.Add(CrearItem(r));
        }

        Resumen = TextoResumen(mostrados.Count, todos.Count);
    }

    private ItemAlumno CrearItem(ResultadoRiesgo resultado)
    {
        string legajo = resultado.Estudiante.Legajo;

        var abrir = new AsyncRelayCommand(() =>
            _navegacion.IrAAsync(Destino.Ficha, legajo));

        return new ItemAlumno(resultado, abrir);
    }

    private static string TextoResumen(int mostrados, int total)
    {
        if (total == 0)
        {
            return "Esta comisión todavía no tiene alumnos";
        }

        return mostrados == 0
            ? "Ningún alumno cumple con esos filtros"
            : $"Mostrando {mostrados} de {total} alumnos";
    }
}
