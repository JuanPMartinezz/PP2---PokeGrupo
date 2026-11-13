namespace RegistroAsistencia.Models
{
    public class Estudiante
    {
        public string Legajo { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public string Apellido { get; set; } = string.Empty;

        public string Comision { get; set; } = string.Empty;

        // Desde cuándo cursa. Las clases anteriores no cuentan para su
        // riesgo. Sin fecha (valor por defecto): cursa desde el comienzo.
        public DateTime FechaIncorporacion { get; set; }

        public override string ToString()
        {
            return $"{Apellido}, {Nombre}";
        }
    }
}
