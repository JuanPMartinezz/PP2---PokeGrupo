using RegistroAsistencia.Models;
using RegistroAsistencia.Services;
using RegistroAsistencia.ViewModels;

namespace PP2.Tests;

/// <summary>
/// Pruebas por historia de usuario (dos por historia, más los casos borde
/// del riesgo). Cada prueba arma su propio Escenario con repositorios en
/// memoria: no hay base de datos ni datos compartidos entre pruebas.
/// </summary>
public class PruebasPP2
{
    private const string ComisionA = "Comisión A";
    private const string ComisionB = "Comisión B";

    private readonly Escenario _e = new();
    private readonly NavegacionFalsa _navegacion = new();
    private readonly DialogosFalsos _dialogos = new();

    // ---------- Ayudas ----------

    private sealed class NavegacionFalsa : INavegacion
    {
        public List<(Destino Destino, string? Dato)> Visitas { get; } = new();

        public int Volvio { get; private set; }

        public int VolvioAlInicio { get; private set; }

        public Task IrAAsync(Destino destino, string? dato = null)
        {
            Visitas.Add((destino, dato));
            return Task.CompletedTask;
        }

        public Task VolverAlInicioAsync()
        {
            VolvioAlInicio++;
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

    private Estudiante Alumno(
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

        _e.Estudiantes.Agregar(estudiante);

        return estudiante;
    }

    private void RegistrarAsistencia(
        Estudiante estudiante, EstadoAsistencia estado, int diasAtras = 0)
    {
        _e.Asistencias.Agregar(new Asistencia
        {
            Estudiante = estudiante,
            Clase = new Clase { Fecha = DateTime.Today.AddDays(-diasAtras) },
            Estado = estado
        });
    }

    private Trabajo CargarTrabajo(string nombre, string comision = ComisionA)
    {
        _e.Calendario.Agregar(new ActividadCalendario
        {
            Comision = comision,
            Cuatrimestre = Cuatrimestres.Actual(),
            Fecha = DateTime.Today.AddDays(3),
            Tipo = TipoActividad.Trabajo,
            Descripcion = nombre
        });

        return _e.Entregas.ObtenerOCrearTrabajo(nombre);
    }

    private void RegistrarContacto(Estudiante estudiante, int diasAtras)
    {
        _e.Contactos.Agregar(new ContactoEstudiante
        {
            Estudiante = estudiante,
            Fecha = DateTime.Today.AddDays(-diasAtras),
            Resultado = "Llamada"
        });
    }

    private AsistenciaViewModel NuevaPlanilla()
    {
        return new AsistenciaViewModel(ComisionA, _e.Nomina, _dialogos, _e.Registro);
    }

    private ListaAlumnosViewModel NuevaLista(string comision = ComisionA)
    {
        return new ListaAlumnosViewModel(
            comision, _navegacion, _e.Riesgo, _e.Comisiones);
    }

    private AlumnoFormViewModel NuevoFormulario()
    {
        return new AlumnoFormViewModel(_navegacion, _e.Nomina, _e.Comisiones);
    }

    // ---------- H1: alumnos ----------

    [Fact]
    public async Task H1_LegajoRepetido_AvisaQuienLoTieneYNoDuplica()
    {
        Alumno("101", "Gómez");
        AlumnoFormViewModel formulario = NuevoFormulario();
        formulario.Nombre = "Luis";
        formulario.Apellido = "Paz";
        formulario.Legajo = "101";

        await formulario.GuardarCommand.ExecuteAsync(null);

        Assert.True(formulario.EsError);
        Assert.Contains("Ya existe el legajo 101", formulario.Mensaje);
        Assert.Single(_e.Almacen.Estudiantes);
    }

    [Theory]
    [InlineData("abc!")]
    [InlineData("12")]
    [InlineData("123456789")]
    [InlineData("12 34")]
    public async Task H1_LegajoQueNoSonDe3a8Digitos_SeRechaza(string legajo)
    {
        AlumnoFormViewModel formulario = NuevoFormulario();
        formulario.Nombre = "Luis";
        formulario.Apellido = "Paz";
        formulario.Legajo = legajo;

        await formulario.GuardarCommand.ExecuteAsync(null);

        Assert.True(formulario.EsError);
        Assert.Contains("dígitos", formulario.Mensaje);
        Assert.Empty(_e.Almacen.Estudiantes);
    }

    [Fact]
    public async Task H1_AlumnoValido_SeAgregaYElMensajeNoEsDeError()
    {
        AlumnoFormViewModel formulario = NuevoFormulario();
        formulario.Nombre = "Luis";
        formulario.Apellido = "Paz";
        formulario.Legajo = "20458";

        await formulario.GuardarCommand.ExecuteAsync(null);

        Assert.False(formulario.EsError);
        Assert.Equal("20458", Assert.Single(_e.Almacen.Estudiantes).Legajo);
    }

    // ---------- H2: calendario ----------

    [Fact]
    public async Task H2_LasClasesSalenDeLaAsistenciaTomada()
    {
        Alumno("101", "Gómez");
        Alumno("102", "Paz");
        AsistenciaViewModel planilla = NuevaPlanilla();
        planilla.Filas.First(f => f.Estudiante.Legajo == "101").Presente = true;
        await planilla.GuardarCommand.ExecuteAsync(null);

        var calendario = new CalendarioViewModel(
            ComisionA, _e.ServicioCalendario, _e.Registro);

        string clase = Assert.Single(calendario.Clases);
        Assert.Contains("Presentes: 1", clase);
        Assert.Contains("Ausentes: 1", clase);
    }

    [Fact]
    public void H2_TrabajoRepetidoConLaMismaFecha_SeRechaza()
    {
        var calendario = new CalendarioViewModel(
            ComisionA, _e.ServicioCalendario, _e.Registro)
        {
            Descripcion = "TP1"
        };
        calendario.AgregarCommand.Execute(null);
        calendario.Descripcion = "TP1";

        calendario.AgregarCommand.Execute(null);

        Assert.Single(_e.Almacen.Calendario);
        Assert.True(calendario.EsError);
        Assert.Equal("Ese trabajo ya está cargado con esa fecha", calendario.Mensaje);
    }

    // ---------- H3: tomar asistencia ----------

    [Fact]
    public async Task H3_LaPrimeraVezSeGuardaSinPreguntarYElMensajeNoEsDeError()
    {
        Alumno("101", "Gómez");
        Alumno("102", "Paz");
        AsistenciaViewModel planilla = NuevaPlanilla();
        planilla.Filas.First(f => f.Estudiante.Legajo == "101").Presente = true;

        await planilla.GuardarCommand.ExecuteAsync(null);

        Assert.Equal(0, _dialogos.Preguntas);
        Assert.False(planilla.EsError);
        Assert.Equal(EstadoAsistencia.Presente,
            _e.Registro.AsistenciaDe("101", DateTime.Today));
        Assert.Equal(EstadoAsistencia.Ausente,
            _e.Registro.AsistenciaDe("102", DateTime.Today));
    }

    [Fact]
    public async Task H3_NoSePuedeTomarAsistenciaEnUnaFechaFutura()
    {
        Alumno("101", "Gómez");
        AsistenciaViewModel planilla = NuevaPlanilla();
        planilla.Fecha = DateTime.Today.AddDays(1);

        await planilla.GuardarCommand.ExecuteAsync(null);

        Assert.True(planilla.EsError);
        Assert.Equal("La fecha de la asistencia no puede ser futura", planilla.Mensaje);
        Assert.Empty(_e.Almacen.Asistencias);
    }

    [Fact]
    public void H3_GuardarDosVecesLaMismaFecha_NoDuplicaRegistros()
    {
        Estudiante ana = Alumno("101", "Gómez");
        var marcas = new[] { new MarcaAsistencia(ana, true) };

        _e.Registro.GuardarAsistencia(DateTime.Today, marcas);
        ResultadoGuardado segundo = _e.Registro.GuardarAsistencia(DateTime.Today, marcas);

        Assert.Single(_e.Almacen.Asistencias);
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
            _e.Registro.AsistenciaDe("101", DateTime.Today));
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
            _e.Registro.AsistenciaDe("101", DateTime.Today));
        Assert.Single(_e.Almacen.Asistencias);
    }

