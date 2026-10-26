using Microsoft.Extensions.DependencyInjection;
using RegistroAsistencia.ViewModels;
using System.ComponentModel;

namespace RegistroAsistencia
{
    public partial class AppShell : Shell
    {
        private readonly MenuViewModel _menu;

        public AppShell(IServiceProvider servicios)
        {
            InitializeComponent();

            _menu = servicios.GetRequiredService<MenuViewModel>();
            PanelMenu.BindingContext = _menu;

            PropertyChanged += AlCambiarElMenu;
        }

        // Al abrir el menú se vuelven a leer las comisiones
        private void AlCambiarElMenu(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FlyoutIsPresented) && FlyoutIsPresented)
            {
                _menu.Actualizar();
            }
        }
    }
}
