using RegistroAsistencia.Models;
using RegistroAsistencia.Services;
using RegistroAsistencia.ViewModels;

namespace PP2.Tests;

public class UnitTest1
{
    //Prueba H6
    [Fact]
    public void CalcularRiesgo_DeberiaRetornar50Porciento()
    {
        // Arrange: preparamos los datos de prueba
        DatosSistema.Asistencias.Clear();

        var estudiante = new Estudiante
        {
            Nombre = "Juan",
            Apellido = "Martinez",
            Legajo = "100"
        };

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

        var riesgoService = new RiesgoService();

        // Act: ejecutamos el método que queremos probar
        decimal resultado =
            riesgoService.CalcularPorcentajeAusencias(estudiante);

        // Assert: verificamos el resultado
        Assert.Equal(50m, resultado);
    }
    //Prueba H6
    [Fact]
    public void CalcularRiesgo_SinAusencias_DeberiaRetornarCero()
    {
        // Arrange
        DatosSistema.Asistencias.Clear();

        var estudiante = new Estudiante
        {
            Nombre = "Agustina",
            Apellido = "Gómez",
            Legajo = "101"
        };

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

        var riesgoService = new RiesgoService();

        // Act
        decimal resultado =
            riesgoService.CalcularPorcentajeAusencias(estudiante);

        // Assert
        Assert.Equal(0m, resultado);
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
        Assert.Equal("Alumno agregado", viewModel.Mensaje);
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
    //Pruebas h3
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
    //Prueba H8
    [Fact]
    public void CargarRiesgos_DeberiaOrdenarMayorRiesgoPrimero()
    {
        // Arrange
        DatosSistema.Estudiantes.Clear();
        DatosSistema.Asistencias.Clear();

        var estudianteRiesgoMedio = new Estudiante
        {
            Nombre = "Juan",
            Apellido = "Martinez",
            Legajo = "400"
        };

        var estudianteRiesgoAlto = new Estudiante
        {
            Nombre = "Agustina",
            Apellido = "Gómez",
            Legajo = "401"
        };

        DatosSistema.Estudiantes.Add(estudianteRiesgoMedio);
        DatosSistema.Estudiantes.Add(estudianteRiesgoAlto);

        // Juan: una presencia y una ausencia = 50 %
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

        // Agustina: dos ausencias = 100 %
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

        // Act
        var viewModel = new RiesgoViewModel();

        // Assert
        Assert.Equal(2, viewModel.Resultados.Count);

        Assert.Equal(
            estudianteRiesgoAlto,
            viewModel.Resultados[0].Estudiante);

        Assert.Equal(
            100m,
            viewModel.Resultados[0].PorcentajeAusencias);

        Assert.Equal(
            estudianteRiesgoMedio,
            viewModel.Resultados[1].Estudiante);

        Assert.Equal(
            50m,
            viewModel.Resultados[1].PorcentajeAusencias);
    }
    //Prueba H8
    [Fact]
    public void CargarRiesgos_DeberiaMostrarEstadisticasCorrectas()
    {
        // Arrange
        DatosSistema.Estudiantes.Clear();
        DatosSistema.Asistencias.Clear();

        var estudiante = new Estudiante
        {
            Nombre = "Paola",
            Apellido = "Clemente",
            Legajo = "402"
        };

        DatosSistema.Estudiantes.Add(estudiante);

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

        // Act
        var viewModel = new RiesgoViewModel();

        ResultadoRiesgo resultado =
            Assert.Single(viewModel.Resultados);

        // Assert
        Assert.Equal(estudiante, resultado.Estudiante);
        Assert.Equal(1, resultado.Presentes);
        Assert.Equal(2, resultado.Ausencias);
        Assert.Equal(3, resultado.TotalClases);
        Assert.Equal(66.67m, resultado.PorcentajeAusencias);
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
    //Prueba H9
    [Fact]
    public void CargarFicha_DeberiaMostrarSoloHistorialDelEstudianteSeleccionado()
    {
        // Arrange
        DatosSistema.Estudiantes.Clear();
        DatosSistema.Asistencias.Clear();

        var agustina = new Estudiante
        {
            Nombre = "Agustina",
            Apellido = "Gomez",
            Legajo = "600"
        };

        var juan = new Estudiante
        {
            Nombre = "Juan",
            Apellido = "Perez",
            Legajo = "601"
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

        Assert.Equal(
            new DateTime(2026, 10, 10),
            viewModel.Historial[0].Clase.Fecha);

        Assert.Equal(
            new DateTime(2026, 10, 11),
            viewModel.Historial[1].Clase.Fecha);
    }
    //Prueba H9
    [Fact]
    public void CargarFicha_DeberiaCalcularEstadisticasCorrectamente()
    {
        // Arrange
        DatosSistema.Estudiantes.Clear();
        DatosSistema.Asistencias.Clear();

        var estudiante = new Estudiante
        {
            Nombre = "Paola",
            Apellido = "Clemente",
            Legajo = "602"
        };

        DatosSistema.Estudiantes.Add(estudiante);

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

        DatosSistema.Asistencias.Add(new Asistencia
        {
            Estudiante = estudiante,
            Clase = new Clase
            {
                Fecha = new DateTime(2026, 10, 12)
            },
            Estado = EstadoAsistencia.Ausente
        });

        var viewModel = new FichaEstudianteViewModel();

        // Act
        viewModel.EstudianteSeleccionado = estudiante;

        // Assert
        Assert.Equal(1, viewModel.Presentes);

        Assert.Equal(2, viewModel.Ausencias);

        Assert.Equal(3, viewModel.TotalClases);

        Assert.Equal(
            66.67m,
            viewModel.PorcentajeAusencias);

        Assert.Equal(
            3,
            viewModel.Historial.Count);
    }
}

