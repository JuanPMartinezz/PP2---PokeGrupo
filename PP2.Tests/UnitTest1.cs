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
    //Prueba H2 - CAT1
    [Fact]
    public void RegistrarActividad_Clase_DeberiaRegistrarLaFechaEnElCalendario()
    {
        // Arrange
        DatosSistema.Calendario.Clear();

        var viewModel = new CalendarioViewModel
        {
            ComisionSeleccionada = "Comisión A",
            CuatrimestreSeleccionado = "2026 - 2do cuatrimestre",
            TipoSeleccionado = TipoActividad.Clase,
            FechaActividad = new DateTime(2026, 10, 14)
        };

        // Act
        viewModel.RegistrarActividadCommand.Execute(null);

        // Assert
        ActividadCalendario actividad =
            Assert.Single(DatosSistema.Calendario);

        Assert.Equal(
            new DateTime(2026, 10, 14),
            actividad.Fecha.Date);

        Assert.Equal(
            TipoActividad.Clase,
            actividad.Tipo);

        Assert.Equal(
            "Comisión A",
            actividad.Comision);

        Assert.Equal(
            "2026 - 2do cuatrimestre",
            actividad.Cuatrimestre);

        Assert.Equal(
            "Clase registrada correctamente",
            viewModel.Mensaje);
    }
    //Prueba H2 - CAT4
    [Fact]
    public void ConsultarCalendario_DeberiaOrdenarLasActividadesCronologicamente()
    {
        // Arrange
        DatosSistema.Calendario.Clear();

        // Se cargan desordenadas a propósito
        DatosSistema.Calendario.Add(new ActividadCalendario
        {
            Comision = "Comisión A",
            Cuatrimestre = "2026 - 2do cuatrimestre",
            Fecha = new DateTime(2026, 10, 28),
            Tipo = TipoActividad.Trabajo,
            Descripcion = "TP1"
        });

        DatosSistema.Calendario.Add(new ActividadCalendario
        {
            Comision = "Comisión A",
            Cuatrimestre = "2026 - 2do cuatrimestre",
            Fecha = new DateTime(2026, 10, 14),
            Tipo = TipoActividad.Clase,
            Descripcion = "Clase"
        });

        DatosSistema.Calendario.Add(new ActividadCalendario
        {
            Comision = "Comisión A",
            Cuatrimestre = "2026 - 2do cuatrimestre",
            Fecha = new DateTime(2026, 10, 21),
            Tipo = TipoActividad.Clase,
            Descripcion = "Clase"
        });

        // Act
        var viewModel = new CalendarioViewModel
        {
            ComisionSeleccionada = "Comisión A",
            CuatrimestreSeleccionado = "2026 - 2do cuatrimestre"
        };

        // Assert
        DateTime[] fechas =
            viewModel.Actividades
                .Select(a => a.Fecha.Date)
                .ToArray();

        Assert.Equal(
            new[]
            {
                new DateTime(2026, 10, 14),
                new DateTime(2026, 10, 21),
                new DateTime(2026, 10, 28)
            },
            fechas);
    }
    //Prueba H10 - CA3
    [Fact]
    public void CargarFicha_DeberiaMostrarElResultadoDelContacto()
    {
        // Arrange
        DatosSistema.Estudiantes.Clear();
        DatosSistema.Asistencias.Clear();
        DatosSistema.Entregas.Clear();
        DatosSistema.Contactos.Clear();

        var estudiante = new Estudiante
        {
            Nombre = "Paola",
            Apellido = "Clemente",
            Legajo = "H10-003",
            Comision = "Comisión A"
        };

        DatosSistema.Estudiantes.Add(estudiante);

        DatosSistema.Contactos.Add(new ContactoEstudiante
        {
            Estudiante = estudiante,
            Fecha = new DateTime(2026, 10, 22),
            Resultado = "No respondió, se reintenta la semana próxima"
        });

        var viewModel = new FichaEstudianteViewModel();

        // Act
        viewModel.EstudianteSeleccionado = estudiante;

        // Assert
        ContactoEstudiante contacto =
            Assert.Single(viewModel.HistorialContactos);

        Assert.Equal(
            "No respondió, se reintenta la semana próxima",
            contacto.Resultado);

        Assert.Equal(
            new DateTime(2026, 10, 22),
            contacto.Fecha.Date);

        Assert.Equal(1, viewModel.TotalContactos);
    }
    //Prueba H10 - CA4
    [Fact]
    public void RegistrarContacto_DeberiaAsociarseSoloAlEstudianteCorrespondiente()
    {
        // Arrange
        DatosSistema.Estudiantes.Clear();
        DatosSistema.Contactos.Clear();

        var ana = new Estudiante
        {
            Nombre = "Ana",
            Apellido = "Lopez",
            Legajo = "H10-004",
            Comision = "Comisión A"
        };

        var juan = new Estudiante
        {
            Nombre = "Juan",
            Apellido = "Perez",
            Legajo = "H10-005",
            Comision = "Comisión A"
        };

        DatosSistema.Estudiantes.Add(ana);
        DatosSistema.Estudiantes.Add(juan);

        var viewModel = new ContactoViewModel
        {
            ComisionSeleccionada = "Comisión A",
            EstudianteSeleccionado = ana,
            FechaContacto = new DateTime(2026, 10, 20),
            Resultado = "Respondió el mensaje"
        };

        viewModel.RegistrarContactoCommand.Execute(null);

        // Act
        viewModel.EstudianteSeleccionado = juan;
        viewModel.Resultado = "No respondió";

        viewModel.RegistrarContactoCommand.Execute(null);

        // Assert
        Assert.Equal(2, DatosSistema.Contactos.Count);

        ContactoEstudiante contactoAna =
            Assert.Single(
                DatosSistema.Contactos,
                c => c.Estudiante.Legajo == "H10-004");

        Assert.Equal(
            "Respondió el mensaje",
            contactoAna.Resultado);

        ContactoEstudiante contactoJuan =
            Assert.Single(
                DatosSistema.Contactos,
                c => c.Estudiante.Legajo == "H10-005");

        Assert.Equal(
            "No respondió",
            contactoJuan.Resultado);

        // La consulta del estudiante seleccionado solo muestra el suyo
        ContactoEstudiante mostrado =
            Assert.Single(viewModel.Contactos);

        Assert.Equal(
            "H10-005",
            mostrado.Estudiante.Legajo);

        // Una fecha futura no se puede registrar
        viewModel.Resultado = "Respondió el mensaje";
        viewModel.FechaContacto = DateTime.Today.AddDays(1);

        viewModel.RegistrarContactoCommand.Execute(null);

        Assert.Contains("futura", viewModel.Mensaje);
        Assert.Equal(2, DatosSistema.Contactos.Count);
    }
    //Prueba H7 - CA1 y CA3
    [Fact]
    public void GuardarUmbrales_ConValoresValidos_DeberiaUsarlosEnElCalculoDeRiesgo()
    {
        // Arrange
        DatosSistema.RestablecerUmbrales();
        DatosSistema.Asistencias.Clear();
        DatosSistema.Entregas.Clear();

        var estudiante = new Estudiante
        {
            Nombre = "Ana",
            Apellido = "Lopez",
            Legajo = "H7-001",
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

        // Asistencia: 2 ausencias = 100 % de riesgo
        DatosSistema.Asistencias.Add(new Asistencia
        {
            Estudiante = estudiante,
            Clase = new Clase
            {
                Fecha = new DateTime(2026, 10, 7)
            },
            Estado = EstadoAsistencia.Ausente
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

        // Entregas: 1 no entregado + 1 fuera de término
        DatosSistema.Entregas.Add(new Entrega
        {
            Estudiante = estudiante,
            Trabajo = trabajo1,
            Estado = EstadoEntrega.NoEntregado
        });

        DatosSistema.Entregas.Add(new Entrega
        {
            Estudiante = estudiante,
            Trabajo = trabajo2,
            Estado = EstadoEntrega.EntregadoFueraDeTermino
        });

        var riesgoService = new RiesgoService();

        // Con los valores por defecto:
        // entregas = (100 + 50) / 2 = 75 %
        // riesgo final = (100 + 75) / 2 = 87,5 %
        Assert.Equal(
            87.5m,
            riesgoService.CalcularRiesgo(estudiante));

        var viewModel = new ConfiguracionViewModel
        {
            PesoAsistenciaTexto = "70",
            RiesgoFueraDeTerminoTexto = "20",
            RiesgoNoEntregadoTexto = "80"
        };

        try
        {
            // Act
            viewModel.GuardarUmbralesCommand.Execute(null);

            // Assert - se guardaron
            Assert.Equal(
                "Umbrales guardados correctamente",
                viewModel.Mensaje);

            Assert.Equal(70m, DatosSistema.Umbrales.PesoAsistencia);
            Assert.Equal(20m, DatosSistema.Umbrales.RiesgoFueraDeTermino);
            Assert.Equal(80m, DatosSistema.Umbrales.RiesgoNoEntregado);

            // Assert - el nuevo cálculo usa los valores configurados
            // entregas = (80 + 20) / 2 = 50 %
            // riesgo final = (100 * 70 + 50 * 30) / 100 = 85 %
            Assert.Equal(
                85m,
                riesgoService.CalcularRiesgo(estudiante));

            // Assert - el nivel usa los cortes configurados (medio 30, alto 60)
            Assert.Equal("Alto", riesgoService.ObtenerNivel(85m, 4));
            Assert.Equal("Sin datos", riesgoService.ObtenerNivel(0m, 0));

            // Act - se suben los cortes sin tocar el código
            viewModel.RiesgoMedioTexto = "40";
            viewModel.RiesgoAltoTexto = "90";
            viewModel.GuardarUmbralesCommand.Execute(null);

            // Assert - el mismo riesgo ahora es de nivel medio
            Assert.Equal("Medio", riesgoService.ObtenerNivel(85m, 4));
        }
        finally
        {
            DatosSistema.RestablecerUmbrales();
        }
    }
    //Prueba H7 - CA2 y CA4
    [Fact]
    public void GuardarUmbrales_ConValorInvalido_NoDeberiaGuardarse()
    {
        // Arrange
        DatosSistema.GuardarUmbrales(new UmbralesRiesgo
        {
            PesoAsistencia = 60,
            RiesgoFueraDeTermino = 30,
            RiesgoNoEntregado = 90
        });

        try
        {
            var viewModel = new ConfiguracionViewModel();

            // Assert - muestra los valores actualmente establecidos
            Assert.Equal("60", viewModel.PesoAsistenciaTexto);
            Assert.Equal("30", viewModel.RiesgoFueraDeTerminoTexto);
            Assert.Equal("90", viewModel.RiesgoNoEntregadoTexto);

            // Act - valor fuera de rango
            viewModel.RiesgoNoEntregadoTexto = "150";
            viewModel.GuardarUmbralesCommand.Execute(null);

            // Assert
            Assert.Contains("no es válido", viewModel.Mensaje);
            Assert.Equal(90m, DatosSistema.Umbrales.RiesgoNoEntregado);

            // Act - texto que no es un número
            viewModel.Mensaje = "";
            viewModel.RiesgoNoEntregadoTexto = "abc";
            viewModel.GuardarUmbralesCommand.Execute(null);

            // Assert - tampoco se guardó
            Assert.Contains("no es válido", viewModel.Mensaje);
            Assert.Equal(60m, DatosSistema.Umbrales.PesoAsistencia);
            Assert.Equal(30m, DatosSistema.Umbrales.RiesgoFueraDeTermino);
            Assert.Equal(90m, DatosSistema.Umbrales.RiesgoNoEntregado);

            // Act - corte medio mayor que el alto
            viewModel.Mensaje = "";
            viewModel.RiesgoNoEntregadoTexto = "90";
            viewModel.RiesgoMedioTexto = "70";
            viewModel.RiesgoAltoTexto = "50";
            viewModel.GuardarUmbralesCommand.Execute(null);

            // Assert - tampoco se guardó
            Assert.Contains("no es válido", viewModel.Mensaje);
            Assert.Equal(30m, DatosSistema.Umbrales.RiesgoMedio);
            Assert.Equal(60m, DatosSistema.Umbrales.RiesgoAlto);
        }
        finally
        {
            DatosSistema.RestablecerUmbrales();
        }
    }
    //Prueba H11 - CA1, CA2 y CA3
    [Fact]
    public void CargarRiesgos_DeberiaIndicarQuienFueContactadoYCuando()
    {
        // Arrange
        DatosSistema.Estudiantes.Clear();
        DatosSistema.Asistencias.Clear();
        DatosSistema.Entregas.Clear();
        DatosSistema.Contactos.Clear();

        var contactada = new Estudiante
        {
            Nombre = "Ana",
            Apellido = "Lopez",
            Legajo = "H11-001",
            Comision = "Comisión A"
        };

        var sinContactar = new Estudiante
        {
            Nombre = "Juan",
            Apellido = "Perez",
            Legajo = "H11-002",
            Comision = "Comisión A"
        };

        DatosSistema.Estudiantes.Add(contactada);
        DatosSistema.Estudiantes.Add(sinContactar);

        DatosSistema.Contactos.Add(new ContactoEstudiante
        {
            Estudiante = contactada,
            Fecha = new DateTime(2026, 10, 20),
            Resultado = "Respondió el mensaje"
        });

        // Act
        var viewModel = new RiesgoViewModel();

        // Assert
        ResultadoRiesgo ana =
            Assert.Single(
                viewModel.Resultados,
                r => r.Estudiante.Legajo == "H11-001");

        ResultadoRiesgo juan =
            Assert.Single(
                viewModel.Resultados,
                r => r.Estudiante.Legajo == "H11-002");

        Assert.True(ana.Contactado);
        Assert.Equal("Contactado", ana.EstadoContacto);
        Assert.Equal(
            new DateTime(2026, 10, 20),
            ana.UltimoContacto);

        Assert.False(juan.Contactado);
        Assert.Equal("Sin contactar", juan.EstadoContacto);
        Assert.Null(juan.UltimoContacto);

        Assert.Equal(
            "Contactados: 1 de 2",
            viewModel.ResumenContactos);

        // Filtro: solo los contactados
        viewModel.FiltroContactoSeleccionado =
            RiesgoViewModel.FiltroSoloContactados;

        ResultadoRiesgo soloContactada =
            Assert.Single(viewModel.Resultados);

        Assert.Equal("H11-001", soloContactada.Estudiante.Legajo);

        Assert.Equal(
            "Mostrando 1 de 2 estudiantes",
            viewModel.ResumenMostrados);

        // Filtro: solo los que todavía no fueron contactados
        viewModel.FiltroContactoSeleccionado =
            RiesgoViewModel.FiltroSoloSinContactar;

        ResultadoRiesgo soloSinContactar =
            Assert.Single(viewModel.Resultados);

        Assert.Equal("H11-002", soloSinContactar.Estudiante.Legajo);

        // Filtro: todos los alumnos
        viewModel.FiltroContactoSeleccionado =
            RiesgoViewModel.FiltroTodos;

        Assert.Equal(2, viewModel.Resultados.Count);
    }
    //Prueba H11 - CA4
    [Fact]
    public void CargarRiesgos_ConVariosContactos_DeberiaMostrarTodosLosDelEstudiante()
    {
        // Arrange
        DatosSistema.Estudiantes.Clear();
        DatosSistema.Asistencias.Clear();
        DatosSistema.Entregas.Clear();
        DatosSistema.Contactos.Clear();

        var ana = new Estudiante
        {
            Nombre = "Ana",
            Apellido = "Lopez",
            Legajo = "H11-003",
            Comision = "Comisión A"
        };

        var juan = new Estudiante
        {
            Nombre = "Juan",
            Apellido = "Perez",
            Legajo = "H11-004",
            Comision = "Comisión A"
        };

        DatosSistema.Estudiantes.Add(ana);
        DatosSistema.Estudiantes.Add(juan);

        DatosSistema.Contactos.Add(new ContactoEstudiante
        {
            Estudiante = ana,
            Fecha = new DateTime(2026, 10, 10),
            Resultado = "No respondió"
        });

        DatosSistema.Contactos.Add(new ContactoEstudiante
        {
            Estudiante = ana,
            Fecha = new DateTime(2026, 10, 20),
            Resultado = "Respondió y se comprometió a entregar el TP1"
        });

        DatosSistema.Contactos.Add(new ContactoEstudiante
        {
            Estudiante = juan,
            Fecha = new DateTime(2026, 10, 15),
            Resultado = "Respondió el mensaje"
        });

        // Act
        var viewModel = new RiesgoViewModel();

        // Assert
        ResultadoRiesgo resultadoAna =
            Assert.Single(
                viewModel.Resultados,
                r => r.Estudiante.Legajo == "H11-003");

        Assert.Equal(2, resultadoAna.CantidadContactos);

        Assert.Equal(
            new DateTime(2026, 10, 20),
            resultadoAna.UltimoContacto);

        // Del más reciente al más antiguo
        Assert.Equal(
            "Respondió y se comprometió a entregar el TP1",
            resultadoAna.Contactos[0].Resultado);

        Assert.Equal(
            "No respondió",
            resultadoAna.Contactos[1].Resultado);

        // Los contactos de Juan no se mezclan con los de Ana
        ResultadoRiesgo resultadoJuan =
            Assert.Single(
                viewModel.Resultados,
                r => r.Estudiante.Legajo == "H11-004");

        ContactoEstudiante contactoJuan =
            Assert.Single(resultadoJuan.Contactos);

        Assert.Equal(
            "Respondió el mensaje",
            contactoJuan.Resultado);
    }
}
