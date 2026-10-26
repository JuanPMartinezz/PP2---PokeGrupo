using RegistroAsistencia.Models;

namespace RegistroAsistencia.Services;

/// <summary>
/// Orden y filtros de la lista de alumnos de una comisión (H8 y H11).
/// Es una regla del negocio: se puede usar sin abrir ninguna pantalla.
/// </summary>
public static class ListadoAlumnos
{
    public static List<ResultadoRiesgo> Aplicar(
        IEnumerable<ResultadoRiesgo> resultados,
        OrdenLista orden,
        FiltroContacto contacto,
        FiltroNivel nivel,
        DateTime hoy)
    {
        IEnumerable<ResultadoRiesgo> filtrados = resultados
            .Where(r => CumpleContacto(r, contacto, hoy))
            .Where(r => CumpleNivel(r, nivel));

        return Ordenar(filtrados, orden).ToList();
    }

    private static bool CumpleContacto(
        ResultadoRiesgo resultado,
        FiltroContacto filtro,
        DateTime hoy)
    {
        return filtro switch
        {
            FiltroContacto.SinContactar => !resultado.Contactado,
            FiltroContacto.SinContactoReciente =>
                resultado.SinContactoDesde(
                    hoy.AddDays(
                        -ResultadoRiesgo.DiasSinContactoReciente)),
            FiltroContacto.ParaLlamar =>
                resultado.ParaLlamar(hoy),
            _ => true
        };
    }

    private static bool CumpleNivel(
        ResultadoRiesgo resultado,
        FiltroNivel filtro)
    {
        return filtro switch
        {
            FiltroNivel.Alto => resultado.NivelRiesgo == NivelesRiesgo.Alto,
            FiltroNivel.Medio => resultado.NivelRiesgo == NivelesRiesgo.Medio,
            FiltroNivel.Bajo => resultado.NivelRiesgo == NivelesRiesgo.Bajo,
            _ => true
        };
    }

    private static IEnumerable<ResultadoRiesgo> Ordenar(
        IEnumerable<ResultadoRiesgo> resultados,
        OrdenLista orden)
    {
        return orden switch
        {
            OrdenLista.ApellidoZA => resultados
                .OrderByDescending(r => r.Estudiante.Apellido)
                .ThenByDescending(r => r.Estudiante.Nombre),
            OrdenLista.MayorRiesgo => resultados
                .OrderByDescending(r => r.PorcentajeRiesgo)
                .ThenBy(r => r.Estudiante.Apellido),
            _ => resultados
                .OrderBy(r => r.Estudiante.Apellido)
                .ThenBy(r => r.Estudiante.Nombre)
        };
    }
}
