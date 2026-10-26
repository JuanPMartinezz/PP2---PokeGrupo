using RegistroAsistencia.Models;
using RegistroAsistencia.Services;
using RegistroAsistencia.ViewModels;

namespace PP2.Tests;

/// <summary>
/// Pruebas por historia de usuario (dos por historia). Todas viven en una
/// sola clase porque DatosSistema guarda sus datos en memoria y cada
/// prueba arranca de cero: xUnit no corre en paralelo las de una clase.
/// </summary>
public class PruebasPP2
{
    private const string ComisionA = "Comisión A";
    private const string ComisionB = "Comisión B";

    private readonly NavegacionFalsa _navegacion = new();
    private readonly DialogosFalsos _dialogos = new();
    private readonly RegistroService _registro = new();
    private readonly RiesgoService _riesgo = new();

    public PruebasPP2()
    {
        DatosSistema.BorrarTodosLosRegistros();
    }

    // ---------- Ayudas ----------

    private sealed class NavegacionFalsa : INavegacion
    {
        public List<(Destino Destino, string? Dato)> Visitas { get; } = new();

        public int Volvio { get; private set; }

        public Task IrAAsync(Destino destino, string? dato = null)
        {
            Visitas.Add((destino, dato));
            return Task.CompletedTask;
        }

        public Task VolverAsync()
        {
            Volvio++;
            return Task.CompletedTask;
        }
    }

    private sealed class DialogosFalsos : IDialogos
    {
        public bool Respuesta { get; set; } = true;

        public int Preguntas { get; private set; }

        public Task<bool> ConfirmarAsync(
            string titulo, string mensaje, string aceptar, string cancelar)
        {
            Preguntas++;
            return Task.FromResult(Respuesta);
        }

        public Task<string?> PedirTextoAsync(
            string titulo, string mensaje, string ejemplo)
        {
            Preguntas++;
            return Task.FromResult<string?>(null);
        }
    }

    private static Estudiante Alumno(
        string legajo,
        string apellido,
        string nombre = "Ana",
        string comision = ComisionA)
    {
        var estudiante = new Estudiante
        {
            Legajo = legajo,
            Apellido = apellido,
            Nombre = nombre,
            Comision = comision
        };

        DatosSistema.AgregarEstudiante(estudiante);

        return estudiante;
    }

    private static void RegistrarAsistencia(
        Estudiante estudiante, EstadoAsistencia estado, int diasAtras = 0)
    {
        DatosSistema.AgregarAsistencia(new Asistencia
        {
            Estudiante = estudiante,
            Clase = new Clase { Fecha = DateTime.Today.AddDays(-diasAtras) },
            Estado = estado
        });
    }

    private static Trabajo CargarTrabajo(string nombre, string comision = ComisionA)
    {
        DatosSistema.AgregarActividadCalendario(new ActividadCalendario
        {
            Comision = comision,
            Cuatrimestre = Cuatrimestres.Actual(),
            Fecha = DateTime.Today.AddDays(3),
            Tipo = TipoActividad.Trabajo,
            Descripcion = nombre
        });

        return DatosSistema.ObtenerOCrearTrabajo(nombre);
    }

    private static void RegistrarContacto(Estudiante estudiante, int diasAtras)
    {
        DatosSistema.AgregarContacto(new ContactoEstudiante
        {
            Estudiante = estudiante,
            Fecha = DateTime.Today.AddDays(-diasAtras),
            Resultado = "Llamada"
        });
    }

    private AsistenciaViewModel NuevaPlanilla()
    {
        return new AsistenciaViewModel(ComisionA, _navegacion, _dialogos, _registro);
    }

    // ---------- H1: alumnos ----------

    [Fact]
    public void H1_AlumnoSinLegajo_PideCompletarLosDatos()
    {
        string? error = Validaciones.Alumno("Ana", "Gómez", "", ComisionA);

        Assert.Equal("Completá nombre, apellido y legajo", error);
    }

    [Fact]
    public async Task H1_LegajoRepetido_AvisaQuienLoTieneYNoDuplica()
    {
        Alumno("101", "Gómez");
        var formulario = new AlumnoFormViewModel(_navegacion)
        {
            Nombre = "Luis",
            Apellido = "Paz",
            Legajo = "101"
        };

        await formulario.GuardarCommand.ExecuteAsync(null);

        Assert.Contains("Ya existe el legajo 101", formulario.Mensaje);
        Assert.Single(DatosSistema.Estudiantes);
    }