    // ---------- H5: entregas ----------

    [Fact]
    public async Task H5_GuardaElEstadoDeCadaEntrega()
    {
        Alumno("101", "Gómez");
        Alumno("102", "Paz");
        Trabajo tp1 = CargarTrabajo("TP1");
        var planilla = new EntregaViewModel(
            ComisionA, _navegacion, _e.Nomina, _e.ServicioCalendario, _dialogos, _e.Registro);
        planilla.Filas.First(f => f.Estudiante.Legajo == "101").EstadoTexto = "Entregado";
        planilla.Filas.First(f => f.Estudiante.Legajo == "102").EstadoTexto = "Fuera de término";

        await planilla.GuardarCommand.ExecuteAsync(null);

        Assert.Equal(EstadoEntrega.Entregado, _e.Registro.EntregaDe("101", tp1.Id));
        Assert.Equal(EstadoEntrega.EntregadoFueraDeTermino, _e.Registro.EntregaDe("102", tp1.Id));
    }

    [Fact]
    public void H5_CerrarLaEntrega_MarcaNoEntregadoSoloALosPendientes()
    {
        Estudiante ana = Alumno("101", "Gómez");
        Estudiante luis = Alumno("102", "Paz");
        Trabajo tp1 = CargarTrabajo("TP1");
        _e.Registro.GuardarEntregas(tp1, new[] { new MarcaEntrega(ana, EstadoEntrega.Entregado) });

        int cerrados = _e.Registro.CerrarEntrega(tp1, new[] { ana, luis });

        Assert.Equal(1, cerrados);
        Assert.Equal(EstadoEntrega.Entregado, _e.Registro.EntregaDe("101", tp1.Id));
        Assert.Equal(EstadoEntrega.NoEntregado, _e.Registro.EntregaDe("102", tp1.Id));
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

        Assert.Equal(25m, _e.Riesgo.CalcularPorcentajeAusencias(ana));
    }

