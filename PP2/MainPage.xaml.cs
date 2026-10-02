namespace PP2
{
    using System.Collections.ObjectModel;

    public partial class MainPage : ContentPage
    {
        int count = 0;

        ObservableCollection<Estudiante> estudiantes = new ObservableCollection<Estudiante>();

        public MainPage()
        {
            InitializeComponent();
            BindingContext = estudiantes;
            
        }

        private async void CargarEstudiante_Clicked(object? sender, EventArgs e)
        {
            Estudiante est = new Estudiante();
            est.nombre = NombreEstudiante.Text;
            est.apellido = ApellidoEstudiante.Text;
            est.dni = int.Parse(DniEstudiante.Text);
            est.comision = int.Parse(ComisionEstudiante.Text);
            est.asistencia = double.Parse(AsistenciaEstudiante.Text);
            est.riesgo = double.Parse(RiesgoEstudiante.Text);
            estudiantes.Add(est);

            await DisplayAlertAsync("Prueba", "Alumno Cargado Correctamente", "OK");
        }
    }
}