    // ---------- H2: calendario ----------

    [Fact]
    public void H2_ClaseNuevaSeGuardaSinPedirNombre()
    {
        var calendario = new CalendarioViewModel(ComisionA) { TipoIndice = 0 };

        calendario.AgregarCommand.Execute(null);

        ActividadCalendario actividad = Assert.Single(DatosSistema.Calendario);
        Assert.Equal(TipoActividad.Clase, actividad.Tipo);
        Assert.Equal(Cuatrimestres.De(DateTime.Today), actividad.Cuatrimestre);
    }

    [Fact]
    public void H2_TrabajoRepetidoConLaMismaFecha_SeRechaza()
    {
        var calendario = new CalendarioViewModel(ComisionA)
        {
            TipoIndice = 1,
            Descripcion = "TP1"
        };
        calendario.AgregarCommand.Execute(null);
        calendario.Descripcion = "TP1";

        calendario.AgregarCommand.Execute(null);

        Assert.Single(DatosSistema.Calendario);
        Assert.Equal("Ese trabajo ya está cargado con esa fecha", calendario.Mensaje);
    }

    // ---------- H3: tomar asistencia ----------

    [Fact]
    public async Task H3_LaPrimeraVezSeGuardaSinPreguntar()
    {
        Alumno("101", "Gómez");
        Alumno("102", "Paz");
        AsistenciaViewModel planilla = NuevaPlanilla();
        planilla.Filas.First(f => f.Estudiante.Legajo == "101").Presente = true;

        await planilla.GuardarCommand.ExecuteAsync(null);

        Assert.Equal(0, _dialogos.Preguntas);
        Assert.Equal(EstadoAsistencia.Presente,
            DatosSistema.BuscarAsistencia("101", DateTime.Today)!.Estado);
        Assert.Equal(EstadoAsistencia.Ausente,
            DatosSistema.BuscarAsistencia("102", DateTime.Today)!.Estado);
    }

    [Fact]
    public void H3_GuardarDosVecesLaMismaFecha_NoDuplicaRegistros()
    {
        Estudiante ana = Alumno("101", "Gómez");
        var marcas = new[] { new MarcaAsistencia(ana, true) };

        _registro.GuardarAsistencia(DateTime.Today, marcas);
        ResultadoGuardado segundo = _registro.GuardarAsistencia(DateTime.Today, marcas);

        Assert.Single(DatosSistema.Asistencias);
        Assert.Equal(0, segundo.Nuevos + segundo.Modificados);
    }

    // ---------- H4: corregir asistencia ----------

    [Fact]
    public async Task H4_CorregirUnRegistroGuardado_PideConfirmacionYSiSeCancelaNoCambia()
    {
        Alumno("101", "Gómez");
        await NuevaPlanilla().GuardarCommand.ExecuteAsync(null);
        AsistenciaViewModel planilla = NuevaPlanilla();
        planilla.Filas[0].Presente = true;
        _dialogos.Respuesta = false;

        await planilla.GuardarCommand.ExecuteAsync(null);

        Assert.Equal(1, _dialogos.Preguntas);
        Assert.Equal(EstadoAsistencia.Ausente,
            DatosSistema.BuscarAsistencia("101", DateTime.Today)!.Estado);
    }

    [Fact]
    public async Task H4_CorregirUnRegistroGuardado_SiSeAceptaSeActualiza()
    {
        Alumno("101", "Gómez");
        await NuevaPlanilla().GuardarCommand.ExecuteAsync(null);
        AsistenciaViewModel planilla = NuevaPlanilla();
        planilla.Filas[0].Presente = true;

        await planilla.GuardarCommand.ExecuteAsync(null);

        Assert.Equal(EstadoAsistencia.Presente,
            DatosSistema.BuscarAsistencia("101", DateTime.Today)!.Estado);
        Assert.Single(DatosSistema.Asistencias);
    }

    // ---------- H5: entregas ----------

