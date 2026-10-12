using RegistroAsistencia.ViewModels;
using RegistroAsistencia.Views;

namespace RegistroAsistencia;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();

        BindingContext = new ComisionViewModel();
    }

    private async void IrAH3_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AsistenciaPage());
    }



    private async void BorrarTodo_Clicked(object sender, EventArgs e)
    {
        bool confirmar = await DisplayAlert( "Confirmar eliminación","Se eliminarán TODOS los alumnos y TODAS las asistencias. Esta acción no se puede deshacer. ¿Continuar?","Sí, borrar todo","Cancelar");

        if (!confirmar)
            return;

        if (BindingContext is ComisionViewModel vm)
        {
            vm.BorrarTodosLosRegistrosCommand.Execute(null);

            await DisplayAlert("Resultado",vm.Mensaje,"Aceptar");
        }
    }
}