    [Fact]
    public void H6_ElPorcentajeSeCalculaSobreLasClasesDictadasNoSobreLosRegistrosDelAlumno()
    {
        Estudiante ana = Alumno("101", "Gómez");
        Estudiante luis = Alumno("102", "Paz");
        for (int i = 0; i < 10; i++)
        {
            RegistrarAsistencia(luis, EstadoAsistencia.Presente, i);
        }
        RegistrarAsistencia(ana, EstadoAsistencia.Ausente, 0);

        // Ana faltó a una de las 10 clases dictadas, no a "todas las suyas"
        Assert.Equal(10m, _e.Riesgo.CalcularPorcentajeAusencias(ana));
    }

    [Fact]
    public void H6_UnaFaltaYFaltarATodoSinEntregarNoEmpatan()
    {
        Estudiante ana = Alumno("101", "Gómez");
        Estudiante luis = Alumno("102", "Paz");
        Trabajo tp1 = CargarTrabajo("TP1");
        for (int i = 0; i < 10; i++)
        {
            RegistrarAsistencia(luis, EstadoAsistencia.Ausente, i);
        }
        RegistrarAsistencia(ana, EstadoAsistencia.Ausente, 0);
        _e.Entregas.Agregar(new Entrega
        {
            Estudiante = luis,
            Trabajo = tp1,
            Estado = EstadoEntrega.NoEntregado
        });

        Assert.Equal(10m, _e.Riesgo.CalcularRiesgo(ana));
        Assert.Equal(100m, _e.Riesgo.CalcularRiesgo(luis));
    }

    [Fact]
    public void H6_AsistenciaYEntregasSeCombinanConLosPesosDeLaComision()
    {
        Estudiante ana = Alumno("101", "Gómez");
        RegistrarAsistencia(ana, EstadoAsistencia.Presente, 1);
        RegistrarAsistencia(ana, EstadoAsistencia.Ausente, 0);
        Trabajo tp1 = CargarTrabajo("TP1");
        _e.Entregas.Agregar(new Entrega
        {
            Estudiante = ana,
            Trabajo = tp1,
            Estado = EstadoEntrega.NoEntregado
        });

        // 50 % de ausencias y 100 % de riesgo en la entrega, 50/50
        Assert.Equal(75m, _e.Riesgo.CalcularRiesgo(ana));
    }

    [Fact]
    public void H6_SinNingunRegistro_NoHayDatosAunqueLaComisionYaTengaClases()
    {
        Estudiante ana = Alumno("101", "Gómez");
        Estudiante luis = Alumno("102", "Paz");
        RegistrarAsistencia(luis, EstadoAsistencia.Presente);

        ResultadoRiesgo sinCargar = _e.Riesgo.ObtenerResultado(ana);
        ResultadoRiesgo vinoATodo = _e.Riesgo.ObtenerResultado(luis);

        Assert.Equal(NivelesRiesgo.SinDatos, sinCargar.NivelRiesgo);
        Assert.Equal(NivelesRiesgo.Bajo, vinoATodo.NivelRiesgo);
    }