    [Fact]
    public async Task H5_GuardaElEstadoDeCadaEntrega()
    {
        Alumno("101", "Gómez");
        Alumno("102", "Paz");
        Trabajo tp1 = CargarTrabajo("TP1");
        var planilla = new EntregaViewModel(ComisionA, _navegacion, _dialogos, _registro);
        planilla.Filas.First(f => f.Estudiante.Legajo == "101").EstadoTexto = "Entregado";
        planilla.Filas.First(f => f.Estudiante.Legajo == "102").EstadoTexto = "Fuera de término";

        await planilla.GuardarCommand.ExecuteAsync(null);

        Assert.Equal(EstadoEntrega.Entregado,
            DatosSistema.BuscarEntrega("101", tp1.Id)!.Estado);
        Assert.Equal(EstadoEntrega.EntregadoFueraDeTermino,
            DatosSistema.BuscarEntrega("102", tp1.Id)!.Estado);
    }

    [Fact]
    public void H5_CerrarLaEntrega_MarcaNoEntregadoSoloALosPendientes()
    {
        Estudiante ana = Alumno("101", "Gómez");
        Estudiante luis = Alumno("102", "Paz");
        Trabajo tp1 = CargarTrabajo("TP1");
        _registro.GuardarEntregas(tp1, new[] { new MarcaEntrega(ana, EstadoEntrega.Entregado) });

        int cerrados = _registro.CerrarEntrega(tp1, new[] { ana, luis });

        Assert.Equal(1, cerrados);
        Assert.Equal(EstadoEntrega.Entregado, DatosSistema.BuscarEntrega("101", tp1.Id)!.Estado);
        Assert.Equal(EstadoEntrega.NoEntregado, DatosSistema.BuscarEntrega("102", tp1.Id)!.Estado);
    }

    // ---------- H6: riesgo ----------

    [Fact]
    public void H6_UnaAusenciaEnCuatroClases_SonVeinticincoPorCiento()
    {
        Estudiante ana = Alumno("101", "Gómez");
        for (int i = 0; i < 3; i++)
        {
            RegistrarAsistencia(ana, EstadoAsistencia.Presente, i);
        }
        RegistrarAsistencia(ana, EstadoAsistencia.Ausente, 3);

        Assert.Equal(25m, _riesgo.CalcularPorcentajeAusencias(ana));
    }

    [Fact]
    public void H6_AsistenciaYEntregasSeCombinanConLosPesosDeLaComision()
    {
        Estudiante ana = Alumno("101", "Gómez");
        RegistrarAsistencia(ana, EstadoAsistencia.Presente, 1);
        RegistrarAsistencia(ana, EstadoAsistencia.Ausente, 0);
        Trabajo tp1 = CargarTrabajo("TP1");
        DatosSistema.AgregarEntrega(new Entrega
        {
            Estudiante = ana,
            Trabajo = tp1,
            Estado = EstadoEntrega.NoEntregado
        });

        // 50 % de ausencias y 100 % de riesgo en la entrega, 50/50
        Assert.Equal(75m, _riesgo.CalcularRiesgo(ana));
    }

    [Fact]
    public void H6_SinNingunRegistro_NoHayDatosYElRiesgoEsCero()
    {
        Estudiante ana = Alumno("101", "Gómez");

        ResultadoRiesgo resultado = _riesgo.ObtenerResultado(ana);

        Assert.Equal(0m, resultado.PorcentajeRiesgo);
        Assert.Equal(NivelesRiesgo.SinDatos, resultado.NivelRiesgo);
    }

    [Fact]
    public void H6_LosLimitesDeLosNivelesCuentanHaciaArriba()
    {
        Assert.Equal(NivelesRiesgo.Alto, _riesgo.ObtenerNivel(ComisionA, 60m, 1));
        Assert.Equal(NivelesRiesgo.Medio, _riesgo.ObtenerNivel(ComisionA, 59.99m, 1));
        Assert.Equal(NivelesRiesgo.Medio, _riesgo.ObtenerNivel(ComisionA, 30m, 1));
        Assert.Equal(NivelesRiesgo.Bajo, _riesgo.ObtenerNivel(ComisionA, 29.99m, 1));
    }

    // ---------- H7: configuración por comisión ----------

    [Fact]
    public void H7_CadaComisionTieneSusPropiosUmbrales()
    {
        DatosSistema.GuardarUmbrales(ComisionB, new UmbralesRiesgo
        {
            RiesgoMedio = 40,
            RiesgoAlto = 80
        });

        Assert.Equal(NivelesRiesgo.Alto, _riesgo.ObtenerNivel(ComisionA, 70m, 1));
        Assert.Equal(NivelesRiesgo.Medio, _riesgo.ObtenerNivel(ComisionB, 70m, 1));
    }

