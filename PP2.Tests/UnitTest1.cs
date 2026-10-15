using RegistroAsistencia.Models;
using RegistroAsistencia.Services;
using RegistroAsistencia.ViewModels;

namespace PP2.Tests;

public class UnitTest1
{
    // Prueba H6
    [Fact]
    public void CalcularRiesgo_SinFactoresDeRiesgo_DeberiaRetornarCero()
    {
        // Arrange
        DatosSistema.Asistencias.Clear();
        DatosSistema.Entregas.Clear();

        var estudiante = new Estudiante
        {
            Nombre = "Agustina",
            Apellido = "Gómez",
            Legajo = "101",
            Comision = "Comisión A"
        };

        var trabajo = new Trabajo
        {
            Id = 3,
            Nombre = "TP1"
        };

        // Todas las asistencias son presentes
        DatosSistema.Asistencias.Add(new Asistencia
        {
            Estudiante = estudiante,
            Clase = new Clase
            {
                Fecha = DateTime.Today
            },
            Estado = EstadoAsistencia.Presente
        });

        DatosSistema.Asistencias.Add(new Asistencia
        {
            Estudiante = estudiante,
            Clase = new Clase
            {
                Fecha = DateTime.Today.AddDays(1)
            },
            Estado = EstadoAsistencia.Presente
        });

        // Trabajo entregado correctamente
        DatosSistema.Entregas.Add(new Entrega
        {
            Estudiante = estudiante,
            Trabajo = trabajo,
            Estado = EstadoEntrega.Entregado
        });

        var riesgoService = new RiesgoService();

        // Act
        decimal resultado =
            riesgoService.CalcularRiesgo(estudiante);

        // Assert
        Assert.Equal(0m, resultado);
    }
    // Prueba H6
    [Fact]
    public void CalcularRiesgo_DeberiaRetornar50Porciento()
    {
        // Arrange
        DatosSistema.Asistencias.Clear();
        DatosSistema.Entregas.Clear();

        var estudiante = new Estudiante
        {
            Nombre = "Juan",
            Apellido = "Martinez",
            Legajo = "100",
            Comision = "Comisión A"
        };

        var trabajo1 = new Trabajo
        {
            Id = 1,
            Nombre = "TP1"
        };

        var trabajo2 = new Trabajo
        {
            Id = 2,
            Nombre = "TP2"
        };

        // Asistencia: 1 presente + 1 ausente = 50 %
        DatosSistema.Asistencias.Add(new Asistencia
        {
            Estudiante = estudiante,
            Clase = new Clase
            {
                Fecha = DateTime.Today
            },
            Estado = EstadoAsistencia.Presente
        });

        DatosSistema.Asistencias.Add(new Asistencia
        {
            Estudiante = estudiante,
            Clase = new Clase
            {
                Fecha = DateTime.Today.AddDays(1)
            },
            Estado = EstadoAsistencia.Ausente
        });

        // Entregas: 1 entregado + 1 no entregado = 50 %
        DatosSistema.Entregas.Add(new Entrega
        {
            Estudiante = estudiante,
            Trabajo = trabajo1,
            Estado = EstadoEntrega.Entregado
        });

        DatosSistema.Entregas.Add(new Entrega
        {
            Estudiante = estudiante,
            Trabajo = trabajo2,
            Estado = EstadoEntrega.NoEntregado
        });

        var riesgoService = new RiesgoService();

        // Act
        decimal resultado =
            riesgoService.CalcularRiesgo(estudiante);

        // Assert
        Assert.Equal(50m, resultado);
    }
    //Prueba H1
    [Fact]
    public void AgregarAlumno_ConDatosValidos_DeberiaAgregarlo()
    {
        // Arrange
        DatosSistema.Estudiantes.Clear();

        var viewModel = new ComisionViewModel
        {
            Nombre = "Agustina",
            Apellido = "Gómez",
            Legajo = "200"
        };

        // Act
        viewModel.AgregarAlumnoCommand.Execute(null);

        // Assert
        Assert.Single(DatosSistema.Estudiantes);
        Assert.Equal("200", DatosSistema.Estudiantes[0].Legajo);
        Assert.Equal("Alumno agregado a Comisión A", viewModel.Mensaje);
    }
    //Prueba H1
    [Fact]
    public void AgregarAlumno_ConLegajoDuplicado_NoDeberiaAgregarlo()
    {
        // Arrange
        DatosSistema.Estudiantes.Clear();

        var viewModel = new ComisionViewModel
        {
            Nombre = "Agustina",
            Apellido = "Gómez",
            Legajo = "200"
        };

        viewModel.AgregarAlumnoCommand.Execute(null);

        viewModel.Nombre = "Juan";
        viewModel.Apellido = "Martinez";
        viewModel.Legajo = "200";

        // Act
        viewModel.AgregarAlumnoCommand.Execute(null);

        // Assert
        Assert.Single(DatosSistema.Estudiantes);
        Assert.Equal(
            "Ya existe un alumno con ese legajo",
            viewModel.Mensaje);
    }
    //Prueba H1
    [Fact]
    public void AgregarAlumno_DeberiaAsignarloALaComisionSeleccionada()
    {
        // Arrange
        DatosSistema.Estudiantes.Clear();

        var viewModel = new ComisionViewModel
        {
            ComisionSeleccionada = "Comisión B",
            Nombre = "Ana",
            Apellido = "Lopez",
            Legajo = "C001"
        };

        // Act
        viewModel.AgregarAlumnoCommand.Execute(null);

        // Assert
        Assert.Single(viewModel.Estudiantes);

        Assert.Equal(
            "Comisión B",
            viewModel.Estudiantes[0].Comision);

        Assert.Equal(
            "Alumno agregado a Comisión B",
            viewModel.Mensaje);
    }
    //Pruebas H1
    [Fact]
    public void CambiarComision_DeberiaMostrarSoloEstudiantesDeEsaComision()
    {
        // Arrange
        DatosSistema.Estudiantes.Clear();

        DatosSistema.Estudiantes.Add(new Estudiante
        {
            Nombre = "Ana",
            Apellido = "Lopez",
            Legajo = "C002",
            Comision = "Comisión A"
        });

        DatosSistema.Estudiantes.Add(new Estudiante
        {
            Nombre = "Juan",
            Apellido = "Perez",
            Legajo = "C003",
            Comision = "Comisión B"
        });

        var viewModel = new ComisionViewModel();

        // Act
        viewModel.ComisionSeleccionada = "Comisión A";

        // Assert Comisión A
        Assert.Single(viewModel.Estudiantes);

        Assert.Equal(
            "C002",
            viewModel.Estudiantes[0].Legajo);

        Assert.Equal(
            1,
            viewModel.CantidadAlumnos);

        // Act
        viewModel.ComisionSeleccionada = "Comisión B";

        // Assert Comisión B
        Assert.Single(viewModel.Estudiantes);

        Assert.Equal(
            "C003",
            viewModel.Estudiantes[0].Legajo);

        Assert.Equal(
            1,
            viewModel.CantidadAlumnos);
    }
    //Pruebas h3
    [Fact]
    public void ConsultarAsistencias_DeberiaFiltrarPorFechaYComision()
    {
        // Arrange
        DatosSistema.Estudiantes.Clear();
        DatosSistema.Asistencias.Clear();

        var agustina = new Estudiante
        {
            Nombre = "Agustina",
            Apellido = "Gomez",
            Legajo = "H3-001",
            Comision = "Comisión A"
        };

        var juan = new Estudiante
        {
            Nombre = "Juan",
            Apellido = "Perez",
            Legajo = "H3-002",
            Comision = "Comisión B"
        };

        DatosSistema.Estudiantes.Add(agustina);
        DatosSistema.Estudiantes.Add(juan);

        // Comisión A - 14/10
        DatosSistema.Asistencias.Add(new Asistencia
        {
            Estudiante = agustina,
            Clase = new Clase
            {
                Fecha = new DateTime(2026, 10, 14)
            },
            Estado = EstadoAsistencia.Presente
        });

        // Comisión A - otra fecha
        DatosSistema.Asistencias.Add(new Asistencia
        {
            Estudiante = agustina,
            Clase = new Clase
            {
                Fecha = new DateTime(2026, 10, 15)
            },
            Estado = EstadoAsistencia.Ausente
        });

        // Comisión B - misma fecha
        DatosSistema.Asistencias.Add(new Asistencia
        {
            Estudiante = juan,
            Clase = new Clase
            {
                Fecha = new DateTime(2026, 10, 14)
            },
            Estado = EstadoAsistencia.Ausente
        });

        var viewModel = new AsistenciaViewModel
        {
            ComisionSeleccionada = "Comisión A",
            FechaClase = new DateTime(2026, 10, 14)
        };

        // Act
        viewModel.ConsultarAsistenciasCommand.Execute(null);

        // Assert
        Assert.Single(viewModel.Asistencias);

        Assert.Equal(
            "H3-001",
            viewModel.Asistencias[0].Estudiante.Legajo);

        Assert.Equal(
            "Comisión A",
            viewModel.Asistencias[0].Estudiante.Comision);

        Assert.Equal(
            new DateTime(2026, 10, 14),
            viewModel.Asistencias[0].Clase.Fecha.Date);

        Assert.Equal(
            EstadoAsistencia.Presente,
            viewModel.Asistencias[0].Estado);
    }
    //Pruebas H3
    [Fact]
    public void RegistrarPresente_ConAlumnoSeleccionado_DeberiaAgregarAsistencia()
    {
        // Arrange
        DatosSistema.Asistencias.Clear();

        var estudiante = new Estudiante
        {
            Nombre = "Agustina",
            Apellido = "Gómez",
            Legajo = "300"
        };

        var viewModel = new AsistenciaViewModel
        {
            AlumnoSeleccionado = estudiante,
            FechaClase = new DateTime(2026, 10, 7)
        };

        // Act
        viewModel.RegistrarPresenteCommand.Execute(null);

        // Assert
        Assert.Single(DatosSistema.Asistencias);

        Assert.Equal(
            EstadoAsistencia.Presente,
            DatosSistema.Asistencias[0].Estado);

        Assert.Equal(
            estudiante,
            DatosSistema.Asistencias[0].Estudiante);

        Assert.Equal(
            new DateTime(2026, 10, 7),
            DatosSistema.Asistencias[0].Clase.Fecha);

        Assert.Equal(
            "Presente registrado",
            viewModel.Mensaje);
    }
    //Pruebas h3
    [Fact]
    public void RegistrarAsistencia_Duplicada_NoDeberiaAgregarSegundoRegistro()
    {
        // Arrange
        DatosSistema.Asistencias.Clear();

        var estudiante = new Estudiante
        {
            Nombre = "Juan",
            Apellido = "Martinez",
            Legajo = "301"
        };

        var viewModel = new AsistenciaViewModel
        {
            AlumnoSeleccionado = estudiante,
            FechaClase = new DateTime(2026, 10, 7)
        };

        viewModel.RegistrarPresenteCommand.Execute(null);

        // Act
        viewModel.RegistrarAusenteCommand.Execute(null);

        // Assert
        Assert.Single(DatosSistema.Asistencias);

        Assert.Equal(
            EstadoAsistencia.Presente,
            DatosSistema.Asistencias[0].Estado);

        Assert.Equal(
            "La asistencia ya fue registrada para ese alumno y fecha",
            viewModel.Mensaje);
    }
    // Prueba H8
    [Fact]
    public void CargarRiesgos_DeberiaOrdenarMayorRiesgoPrimero()
    {
        // Arrange
        DatosSistema.Estudiantes.Clear();
        DatosSistema.Asistencias.Clear();
        DatosSistema.Entregas.Clear();

        var estudianteRiesgoMedio = new Estudiante
        {
            Nombre = "Juan",
            Apellido = "Martinez",
            Legajo = "400",
            Comision = "Comisión A"
        };

        var estudianteRiesgoAlto = new Estudiante
        {
            Nombre = "Agustina",
            Apellido = "Gómez",
            Legajo = "401",
            Comision = "Comisión A"
        };

        DatosSistema.Estudiantes.Add(estudianteRiesgoMedio);
        DatosSistema.Estudiantes.Add(estudianteRiesgoAlto);

        // Juan:
        // Asistencia = 50 %
        DatosSistema.Asistencias.Add(new Asistencia
        {
            Estudiante = estudianteRiesgoMedio,
            Clase = new Clase
            {
                Fecha = new DateTime(2026, 10, 7)
            },
            Estado = EstadoAsistencia.Presente
        });

        DatosSistema.Asistencias.Add(new Asistencia
        {
            Estudiante = estudianteRiesgoMedio,
            Clase = new Clase
            {
                Fecha = new DateTime(2026, 10, 8)
            },
            Estado = EstadoAsistencia.Ausente
        });

        // Entrega = 50 %
        var trabajoJuan = new Trabajo
        {
            Id = 1,
            Nombre = "TP1"
        };

        DatosSistema.Entregas.Add(new Entrega
        {
            Estudiante = estudianteRiesgoMedio,
            Trabajo = trabajoJuan,
            Estado = EstadoEntrega.EntregadoFueraDeTermino
        });

        // Riesgo final Juan = 50 %

        // Agustina:
        // Asistencia = 100 %
        DatosSistema.Asistencias.Add(new Asistencia
        {
            Estudiante = estudianteRiesgoAlto,
            Clase = new Clase
            {
                Fecha = new DateTime(2026, 10, 7)
            },
            Estado = EstadoAsistencia.Ausente
        });

        DatosSistema.Asistencias.Add(new Asistencia
        {
            Estudiante = estudianteRiesgoAlto,
            Clase = new Clase
            {
                Fecha = new DateTime(2026, 10, 8)
            },
            Estado = EstadoAsistencia.Ausente
        });

        // Entrega = 100 %
        var trabajoAgustina = new Trabajo
        {
            Id = 2,
            Nombre = "TP2"
        };

        DatosSistema.Entregas.Add(new Entrega
        {
            Estudiante = estudianteRiesgoAlto,
            Trabajo = trabajoAgustina,
            Estado = EstadoEntrega.NoEntregado
        });

        // Riesgo final Agustina = 100 %

        // Act
        var viewModel = new RiesgoViewModel();

        // Assert
        Assert.Equal(2, viewModel.Resultados.Count);

        Assert.Equal(
            estudianteRiesgoAlto,
            viewModel.Resultados[0].Estudiante);

        Assert.Equal(
            100m,
            viewModel.Resultados[0].PorcentajeRiesgo);

        Assert.Equal(
            estudianteRiesgoMedio,
            viewModel.Resultados[1].Estudiante);

        Assert.Equal(
            50m,
            viewModel.Resultados[1].PorcentajeRiesgo);
    }
    // Prueba H8
    [Fact]
    public void CargarRiesgos_DeberiaMostrarEstadisticasCorrectas()
    {
        // Arrange
        DatosSistema.Estudiantes.Clear();
        DatosSistema.Asistencias.Clear();
        DatosSistema.Entregas.Clear();

        var estudiante = new Estudiante
        {
            Nombre = "Paola",
            Apellido = "Clemente",
            Legajo = "402",
            Comision = "Comisión A"
        };

        DatosSistema.Estudiantes.Add(estudiante);

        // Asistencias:
        // 1 presente + 2 ausencias
        DatosSistema.Asistencias.Add(new Asistencia
        {
            Estudiante = estudiante,
            Clase = new Clase
            {
                Fecha = new DateTime(2026, 10, 7)
            },
            Estado = EstadoAsistencia.Presente
        });

        DatosSistema.Asistencias.Add(new Asistencia
        {
            Estudiante = estudiante,
            Clase = new Clase
            {
                Fecha = new DateTime(2026, 10, 8)
            },
            Estado = EstadoAsistencia.Ausente
        });

        DatosSistema.Asistencias.Add(new Asistencia
        {
            Estudiante = estudiante,
            Clase = new Clase
            {
                Fecha = new DateTime(2026, 10, 9)
            },
            Estado = EstadoAsistencia.Ausente
        });

        var trabajo1 = new Trabajo
        {
            Id = 3,
            Nombre = "TP1"
        };

        var trabajo2 = new Trabajo
        {
            Id = 4,
            Nombre = "TP2"
        };

        // Entregas:
        // 1 entregado + 1 no entregado
        DatosSistema.Entregas.Add(new Entrega
        {
            Estudiante = estudiante,
            Trabajo = trabajo1,
            Estado = EstadoEntrega.Entregado
        });

        DatosSistema.Entregas.Add(new Entrega
        {
            Estudiante = estudiante,
            Trabajo = trabajo2,
            Estado = EstadoEntrega.NoEntregado
        });

        // Act
        var viewModel = new RiesgoViewModel();

        ResultadoRiesgo resultado =
            Assert.Single(viewModel.Resultados);

        // Assert - Asistencias
        Assert.Equal(estudiante, resultado.Estudiante);
        Assert.Equal(1, resultado.Presentes);
        Assert.Equal(2, resultado.Ausencias);
        Assert.Equal(3, resultado.TotalClases);

        // Assert - Entregas
        Assert.Equal(2, resultado.TotalEntregas);
        Assert.Equal(1, resultado.Entregados);
        Assert.Equal(1, resultado.NoEntregados);
        Assert.Equal(0, resultado.EntregadosFueraDeTermino);

        // Asistencia = 66,67 %
        // Entregas = 50 %
        // Riesgo final = (66,67 + 50) / 2 = 58,34 %
        Assert.Equal(58.34m, resultado.PorcentajeRiesgo);
    }
    //Prueba H4
    [Fact]
    public void CorregirAsistencia_Ausente_DeberiaCambiarAPresente()
    {
        // Arrange
        DatosSistema.Asistencias.Clear();

        var estudiante = new Estudiante
        {
            Nombre = "Agustina",
            Apellido = "Gómez",
            Legajo = "500"
        };

        var asistencia = new Asistencia
        {
            Estudiante = estudiante,

            Clase = new Clase
            {
                Fecha = new DateTime(2026, 10, 14)
            },

            Estado = EstadoAsistencia.Ausente
        };

        DatosSistema.Asistencias.Add(asistencia);

        var viewModel = new AsistenciaViewModel
        {
            AsistenciaSeleccionada = asistencia
        };

        // Act
        viewModel.CorregirAsistenciaCommand.Execute(null);

        // Assert
        Assert.Single(DatosSistema.Asistencias);

        Assert.Equal(
            EstadoAsistencia.Presente,
            DatosSistema.Asistencias[0].Estado);

        Assert.Equal(
            "Asistencia corregida a Presente",
            viewModel.Mensaje);
    }
    //Prueba H4
    [Fact]
    public void CorregirAsistencia_DeberiaMantenerAlumnoFechaYCantidad()
    {
        // Arrange
        DatosSistema.Asistencias.Clear();

        var estudiante = new Estudiante
        {
            Nombre = "Juan",
            Apellido = "Pérez",
            Legajo = "501"
        };

        DateTime fechaOriginal =
            new DateTime(2026, 10, 15);

        var asistencia = new Asistencia
        {
            Estudiante = estudiante,

            Clase = new Clase
            {
                Fecha = fechaOriginal
            },

            Estado = EstadoAsistencia.Presente
        };

        DatosSistema.Asistencias.Add(asistencia);

        int cantidadAntes =
            DatosSistema.Asistencias.Count;

        var viewModel = new AsistenciaViewModel
        {
            AsistenciaSeleccionada = asistencia
        };

        // Act
        viewModel.CorregirAsistenciaCommand.Execute(null);

        // Assert
        Assert.Equal(
            cantidadAntes,
            DatosSistema.Asistencias.Count);

        Asistencia resultado =
            Assert.Single(DatosSistema.Asistencias);

        Assert.Equal(
            EstadoAsistencia.Ausente,
            resultado.Estado);

        Assert.Equal(
            estudiante.Legajo,
            resultado.Estudiante.Legajo);

        Assert.Equal(
            fechaOriginal.Date,
            resultado.Clase.Fecha.Date);
    }
    // Prueba H9
    [Fact]
    public void CargarFicha_DeberiaMostrarSoloHistorialDelEstudianteSeleccionado()
    {
        // Arrange
        DatosSistema.Estudiantes.Clear();
        DatosSistema.Asistencias.Clear();
        DatosSistema.Entregas.Clear();

        var agustina = new Estudiante
        {
            Nombre = "Agustina",
            Apellido = "Gomez",
            Legajo = "600",
            Comision = "Comisión A"
        };

        var juan = new Estudiante
        {
            Nombre = "Juan",
            Apellido = "Perez",
            Legajo = "601",
            Comision = "Comisión A"
        };

        var trabajo = new Trabajo
        {
            Id = 10,
            Nombre = "TP1"
        };

        DatosSistema.Estudiantes.Add(agustina);
        DatosSistema.Estudiantes.Add(juan);

        DatosSistema.Asistencias.Add(new Asistencia
        {
            Estudiante = agustina,
            Clase = new Clase
            {
                Fecha = new DateTime(2026, 10, 10)
            },
            Estado = EstadoAsistencia.Presente
        });

        DatosSistema.Asistencias.Add(new Asistencia
        {
            Estudiante = agustina,
            Clase = new Clase
            {
                Fecha = new DateTime(2026, 10, 11)
            },
            Estado = EstadoAsistencia.Ausente
        });

        DatosSistema.Asistencias.Add(new Asistencia
        {
            Estudiante = juan,
            Clase = new Clase
            {
                Fecha = new DateTime(2026, 10, 12)
            },
            Estado = EstadoAsistencia.Presente
        });

        DatosSistema.Entregas.Add(new Entrega
        {
            Estudiante = agustina,
            Trabajo = trabajo,
            Estado = EstadoEntrega.Entregado
        });

        DatosSistema.Entregas.Add(new Entrega
        {
            Estudiante = juan,
            Trabajo = trabajo,
            Estado = EstadoEntrega.NoEntregado
        });

        var viewModel = new FichaEstudianteViewModel();

        // Act
        viewModel.EstudianteSeleccionado = agustina;

        // Assert
        Assert.Equal(2, viewModel.Historial.Count);

        Assert.All(
            viewModel.Historial,
            asistencia =>
                Assert.Equal(
                    agustina.Legajo,
                    asistencia.Estudiante.Legajo));

        Assert.Single(viewModel.HistorialEntregas);

        Assert.All(
            viewModel.HistorialEntregas,
            entrega =>
                Assert.Equal(
                    agustina.Legajo,
                    entrega.Estudiante.Legajo));
    }
    // Prueba H9
    [Fact]
    public void CargarFicha_DeberiaCalcularEstadisticasCorrectamente()
    {
        // Arrange
        DatosSistema.Estudiantes.Clear();
        DatosSistema.Asistencias.Clear();
        DatosSistema.Entregas.Clear();

        var estudiante = new Estudiante
        {
            Nombre = "Paola",
            Apellido = "Clemente",
            Legajo = "602",
            Comision = "Comisión A"
        };

        var trabajo1 = new Trabajo
        {
            Id = 11,
            Nombre = "TP1"
        };

        var trabajo2 = new Trabajo
        {
            Id = 12,
            Nombre = "TP2"
        };

        DatosSistema.Estudiantes.Add(estudiante);

        // ASISTENCIAS:
        // 1 presente + 1 ausente = 50 % riesgo

        DatosSistema.Asistencias.Add(new Asistencia
        {
            Estudiante = estudiante,
            Clase = new Clase
            {
                Fecha = new DateTime(2026, 10, 10)
            },
            Estado = EstadoAsistencia.Presente
        });

        DatosSistema.Asistencias.Add(new Asistencia
        {
            Estudiante = estudiante,
            Clase = new Clase
            {
                Fecha = new DateTime(2026, 10, 11)
            },
            Estado = EstadoAsistencia.Ausente
        });

        // ENTREGAS:
        // 1 entregado + 1 no entregado = 50 % riesgo

        DatosSistema.Entregas.Add(new Entrega
        {
            Estudiante = estudiante,
            Trabajo = trabajo1,
            Estado = EstadoEntrega.Entregado
        });

        DatosSistema.Entregas.Add(new Entrega
        {
            Estudiante = estudiante,
            Trabajo = trabajo2,
            Estado = EstadoEntrega.NoEntregado
        });

        var viewModel = new FichaEstudianteViewModel();

        // Act
        viewModel.EstudianteSeleccionado = estudiante;

        // Assert - Asistencia

        Assert.Equal(1, viewModel.Presentes);
        Assert.Equal(1, viewModel.Ausencias);
        Assert.Equal(2, viewModel.TotalClases);
        Assert.Equal(2, viewModel.Historial.Count);

        // Assert - Entregas

        Assert.Equal(1, viewModel.Entregados);
        Assert.Equal(1, viewModel.NoEntregados);
        Assert.Equal(0, viewModel.EntregadosFueraDeTermino);
        Assert.Equal(2, viewModel.TotalEntregas);
        Assert.Equal(2, viewModel.HistorialEntregas.Count);

        // Asistencia = 50 %
        // Entregas = 50 %
        // Riesgo combinado = (50 + 50) / 2 = 50 %

        Assert.Equal(
            50m,
            viewModel.PorcentajeRiesgo);
    }
    //Prueba H5
    [Fact]
    public void RegistrarEntrega_DeberiaGuardarEstudianteTrabajoYEstado()
    {
        // Arrange
        DatosSistema.Estudiantes.Clear();
        DatosSistema.Trabajos.Clear();
        DatosSistema.Entregas.Clear();

        var estudiante = new Estudiante
        {
            Nombre = "Ana",
            Apellido = "Lopez",
            Legajo = "H5-001",
            Comision = "Comisión A"
        };

        var trabajo = new Trabajo
        {
            Id = 1,
            Nombre = "TP1"
        };

        DatosSistema.Estudiantes.Add(estudiante);
        DatosSistema.Trabajos.Add(trabajo);

        var viewModel = new TrabajoViewModel
        {
            ComisionSeleccionada = "Comisión A",
            EstudianteSeleccionado = estudiante,
            TrabajoSeleccionado = trabajo,
            EstadoSeleccionado = EstadoEntrega.Entregado
        };

        // Act
        viewModel.RegistrarEntregaCommand.Execute(null);

        // Assert
        Assert.Single(DatosSistema.Entregas);

        var entrega = DatosSistema.Entregas[0];

        Assert.Equal("H5-001", entrega.Estudiante.Legajo);
        Assert.Equal(1, entrega.Trabajo.Id);
        Assert.Equal(EstadoEntrega.Entregado, entrega.Estado);
    }
    //Prueba H5
    [Fact]
    public void RegistrarEntrega_Duplicada_DeberiaSerRechazada()
    {
        // Arrange
        DatosSistema.Estudiantes.Clear();
        DatosSistema.Trabajos.Clear();
        DatosSistema.Entregas.Clear();

        var estudiante = new Estudiante
        {
            Nombre = "Juan",
            Apellido = "Martinez",
            Legajo = "H5-002",
            Comision = "Comisión A"
        };

        var trabajo = new Trabajo
        {
            Id = 2,
            Nombre = "TP2"
        };

        DatosSistema.Estudiantes.Add(estudiante);
        DatosSistema.Trabajos.Add(trabajo);

        DatosSistema.Entregas.Add(new Entrega
        {
            Estudiante = estudiante,
            Trabajo = trabajo,
            Estado = EstadoEntrega.Entregado
        });

        var viewModel = new TrabajoViewModel
        {
            ComisionSeleccionada = "Comisión A",
            EstudianteSeleccionado = estudiante,
            TrabajoSeleccionado = trabajo,
            EstadoSeleccionado = EstadoEntrega.NoEntregado
        };

        // Act
        viewModel.RegistrarEntregaCommand.Execute(null);

        // Assert
        Assert.Single(DatosSistema.Entregas);

        Assert.Equal(
            "La entrega ya fue registrada para ese estudiante y trabajo",
            viewModel.Mensaje);

        // La entrega original tampoco debe modificarse
        Assert.Equal(
            EstadoEntrega.Entregado,
            DatosSistema.Entregas[0].Estado);
    }
    //Prueba H5
    [Fact]
    public void ActualizarEntrega_DeberiaModificarSoloLaEntregaSeleccionada()
    {
        // Arrange
        DatosSistema.Estudiantes.Clear();
        DatosSistema.Trabajos.Clear();
        DatosSistema.Entregas.Clear();

        var ana = new Estudiante
        {
            Nombre = "Ana",
            Apellido = "Lopez",
            Legajo = "H5-003",
            Comision = "Comisión A"
        };

        var juan = new Estudiante
        {
            Nombre = "Juan",
            Apellido = "Perez",
            Legajo = "H5-004",
            Comision = "Comisión A"
        };

        var trabajo = new Trabajo
        {
            Id = 3,
            Nombre = "TP3"
        };

        DatosSistema.Estudiantes.Add(ana);
        DatosSistema.Estudiantes.Add(juan);
        DatosSistema.Trabajos.Add(trabajo);

        var entregaAna = new Entrega
        {
            Estudiante = ana,
            Trabajo = trabajo,
            Estado = EstadoEntrega.NoEntregado
        };

        var entregaJuan = new Entrega
        {
            Estudiante = juan,
            Trabajo = trabajo,
            Estado = EstadoEntrega.Entregado
        };

        DatosSistema.Entregas.Add(entregaAna);
        DatosSistema.Entregas.Add(entregaJuan);

        var viewModel = new TrabajoViewModel
        {
            ComisionSeleccionada = "Comisión A",
            TrabajoSeleccionado = trabajo
        };

        viewModel.ConsultarEntregasCommand.Execute(null);

        viewModel.EntregaSeleccionada =
            viewModel.Entregas.First(e =>
                e.Estudiante.Legajo == "H5-003");

        viewModel.EstadoSeleccionado =
            EstadoEntrega.EntregadoFueraDeTermino;

        // Act
        viewModel.ActualizarEntregaCommand.Execute(null);

        // Assert
        Assert.Equal(2, DatosSistema.Entregas.Count);

        var anaActualizada =
            DatosSistema.Entregas.First(e =>
                e.Estudiante.Legajo == "H5-003");

        var juanSinCambios =
            DatosSistema.Entregas.First(e =>
                e.Estudiante.Legajo == "H5-004");

        Assert.Equal(
            EstadoEntrega.EntregadoFueraDeTermino,
            anaActualizada.Estado);

        Assert.Equal(
            EstadoEntrega.Entregado,
            juanSinCambios.Estado);
    }
}

