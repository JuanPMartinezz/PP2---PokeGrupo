using RegistroAsistencia.Models;
using RegistroAsistencia.Repositorios;

namespace RegistroAsistencia.Services;

/// <summary>
/// Configuración del cálculo de riesgo de cada comisión (H7).
/// Una comisión que no configuró nada usa los valores por defecto.
/// </summary>
public class UmbralesService
{
    private readonly IComisionRepositorio _comisiones;

    public UmbralesService(IComisionRepositorio comisiones)
    {
        _comisiones = comisiones;
    }

    public UmbralesRiesgo Obtener(string comision)
    {
        return _comisiones.ObtenerUmbrales(comision)
               ?? new UmbralesRiesgo();
    }

    // Devuelve el motivo si los valores no son válidos; en ese caso
    // no se guarda ninguno
    public string? Guardar(string comision, UmbralesRiesgo umbrales)
    {
        string? error = umbrales.Validar();

        if (error is null)
        {
            // Se guarda una copia: si alguien modifica después el objeto
            // recibido, la configuración no cambia sin validarse
            _comisiones.GuardarUmbrales(comision, umbrales.Copiar());
        }

        return error;
    }

    public void Restablecer(string comision)
    {
        _comisiones.GuardarUmbrales(comision, new UmbralesRiesgo());
    }
}
