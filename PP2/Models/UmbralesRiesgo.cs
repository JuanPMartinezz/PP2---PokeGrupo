namespace RegistroAsistencia.Models;

/// <summary>
/// Valores que usa RiesgoService para calcular el riesgo académico.
/// Los valores por defecto reproducen la fórmula original de H6.
/// </summary>
public class UmbralesRiesgo
{
    // Porcentaje del riesgo final que aporta la asistencia (0 a 100).
    // El resto (100 - PesoAsistencia) lo aportan las entregas.
    public decimal PesoAsistencia { get; set; } = 50;

    // Riesgo (0 a 100) que suma una entrega "fuera de término".
    public decimal RiesgoFueraDeTermino { get; set; } = 50;

    // Riesgo (0 a 100) que suma una entrega "no entregada".
    public decimal RiesgoNoEntregado { get; set; } = 100;

    // Riesgo (0 a 100) desde el cual un estudiante pasa a nivel medio.
    public decimal RiesgoMedio { get; set; } = 30;

    // Riesgo (0 a 100) desde el cual un estudiante pasa a nivel alto.
    public decimal RiesgoAlto { get; set; } = 60;

    public decimal PesoEntregas => 100 - PesoAsistencia;

    public UmbralesRiesgo Copiar()
    {
        return new UmbralesRiesgo
        {
            PesoAsistencia = PesoAsistencia,
            RiesgoFueraDeTermino = RiesgoFueraDeTermino,
            RiesgoNoEntregado = RiesgoNoEntregado,
            RiesgoMedio = RiesgoMedio,
            RiesgoAlto = RiesgoAlto
        };
    }

    /// <summary>
    /// Devuelve el motivo por el que los valores no son válidos,
    /// o null si todos son correctos.
    /// </summary>
    public string? Validar()
    {
        if (!EsPorcentaje(PesoAsistencia))
        {
            return "El peso de la asistencia no es válido: debe estar entre 0 y 100";
        }

        if (!EsPorcentaje(RiesgoFueraDeTermino))
        {
            return "El riesgo por entrega fuera de término no es válido: debe estar entre 0 y 100";
        }

        if (!EsPorcentaje(RiesgoNoEntregado))
        {
            return "El riesgo por entrega no entregada no es válido: debe estar entre 0 y 100";
        }

        if (RiesgoFueraDeTermino > RiesgoNoEntregado)
        {
            return "El riesgo por entrega fuera de término no es válido: no puede ser mayor que el de una entrega no entregada";
        }

        if (!EsPorcentaje(RiesgoMedio))
        {
            return "El umbral de riesgo medio no es válido: debe estar entre 0 y 100";
        }

        if (!EsPorcentaje(RiesgoAlto))
        {
            return "El umbral de riesgo alto no es válido: debe estar entre 0 y 100";
        }

        if (RiesgoMedio > RiesgoAlto)
        {
            return "El umbral de riesgo medio no es válido: no puede ser mayor que el de riesgo alto";
        }

        return null;
    }

    private static bool EsPorcentaje(decimal valor)
    {
        return valor >= 0 && valor <= 100;
    }
}
