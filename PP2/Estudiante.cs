using System;
using System.Collections.Generic;
using System.Text;

namespace PP2
{
    internal class Estudiante
    {
         public string nombre { get; set; }
         public string apellido { get; set; }
         public int dni { get; set; }
         public int comision { get; set; }
         public double asistencia { get; set; } //Porcentaje de asistencias
         public double riesgo { get; set; } // Porcentaje de riesgos

    }
}
