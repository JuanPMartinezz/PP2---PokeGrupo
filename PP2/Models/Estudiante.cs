using System;
using System.Collections.Generic;
using System.Text;

namespace RegistroAsistencia.Models
{
    public class Estudiante
    {
        public string Legajo { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public string Apellido { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"{Apellido}, {Nombre}";
        }
    }
}
