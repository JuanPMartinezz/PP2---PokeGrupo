using System;
using System.Collections.Generic;
using System.Text;

namespace RegistroAsistencia.Models
{
    public class Comision
    {
        public string Nombre { get; set; } = "Comisión A";

        public List<Estudiante> Estudiantes { get; set; }
        = new();
    }
}