    [Fact]
    public void H7_ValoresInvalidos_NoSeGuardanYSeExplicaPorQue()
    {
        var configuracion = new ConfigRiesgoViewModel(ComisionA)
        {
            RiesgoMedio = "70",
            RiesgoAlto = "50"
        };

        configuracion.GuardarCommand.Execute(null);

        Assert.Contains("no puede ser mayor", configuracion.Mensaje);
        Assert.Equal(60m, DatosSistema.ObtenerUmbrales(ComisionA).RiesgoAlto);
    }

    // ---------- H8: lista de alumnos ----------

    [Fact]
    public void H8_ListaPorApellido_AZyZA()
    {
        Alumno("1", "Paz", "Luis");
        Alumno("2", "Alvarez", "Eva");
        Alumno("3", "Gómez", "Ana");
        var lista = new ListaAlumnosViewModel(ComisionA, _navegacion, _riesgo);

        string[] az = lista.Alumnos.Select(a => a.Nombre).ToArray();
        lista.OrdenIndice = (int)OrdenLista.ApellidoZA;
        string[] za = lista.Alumnos.Select(a => a.Nombre).ToArray();

        Assert.Equal(new[] { "Alvarez, Eva", "Gómez, Ana", "Paz, Luis" }, az);
        Assert.Equal(Enumerable.Reverse(az), za);
    }

    [Fact]
    public void H8_MayorRiesgoPrimero_YSePuedeCambiarDeComision()
    {
        Estudiante ana = Alumno("1", "Gómez");
        Estudiante luis = Alumno("2", "Paz");
        Alumno("3", "Otra", "Eva", ComisionB);
        RegistrarAsistencia(ana, EstadoAsistencia.Presente);
        RegistrarAsistencia(luis, EstadoAsistencia.Ausente);
        var lista = new ListaAlumnosViewModel(ComisionA, _navegacion, _riesgo)
        {
            OrdenIndice = (int)OrdenLista.MayorRiesgo
        };

        Assert.Equal("Paz, Ana", lista.Alumnos[0].Nombre);

        lista.ComisionSeleccionada = ComisionB;

        Assert.Equal("Otra, Eva", Assert.Single(lista.Alumnos).Nombre);
    }

    // ---------- H9: ficha del alumno ----------

    [Fact]
    public void H9_LaFichaMuestraLosDatosCompletos()
    {
        Estudiante ana = Alumno("101", "Gómez");
        RegistrarAsistencia(ana, EstadoAsistencia.Ausente);
        RegistrarContacto(ana, 2);

        var ficha = new FichaAlumnoViewModel("101", _navegacion, _dialogos, _riesgo);

        Assert.Equal("Gómez, Ana", ficha.NombreCompleto);
        Assert.Contains("101", ficha.Datos);
        Assert.Contains("1 ausencias", ficha.AsistenciaTexto);
        Assert.Single(ficha.Contactos);
    }

    [Fact]
    public async Task H9_EliminarAlumno_PideConfirmacionYBorraSusRegistros()
    {
        Estudiante ana = Alumno("101", "Gómez");
        RegistrarAsistencia(ana, EstadoAsistencia.Presente);
        var ficha = new FichaAlumnoViewModel("101", _navegacion, _dialogos, _riesgo);
        _dialogos.Respuesta = false;

        await ficha.EliminarCommand.ExecuteAsync(null);
        Assert.Single(DatosSistema.Estudiantes);

        _dialogos.Respuesta = true;
        await ficha.EliminarCommand.ExecuteAsync(null);

        Assert.Empty(DatosSistema.Estudiantes);
        Assert.Empty(DatosSistema.Asistencias);
    }

    // ---------- H10: contactos ----------

    [Fact]
    public async Task H10_ContactoQueNoAtendio_QuedaRegistradoConSuTexto()
    {
        Alumno("101", "Gómez");
        var contacto = new ContactoViewModel("101", _navegacion)
        {
            Atendio = false,
            Resultado = "Sonó y no contestó"
        };

        await contacto.GuardarCommand.ExecuteAsync(null);

        ContactoEstudiante guardado = Assert.Single(DatosSistema.Contactos);
        Assert.False(guardado.Atendio);
        Assert.Equal(ContactoEstudiante.TextoNoAtendio, guardado.EstadoTexto);
        Assert.Equal(1, _navegacion.Volvio);
    }

