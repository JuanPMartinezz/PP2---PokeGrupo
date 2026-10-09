using System;
using System.Collections.Generic;
using System.Text;

namespace RegistroAsistencia.Models;

public class Asistencia
{
    public Estudiante Estudiante { get; set; }

    public Clase Clase { get; set; }

    public EstadoAsistencia Estado { get; set; }
}
