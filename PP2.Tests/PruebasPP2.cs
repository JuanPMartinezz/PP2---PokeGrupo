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

        // Contenido del archivo que "elige" el usuario; null si cancela
        public string? ArchivoElegido { get; set; }

        public List<string> Avisos { get; } = new();

        public Task<string?> ElegirArchivoDeTextoAsync(string titulo)
        {
            return Task.FromResult(ArchivoElegido);
        }

        public Task AvisarAsync(string titulo, string mensaje)
        {
            Avisos.Add(mensaje);
            return Task.CompletedTask;
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
        return new AsistenciaViewModel(
            ComisionA, _e.Nomina, _dialogos, _e.Registro, _e.Riesgo);
    }

    private ListaAlumnosViewModel NuevaLista(string comision = ComisionA)
    {
        return new ListaAlumnosViewModel(
            comision,
            _navegacion,
            _e.Riesgo,
            _e.Comisiones,
            _e.Importacion,
            _dialogos);
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
    public void H2_TrabajoConElMismoNombre_SeRechazaAunqueCambieLaFecha()
    {
        var calendario = new CalendarioViewModel(
            ComisionA, _e.ServicioCalendario, _e.Registro)
        {
            Descripcion = "TP1"
        };
        calendario.AgregarCommand.Execute(null);
        calendario.Descripcion = "TP1";
        calendario.Fecha = DateTime.Today.AddDays(7);

        calendario.AgregarCommand.Execute(null);

        Assert.Single(_e.Almacen.Calendario);
        Assert.True(calendario.EsError);
        Assert.Contains("ese nombre", calendario.Mensaje);
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
    public async Task H5_NoSeCierraLaEntregaDeUnTrabajoQueTodaviaNoVencio()
    {
        Alumno("101", "Gómez");
        CargarTrabajo("TP1");
        var planilla = new EntregaViewModel(
            ComisionA, _navegacion, _e.Nomina, _e.ServicioCalendario, _dialogos, _e.Registro);

        await planilla.CerrarEntregaCommand.ExecuteAsync(null);

        Assert.True(planilla.EsError);
        Assert.Contains("vence", planilla.Mensaje);
        Assert.Equal(0, _dialogos.Preguntas);
        Assert.Empty(_e.Almacen.Entregas);
    }

    // ---------- H6: riesgo ----------

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
    public void H6_ElAlumnoIncorporadoTarde_SoloCuentaLasClasesDesdeQueSeIncorporo()
    {
        Estudiante luis = Alumno("102", "Paz");
        for (int i = 0; i < 10; i++)
        {
            RegistrarAsistencia(luis, EstadoAsistencia.Presente, i);
        }

        Estudiante ana = Alumno("101", "Gómez");
        ana.FechaIncorporacion = DateTime.Today.AddDays(-1);
        RegistrarAsistencia(ana, EstadoAsistencia.Ausente, 0);
        RegistrarAsistencia(ana, EstadoAsistencia.Presente, 1);
        // Una marca anterior a su incorporación no le corresponde
        RegistrarAsistencia(ana, EstadoAsistencia.Ausente, 5);

        // Faltó a 1 de las 2 clases dictadas desde que se incorporó
        Assert.Equal(50m, _e.Riesgo.CalcularPorcentajeAusencias(ana));
        Assert.Equal(2, _e.Riesgo.ObtenerResultado(ana).TotalClases);
    }

    [Fact]
    public void H6_UnTrabajoNoEntregadoEntreCuatro_SonVeinticincoPorCiento()
    {
        Estudiante ana = Alumno("101", "Gómez");
        Estudiante luis = Alumno("102", "Paz");
        foreach (string nombre in new[] { "TP1", "TP2", "TP3", "TP4" })
        {
            _e.Entregas.Agregar(new Entrega
            {
                Estudiante = luis,
                Trabajo = CargarTrabajo(nombre),
                Estado = EstadoEntrega.Entregado
            });
        }
        _e.Entregas.Agregar(new Entrega
        {
            Estudiante = ana,
            Trabajo = _e.Entregas.ObtenerOCrearTrabajo("TP1"),
            Estado = EstadoEntrega.NoEntregado
        });

        Assert.Equal(25m, _e.Riesgo.CalcularPorcentajeEntregas(ana));
        Assert.Equal(0m, _e.Riesgo.CalcularPorcentajeEntregas(luis));
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

    [Fact]
    public async Task H10_ElContactoGuardaElMedioElegido()
    {
        Alumno("101", "Gómez");
        ContactoViewModel contacto = NuevoContacto();
        contacto.MedioIndice = (int)MedioContacto.Correo;
        contacto.Resultado = "Se le escribió por el campus";

        await contacto.GuardarCommand.ExecuteAsync(null);

        Assert.Equal(
            MedioContacto.Correo, Assert.Single(_e.Almacen.Contactos).Medio);
    }

    [Fact]
    public async Task H10_SinMedioElegido_SeRechaza()
    {
        Alumno("101", "Gómez");
        ContactoViewModel contacto = NuevoContacto();
        contacto.MedioIndice = -1;
        contacto.Resultado = "Llamada";

        await contacto.GuardarCommand.ExecuteAsync(null);

        Assert.True(contacto.EsError);
        Assert.Equal("Elegí el medio del contacto", contacto.Mensaje);
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


    //Pruebas de aceptación de la H8, CA1 y CA2.
    //H8: Como tutor de una comisión, quiero visualizar a mis estudiantes ordenados según su nivel de riesgo para poder priorizar el seguimiento de aquellos que presentan mayor riesgo.
    //C1: Dada una comisión con estudiantes que tienen un riesgo calculado, cuando el tutor consulta la comisión, el sistema debe mostrar a los estudiantes ordenados de mayor a menor riesgo.
    //C2: Dado un estudiante con un riesgo calculado, cuando el tutor consulta su información, el sistema debe mostrar el detalle de los factores que justifican ese valor.


    [Fact]
    public void Given_ComisionConAlumnosDeDistintoRiesgo_When_TutorConsultaLaComision_Then_SeMuestranAlumnosDeMayorAMenorRiesgo()
    {

        // GIVEN: Preparación explícita de adaptadores, servicios y datos
        
        // 1. Datos
        var almacen = new AlmacenEnMemoria();
        var repoComisiones = new ComisionRepositorioEnMemoria(almacen);
        var repoEstudiantes = new EstudianteRepositorioEnMemoria(almacen);
        var repoAsistencias = new AsistenciaRepositorioEnMemoria(almacen);
        var repoEntregas = new EntregaRepositorioEnMemoria(almacen);
        var repoContactos = new ContactoRepositorioEnMemoria(almacen);
        var repoAdministracion = new AdministracionRepositorioEnMemoria(almacen);

        // 2. Servicios 
        var comisionService = new ComisionService(repoComisiones, repoEstudiantes, repoAdministracion);
        var nominaService = new NominaService(repoEstudiantes, repoComisiones);
        var registroService = new RegistroService(repoAsistencias, repoEntregas);
        var umbralesService = new UmbralesService(repoComisiones);
        var riesgoService = new RiesgoService(
            repoEstudiantes, repoAsistencias, repoEntregas, repoContactos, umbralesService);

        // La lista también ofrece importar alumnos desde un CSV
        var repoCalendario = new CalendarioRepositorioEnMemoria(almacen);
        var calendarioService = new CalendarioService(repoCalendario, repoEntregas);
        var importacionService = new ImportacionService(
            nominaService, registroService, calendarioService,
            new UnidadDeTrabajoEnMemoria());

        var navegacion = new NavegacionFalsa(); // simula el cambio de pantallas en memoria

        const string comision = "Comisión A";
        comisionService.Inicializar();

        nominaService.Agregar("Ramiro", "Álvarez", "101", comision);
        nominaService.Agregar("Roberto", "Gómez", "102", comision);
        nominaService.Agregar("Juan", "Pérez", "103", comision);

        Estudiante alvarez = nominaService.Buscar("101")!;
        Estudiante gomez = nominaService.Buscar("102")!;
        Estudiante perez = nominaService.Buscar("103")!;

        DateTime clase1 = DateTime.Today.AddDays(-7);
        DateTime clase2 = DateTime.Today;

        registroService.GuardarAsistencia(clase1, new[]
        {
            new MarcaAsistencia(alvarez, Presente: false),
            new MarcaAsistencia(gomez, Presente: true),
            new MarcaAsistencia(perez, Presente: true)
        });

        registroService.GuardarAsistencia(clase2, new[]
        {
            new MarcaAsistencia(alvarez, Presente: false),
            new MarcaAsistencia(gomez, Presente: false),
            new MarcaAsistencia(perez, Presente: true)
        });

        // WHEN : El tutor consulta la comisión y ordena por mayor riesgo
        var listaAlumnosVm = new ListaAlumnosViewModel(
            comision, navegacion, riesgoService, comisionService,
            importacionService, new DialogosFalsos());

        listaAlumnosVm.OrdenIndice = (int)OrdenLista.MayorRiesgo;

        // THEN: La lista expuesta ordena de mayor a menor riesgo
        Assert.Equal(
            new[] { "Álvarez, Ramiro", "Gómez, Roberto", "Pérez, Juan" },
            listaAlumnosVm.Alumnos.Select(a => a.Nombre)
        );
    }

    [Fact]
    public void Given_AlumnoConRiesgoCalculado_When_TutorConsultaSuInformacion_Then_SeMuestranLosFactoresQueJustificanElRiesgo()
    {
        // GIVEN : Un alumno con el riesgo calculado 

        // Preparación explícita de adaptadores, servicios y registros

        //Datos
        var almacen = new AlmacenEnMemoria();
        var repoComisiones = new ComisionRepositorioEnMemoria(almacen);
        var repoEstudiantes = new EstudianteRepositorioEnMemoria(almacen);
        var repoAsistencias = new AsistenciaRepositorioEnMemoria(almacen);
        var repoEntregas = new EntregaRepositorioEnMemoria(almacen);
        var repoContactos = new ContactoRepositorioEnMemoria(almacen);
        var repoAdministracion = new AdministracionRepositorioEnMemoria(almacen);

        // 2. Servicios
        var comisionService = new ComisionService(repoComisiones, repoEstudiantes, repoAdministracion);
        var nominaService = new NominaService(repoEstudiantes, repoComisiones);
        var registroService = new RegistroService(repoAsistencias, repoEntregas);
        var umbralesService = new UmbralesService(repoComisiones);
        var riesgoService = new RiesgoService(
            repoEstudiantes, repoAsistencias, repoEntregas, repoContactos, umbralesService);

        // 3. Reemplazos del viewmodel
        var navegacion = new NavegacionFalsa();
        var dialogos = new DialogosFalsos();

        const string comision = "Comisión A";
        comisionService.Inicializar();
        nominaService.Agregar("Ana", "Gómez", "101", comision);
        Estudiante ana = nominaService.Buscar("101")!;

        registroService.GuardarAsistencia(
            DateTime.Today.AddDays(-7),
            new[] { new MarcaAsistencia(ana, Presente: true) });

        registroService.GuardarAsistencia(
            DateTime.Today,
            new[] { new MarcaAsistencia(ana, Presente: false) });

        Trabajo tp1 = repoEntregas.ObtenerOCrearTrabajo("TP1");
        Trabajo tp2 = repoEntregas.ObtenerOCrearTrabajo("TP2");

        registroService.GuardarEntregas(tp1, new[] { new MarcaEntrega(ana, EstadoEntrega.EntregadoFueraDeTermino) });

        registroService.GuardarEntregas(tp2, new[] { new MarcaEntrega(ana, EstadoEntrega.NoEntregado) });

        ResultadoRiesgo resultadoRiesgo = riesgoService.ObtenerResultado(ana);

        // WHEN (Cuando): El tutor abre la ficha individual del estudiante

        var fichaAlumnoVm = new FichaAlumnoViewModel( ana.Legajo, navegacion, dialogos, riesgoService, nominaService);

        // THEN (Entonces): Se verifica cada factor observable que justifica el riesgo

        Assert.Equal(resultadoRiesgo.NivelRiesgo, fichaAlumnoVm.Nivel);

        Assert.Equal(
            "Asistencia: 1 presentes y 1 ausencias en 2 clases dictadas",
            fichaAlumnoVm.AsistenciaTexto);

        Assert.Equal(
            "Entregas: 0 entregadas, 1 fuera de término y 1 sin entregar, de 2 trabajos",
            fichaAlumnoVm.EntregasTexto);

        
        Assert.False(string.IsNullOrWhiteSpace(fichaAlumnoVm.RiesgoTexto));
    }

    // ---------- H12: reporte de riesgo de la comisión ----------

    // Cuatro alumnos, uno en cada nivel: alto, medio, bajo y sin datos
    private void CargarComisionConUnAlumnoPorNivel()
    {
        Estudiante alto = Alumno("1", "Alto");
        Estudiante medio = Alumno("2", "Medio");
        Estudiante bajo = Alumno("3", "Bajo");
        Alumno("4", "Nuevo");
        Trabajo tp1 = CargarTrabajo("TP1");

        RegistrarAsistencia(alto, EstadoAsistencia.Ausente, 2);
        RegistrarAsistencia(alto, EstadoAsistencia.Ausente, 1);
        RegistrarAsistencia(medio, EstadoAsistencia.Ausente, 2);
        RegistrarAsistencia(medio, EstadoAsistencia.Presente, 1);
        RegistrarAsistencia(bajo, EstadoAsistencia.Presente, 2);
        RegistrarAsistencia(bajo, EstadoAsistencia.Presente, 1);

        RegistrarEntrega(alto, tp1, EstadoEntrega.NoEntregado);
        RegistrarEntrega(medio, tp1, EstadoEntrega.EntregadoFueraDeTermino);
        RegistrarEntrega(bajo, tp1, EstadoEntrega.Entregado);
    }

    private void RegistrarEntrega(
        Estudiante estudiante, Trabajo trabajo, EstadoEntrega estado)
    {
        _e.Entregas.Agregar(new Entrega
        {
            Estudiante = estudiante,
            Trabajo = trabajo,
            Estado = estado
        });
    }

    [Fact]
    public void H12_ElReporteCuentaCadaNivelYElPorcentajeEnRiesgo()
    {
        CargarComisionConUnAlumnoPorNivel();

        var reporte = new ReporteRiesgoViewModel(ComisionA, _e.Riesgo);

        Assert.Equal(4, reporte.TotalEstudiantes);
        Assert.Equal(1, reporte.EstudiantesEnRiesgo);
        Assert.Equal(1, reporte.EstudiantesRiesgoMedio);
        Assert.Equal(1, reporte.EstudiantesRiesgoBajo);
        Assert.Equal(25m, reporte.PorcentajeEnRiesgo);
        Assert.Equal("1 de 4 estudiantes tienen riesgo alto.", reporte.Resumen);
    }

    [Fact]
    public void H12_ElReporteCoincideConLoQueMuestraElPanelDeLaComision()
    {
        CargarComisionConUnAlumnoPorNivel();

        var reporte = new ReporteRiesgoViewModel(ComisionA, _e.Riesgo);
        ListaAlumnosViewModel lista = NuevaLista();

        Assert.Equal(
            lista.Alumnos.Count(a => a.Nivel == NivelesRiesgo.Alto),
            reporte.EstudiantesEnRiesgo);
        Assert.Equal(lista.Alumnos.Count, reporte.TotalEstudiantes);
    }

    [Fact]
    public void H12_ComisionSinEstudiantes_NoDivideYAvisa()
    {
        var reporte = new ReporteRiesgoViewModel(ComisionB, _e.Riesgo);

        Assert.True(reporte.SinEstudiantes);
        Assert.Equal(0m, reporte.PorcentajeEnRiesgo);
        Assert.Equal("Esta comisión todavía no tiene estudiantes.", reporte.Resumen);
    }

    [Fact]
    public void H12_ComisionSinDatos_NoAfirmaQueNadieEstaEnRiesgo()
    {
        Alumno("1", "Gómez");
        Alumno("2", "Paz");

        var reporte = new ReporteRiesgoViewModel(ComisionA, _e.Riesgo);

        Assert.True(reporte.SinDatos);
        Assert.Equal(0, reporte.EstudiantesEnRiesgo);
        Assert.Equal(
            "Todavía no hay datos suficientes para calcular el riesgo.",
            reporte.Resumen);
    }

    // ---------- Carga masiva desde CSV ----------

    private const string Encabezado =
        "Nombre;Apellido;Legajo;Comision;Asistencias;Entregas\n";

    [Fact]
    public void Importar_FilasInvalidas_SeInformanConSuLineaYSeSigueConLasValidas()
    {
        string futura = DateTime.Today.AddDays(3).ToString("yyyy-MM-dd");
        string csv = Encabezado +
            "Ana;Gómez;ab;Comisión A;;\n" +
            "Luis;Paz;102;Comisión Z;;\n" +
            "Eva;Ríos;103;Comisión A;;\n" +
            "Sol;Vera;103;Comisión A;;\n" +
            $"Tito;Gil;104;Comisión A;{futura}:Presente;";

        ResultadoImportacion r = _e.Importacion.Importar(csv);

        // Entran 103 y 104; el resto se informa con su número de línea
        Assert.Equal(2, r.AlumnosNuevos);
        Assert.Equal(0, r.Registros);
        Assert.Equal(4, r.Errores.Count);
        Assert.Contains("Línea 2", r.Errores[0]);
        Assert.Contains("Línea 3", r.Errores[1]);
        Assert.Contains("Línea 5", r.Errores[2]);
        Assert.Contains("Línea 6", r.Errores[3]);
    }

    [Fact]
    public async Task Given_ArchivoConAlumnos_When_TutorImporta_Then_LaListaLosMuestraYSeAvisaElResultado()
    {
        // Given: un archivo elegido por el tutor
        _dialogos.ArchivoElegido = Encabezado +
            "Ana;Gómez;101;Comisión A;2020-03-02:Presente;\n" +
            "Luis;Paz;102;Comisión A;;";
        ListaAlumnosViewModel lista = NuevaLista();

        // When: importa los alumnos
        await lista.ImportarAlumnosCommand.ExecuteAsync(null);

        // Then: la lista los muestra y se informa cuántos entraron
        Assert.Equal(2, lista.Alumnos.Count);
        Assert.Contains("Alumnos nuevos: 2", _dialogos.Avisos.Single());
    }

}
