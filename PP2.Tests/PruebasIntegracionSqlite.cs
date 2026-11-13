using RegistroAsistencia.Datos;
using RegistroAsistencia.Models;
using RegistroAsistencia.Services;

namespace PP2.Tests;

/// <summary>
/// Pruebas de integración: los servicios reales sobre repositorios SQLite
/// reales, con una base en memoria. Comprueban lo que las pruebas con
/// repositorios en memoria no pueden: que lo guardado se lee igual, que
/// las tablas se relacionan bien y que el flujo completo funciona.
/// </summary>
public sealed class PruebasIntegracionSqlite : IDisposable
{
    private const string ComisionA = "Comisión A";

    private readonly BaseDatos _db;

    private readonly Servicios _s;

    // Cada prueba tiene su propia base en memoria, vacía
    public PruebasIntegracionSqlite()
    {
        _db = new BaseDatos(":memory:");
        _s = new Servicios(_db);
        _s.Comisiones.Inicializar();
    }

    public void Dispose()
    {
        _db.Conexion.Dispose();
    }

    // Los mismos servicios que arma MauiProgram, sobre una base dada
    private sealed class Servicios
    {
        public ComisionService Comisiones { get; }

        public NominaService Nomina { get; }

        public RegistroService Registro { get; }

        public ContactoService Contactos { get; }

        public ImportacionService Importacion { get; }

        public RiesgoService Riesgo { get; }

        public Servicios(BaseDatos db)
        {
            var comisiones = new ComisionRepositorioSqlite(db);
            var estudiantes = new EstudianteRepositorioSqlite(db);
            var asistencias = new AsistenciaRepositorioSqlite(db);
            var entregas = new EntregaRepositorioSqlite(db);
            var calendario = new CalendarioRepositorioSqlite(db);
            var contactos = new ContactoRepositorioSqlite(db);

            var umbrales = new UmbralesService(comisiones);
            var servicioCalendario = new CalendarioService(calendario, entregas);

            Comisiones = new ComisionService(
                comisiones,
                estudiantes,
                new AdministracionRepositorioSqlite(db));
            Nomina = new NominaService(estudiantes, comisiones);
            Registro = new RegistroService(asistencias, entregas);
            Contactos = new ContactoService(contactos, estudiantes);
            Importacion = new ImportacionService(
                Nomina,
                Registro,
                servicioCalendario,
                new UnidadDeTrabajoSqlite(db));
            Riesgo = new RiesgoService(
                estudiantes, asistencias, entregas, contactos, umbrales);
        }
    }

    private static string CsvDeEjemplo()
    {
        string ruta = Path.Combine(
            AppContext.BaseDirectory, "datos-ejemplo", "alumnos-demo.csv");

        return File.ReadAllText(ruta);
    }

    [Fact]
    public void Integracion_ElCsvDeEjemplo_SeImportaCompletoYElReporteDaLosNivelesEsperados()
    {
        ResultadoImportacion r = _s.Importacion.Importar(CsvDeEjemplo());

        Assert.Empty(r.Errores);
        Assert.Equal(200, r.AlumnosNuevos);
        Assert.Equal(1400, r.Registros);

        // 100 alumnos por comisión, ninguno sin datos
        var esperado = new ResumenComision(100, 18, 14, 68, 0);
        Assert.Equal(esperado, _s.Riesgo.ObtenerResumen("Comisión A"));
        Assert.Equal(esperado, _s.Riesgo.ObtenerResumen("Comisión B"));
    }

    [Fact]
    public void Integracion_LoGuardadoSeLeeIgualDespuesDeCerrarYAbrirLaBase()
    {
        string archivo = Path.Combine(
            Path.GetTempPath(), $"pp2-{Guid.NewGuid():N}.db3");

        try
        {
            GuardarAlumnoEnArchivo(archivo);

            var reabierta = new BaseDatos(archivo);
            Estudiante? ana = new Servicios(reabierta).Nomina.Buscar("101");
            reabierta.Conexion.Dispose();

            Assert.NotNull(ana);
            Assert.Equal(ComisionA, ana!.Comision);
            Assert.Equal(DateTime.Today.AddDays(-9), ana.FechaIncorporacion);
        }
        finally
        {
            File.Delete(archivo);
        }
    }

    private static void GuardarAlumnoEnArchivo(string archivo)
    {
        var db = new BaseDatos(archivo);
        var servicios = new Servicios(db);

        servicios.Comisiones.Inicializar();
        servicios.Nomina.Agregar(
            "Ana", "Gómez", "101", ComisionA, DateTime.Today.AddDays(-9));

        db.Conexion.Dispose();
    }

}