    [Fact]
    public void H6_LosLimitesDeLosNivelesCuentanHaciaArriba()
    {
        Assert.Equal(NivelesRiesgo.Alto, _e.Riesgo.ObtenerNivel(ComisionA, 60m, 1));
        Assert.Equal(NivelesRiesgo.Medio, _e.Riesgo.ObtenerNivel(ComisionA, 59.99m, 1));
        Assert.Equal(NivelesRiesgo.Medio, _e.Riesgo.ObtenerNivel(ComisionA, 30m, 1));
        Assert.Equal(NivelesRiesgo.Bajo, _e.Riesgo.ObtenerNivel(ComisionA, 29.99m, 1));
    }

    // ---------- H7: configuración por comisión ----------

    [Fact]
    public void H7_CadaComisionTieneSusPropiosUmbrales()
    {
        _e.Umbrales.Guardar(ComisionB, new UmbralesRiesgo
        {
            RiesgoMedio = 40,
            RiesgoAlto = 80
        });

        Assert.Equal(NivelesRiesgo.Alto, _e.Riesgo.ObtenerNivel(ComisionA, 70m, 1));
        Assert.Equal(NivelesRiesgo.Medio, _e.Riesgo.ObtenerNivel(ComisionB, 70m, 1));
    }

    [Fact]
    public void H7_ValoresInvalidos_NoSeGuardanYSeExplicaPorQue()
    {
        var configuracion = new ConfigRiesgoViewModel(ComisionA, _e.Umbrales)
        {
            RiesgoMedio = "70",
            RiesgoAlto = "50"
        };

        configuracion.GuardarCommand.Execute(null);

        Assert.True(configuracion.EsError);
        Assert.Contains("no puede ser mayor", configuracion.Mensaje);
        Assert.Equal(60m, _e.Umbrales.Obtener(ComisionA).RiesgoAlto);
    }

    // ---------- H8: lista de alumnos ----------

    [Fact]
    public void H8_ListaPorApellido_AZyZA()
    {
        Alumno("1", "Paz", "Luis");
        Alumno("2", "Alvarez", "Eva");
        Alumno("3", "Gómez", "Ana");
        ListaAlumnosViewModel lista = NuevaLista();

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
        ListaAlumnosViewModel lista = NuevaLista();
        lista.OrdenIndice = (int)OrdenLista.MayorRiesgo;

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

        var ficha = new FichaAlumnoViewModel(
            "101", _navegacion, _dialogos, _e.Riesgo, _e.Nomina);

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
        var ficha = new FichaAlumnoViewModel(
            "101", _navegacion, _dialogos, _e.Riesgo, _e.Nomina);
        _dialogos.Respuesta = false;

        await ficha.EliminarCommand.ExecuteAsync(null);
        Assert.Single(_e.Almacen.Estudiantes);

        _dialogos.Respuesta = true;
        await ficha.EliminarCommand.ExecuteAsync(null);

        Assert.Empty(_e.Almacen.Estudiantes);
        Assert.Empty(_e.Almacen.Asistencias);
    }

    // ---------- H10: contactos ----------

    private ContactoViewModel NuevoContacto(string legajo = "101")
    {
        return new ContactoViewModel(
            legajo, _navegacion, _e.Nomina, _e.ServicioContactos);
    }

    [Fact]
    public async Task H10_ContactoQueNoAtendio_QuedaRegistradoConSuTexto()
    {
        Alumno("101", "Gómez");
        ContactoViewModel contacto = NuevoContacto();
        contacto.Atendio = false;
        contacto.Resultado = "Sonó y no contestó";

        await contacto.GuardarCommand.ExecuteAsync(null);

        ContactoEstudiante guardado = Assert.Single(_e.Almacen.Contactos);
        Assert.False(guardado.Atendio);
        Assert.Equal(ContactoEstudiante.TextoNoAtendio, guardado.EstadoTexto);
        Assert.Equal(1, _navegacion.Volvio);
    }

    [Fact]
    public async Task H10_ContactoSinResultadoOConFechaFutura_SeRechaza()
    {
        Alumno("101", "Gómez");
        ContactoViewModel contacto = NuevoContacto();

        await contacto.GuardarCommand.ExecuteAsync(null);
        string sinResultado = contacto.Mensaje;

        contacto.Resultado = "Llamada";
        contacto.Fecha = DateTime.Today.AddDays(2);
        await contacto.GuardarCommand.ExecuteAsync(null);

        Assert.NotEqual("", sinResultado);
        Assert.True(contacto.EsError);
        Assert.Equal("La fecha del contacto no puede ser futura", contacto.Mensaje);
        Assert.Empty(_e.Almacen.Contactos);
    }

