using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RegistroAsistencia.Datos;
using RegistroAsistencia.Repositorios;
using RegistroAsistencia.Services;
using RegistroAsistencia.ViewModels;
using RegistroAsistencia.Views;

namespace RegistroAsistencia
{
    public static class MauiProgram
    {
        // Esquema con la comisión normalizada (tabla Comisiones con clave);
        // por eso es un archivo nuevo y no reutiliza el anterior
        private const string ArchivoBaseDatos = "registro_asistencia_v2.db3";

        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            RegistrarDatos(builder.Services);
            RegistrarServicios(builder.Services);
            RegistrarPantallas(builder.Services);

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }

        // Los repositorios guardan en SQLite; los servicios solo conocen
        // las interfaces
        private static void RegistrarDatos(IServiceCollection servicios)
        {
            servicios.AddSingleton(_ => new BaseDatos(Path.Combine(
                FileSystem.AppDataDirectory,
                ArchivoBaseDatos)));

            servicios.AddSingleton<IComisionRepositorio, ComisionRepositorioSqlite>();
            servicios.AddSingleton<IEstudianteRepositorio, EstudianteRepositorioSqlite>();
            servicios.AddSingleton<IAsistenciaRepositorio, AsistenciaRepositorioSqlite>();
            servicios.AddSingleton<IEntregaRepositorio, EntregaRepositorioSqlite>();
            servicios.AddSingleton<ICalendarioRepositorio, CalendarioRepositorioSqlite>();
            servicios.AddSingleton<IContactoRepositorio, ContactoRepositorioSqlite>();
            servicios.AddSingleton<IAdministracionRepositorio, AdministracionRepositorioSqlite>();
        }

        private static void RegistrarServicios(IServiceCollection servicios)
        {
            servicios.AddSingleton<UmbralesService>();
            servicios.AddSingleton<ComisionService>();
            servicios.AddSingleton<NominaService>();
            servicios.AddSingleton<CalendarioService>();
            servicios.AddSingleton<ContactoService>();
            servicios.AddSingleton<RegistroService>();
            servicios.AddSingleton<RiesgoService>();

            servicios.AddSingleton<IDialogos, DialogosMaui>();
            servicios.AddSingleton<INavegacion, NavegacionMaui>();
        }

        // Las demás pantallas las abre NavegacionMaui
        private static void RegistrarPantallas(IServiceCollection servicios)
        {
            servicios.AddTransient<InicioViewModel>();
            servicios.AddSingleton<MenuViewModel>();
            servicios.AddTransient<MainPage>();
        }
    }
}
