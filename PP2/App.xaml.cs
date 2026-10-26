namespace RegistroAsistencia
{
    public partial class App : Application
    {
        private readonly IServiceProvider _servicios;

        public App(IServiceProvider servicios)
        {
            InitializeComponent();

            _servicios = servicios;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new StartupPage(_servicios));
        }
    }
}