    // ---------- H11: filtros de la lista ----------

    private List<ResultadoRiesgo> Filtrar(FiltroContacto filtro)
    {
        return ListadoAlumnos.Aplicar(
            _e.Riesgo.ObtenerResultados(ComisionA),
            OrdenLista.ApellidoAZ,
            filtro,
            FiltroNivel.Todos,
            DateTime.Today);
    }

    [Fact]
    public void H11_SinContactar_MuestraSoloALosQueNuncaSeContactaron()
    {
        Estudiante ana = Alumno("1", "Gómez");
        Alumno("2", "Paz", "Luis");
        RegistrarContacto(ana, 1);

        List<ResultadoRiesgo> filtrados = Filtrar(FiltroContacto.SinContactar);

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

        List<ResultadoRiesgo> filtrados = Filtrar(FiltroContacto.SinContactoReciente);

        Assert.Equal(new[] { "2", "3" }, filtrados.Select(r => r.Estudiante.Legajo));
    }

    [Fact]
    public void H11_ParaLlamar_SonLosDeRiesgoAltoSinContactoReciente()
    {
        Estudiante sinLlamar = Alumno("1", "Gómez");
        Estudiante yaLlamado = Alumno("2", "Paz", "Luis");
        Estudiante bien = Alumno("3", "Rey", "Eva");
        RegistrarAsistencia(sinLlamar, EstadoAsistencia.Ausente);
        RegistrarAsistencia(yaLlamado, EstadoAsistencia.Ausente);
        RegistrarAsistencia(bien, EstadoAsistencia.Presente);
        RegistrarContacto(yaLlamado, 1);

        List<ResultadoRiesgo> filtrados = Filtrar(FiltroContacto.ParaLlamar);

        Assert.Equal("1", Assert.Single(filtrados).Estudiante.Legajo);
    }

    // ---------- Comisiones ----------

    [Fact]
    public void Comisiones_NoSePuedeRepetirElNombre()
    {
        Assert.Null(_e.Comisiones.Agregar("Comisión C"));

        string? error = _e.Comisiones.Agregar("comisión c");

        Assert.Equal("Ya existe una comisión con ese nombre", error);
    }

    [Fact]
    public void Comisiones_EliminarUnaBorraSusAlumnosYRegistros()
    {
        Estudiante ana = Alumno("1", "Gómez", "Ana", ComisionB);
        Alumno("2", "Paz", "Luis", ComisionA);
        RegistrarAsistencia(ana, EstadoAsistencia.Presente);

        _e.Comisiones.Eliminar(ComisionB);

        Assert.DoesNotContain(ComisionB, _e.Comisiones.Todas());
        Assert.Equal("2", Assert.Single(_e.Almacen.Estudiantes).Legajo);
        Assert.Empty(_e.Almacen.Asistencias);
    }

    [Fact]
    public void Comisiones_RenombrarActualizaLosAlumnos()
    {
        Alumno("1", "Gómez");

        Assert.Null(_e.Comisiones.Renombrar(ComisionA, "Comisión Noche"));

        Assert.Equal("Comisión Noche", _e.Almacen.Estudiantes[0].Comision);
        Assert.Contains("Comisión Noche", _e.Comisiones.Todas());
    }

    [Fact]
    public async Task Administrar_BorrarTodo_DejaLasComisionesIniciales()
    {
        _e.Comisiones.Agregar("Comisión C");
        Alumno("1", "Gómez");
        var administrar = new AdministrarViewModel(_dialogos, _e.Comisiones);

        await administrar.BorrarTodoCommand.ExecuteAsync(null);

        Assert.Empty(_e.Almacen.Estudiantes);
        Assert.Equal(
            ComisionService.Iniciales.OrderBy(c => c),
            _e.Comisiones.Todas());
    }

    // ---------- Menú ----------

    [Fact]
    public async Task Menu_AbreLaPantallaDeLaComisionElegida()
    {
        _e.Comisiones.Inicializar();
        var menu = new MenuViewModel(_navegacion, _e.Comisiones)
        {
            ComisionSeleccionada = ComisionB
        };

        await menu.AsistenciaCommand.ExecuteAsync(null);

        Assert.Equal(1, _navegacion.VolvioAlInicio);
        Assert.Equal((Destino.Asistencia, ComisionB), _navegacion.Visitas[^1]);
    }
}
