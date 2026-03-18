namespace CarWashFacil
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; } = default!;

        private readonly DatabaseService _databaseService;
        private readonly LifecycleService _lifecycleService;
        private readonly LoginPage _loginPage;

        public App(IServiceProvider serviceProvider,
                   DatabaseService databaseService,
                   LifecycleService lifecycleService,
                   LoginPage loginPage)
        {
            InitializeComponent();

            Services = serviceProvider;
            _databaseService = databaseService;
            _lifecycleService = lifecycleService;
            _loginPage = loginPage;

            MainPage = new NavigationPage(_loginPage);
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(MainPage!);

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await _databaseService.InitAsync();
                    _ = _lifecycleService.AddEventSafeAsync("App creada");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error al iniciar DB: {ex}");
                }
            });

            window.Activated += (s, e) => _ = _lifecycleService.AddEventSafeAsync("App activada");
            window.Deactivated += (s, e) => _ = _lifecycleService.AddEventSafeAsync("App desactivada");
            window.Stopped += (s, e) => _ = _lifecycleService.AddEventSafeAsync("App detenida");
            window.Resumed += (s, e) => _ = _lifecycleService.AddEventSafeAsync("App reanudada");

            return window;
        }
    }
}