    [Fact]
    public async Task H10_ContactoSinResultadoOConFechaFutura_SeRechaza()
    {
        Alumno("101", "Gómez");
        var contacto = new ContactoViewModel("101", _navegacion);

        await contacto.GuardarCommand.ExecuteAsync(null);
        string sinResultado = contacto.Mensaje;

        contacto.Resultado = "Llamada";
        contacto.Fecha = DateTime.Today.AddDays(2);
        await contacto.GuardarCommand.ExecuteAsync(null);

        Assert.NotEqual("", sinResultado);
        Assert.Equal("La fecha del contacto no puede ser futura", contacto.Mensaje);
        Assert.Empty(DatosSistema.Contactos);
    }

    // ---------- H11: filtros de la lista ----------

    [Fact]
    public void H11_SinContactar_MuestraSoloALosQueNuncaSeContactaron()
    {
        Estudiante ana = Alumno("1", "Gómez");
        Alumno("2", "Paz", "Luis");
        RegistrarContacto(ana, 1);

        List<ResultadoRiesgo> filtrados = ListadoAlumnos.Aplicar(
            _riesgo.ObtenerResultados(ComisionA),
            OrdenLista.ApellidoAZ,
            FiltroContacto.SinContactar,
            FiltroNivel.Todos,
            DateTime.Today);

        Assert.Equal("2", Assert.Single(filtrados).Estudiante.Legajo);
    }

    [Fact]
    public void H11_SinContactoReciente_IncluyeAQuienNoTieneContactoOLoTieneViejo()
    {
        Estudiante reciente = Alumno("1", "Gómez");
        Estudiante viejo = Alumno("2", "Paz", "Luis");
        Alumno("3", "Rey", "Eva");
        RegistrarContacto(reciente, 2);
        RegistrarContacto(viejo, 10);

        List<ResultadoRiesgo> filtrados = ListadoAlumnos.Aplicar(
            _riesgo.ObtenerResultados(ComisionA),
            OrdenLista.ApellidoAZ,
            FiltroContacto.SinContactoReciente,
            FiltroNivel.Todos,
            DateTime.Today);

        Assert.Equal(new[] { "2", "3" }, filtrados.Select(r => r.Estudiante.Legajo));
    }

    // ---------- Comisiones ----------

    [Fact]
    public void Comisiones_NoSePuedeRepetirElNombre()
    {
        DatosSistema.AgregarComision("Comisión C");

        var ex = Assert.Throws<InvalidOperationException>(
            () => DatosSistema.AgregarComision("comisión c"));

        Assert.Equal("Ya existe una comisión con ese nombre", ex.Message);
    }

    [Fact]
    public void Comisiones_EliminarUnaBorraSusAlumnosYRegistros()
    {
        Estudiante ana = Alumno("1", "Gómez", "Ana", ComisionB);
        Alumno("2", "Paz", "Luis", ComisionA);
        RegistrarAsistencia(ana, EstadoAsistencia.Presente);

        DatosSistema.EliminarComision(ComisionB);

        Assert.DoesNotContain(ComisionB, DatosSistema.Comisiones);
        Assert.Equal("2", Assert.Single(DatosSistema.Estudiantes).Legajo);
        Assert.Empty(DatosSistema.Asistencias);
    }

    [Fact]
    public void Comisiones_RenombrarActualizaLosAlumnos()
    {
        Alumno("1", "Gómez");

        DatosSistema.RenombrarComision(ComisionA, "Comisión Noche");

        Assert.Equal("Comisión Noche", DatosSistema.Estudiantes[0].Comision);
        Assert.Contains("Comisión Noche", DatosSistema.Comisiones);
    }

    [Fact]
    public async Task Administrar_BorrarTodo_DejaLasComisionesIniciales()
    {
        DatosSistema.AgregarComision("Comisión C");
        Alumno("1", "Gómez");
        var administrar = new AdministrarViewModel(_dialogos);

        await administrar.BorrarTodoCommand.ExecuteAsync(null);

        Assert.Empty(DatosSistema.Estudiantes);
        Assert.Equal(DatosSistema.ComisionesIniciales, DatosSistema.Comisiones.ToArray());
    }
}
