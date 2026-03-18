namespace CarWashFacil.Views
{
    public partial class HomePage : ContentPage
    {
        private readonly HomeViewModel _viewModel;
        private readonly LifecycleService _lifecycleService;

        public HomePage(HomeViewModel viewModel, LifecycleService lifecycleService)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
            _lifecycleService = lifecycleService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                await _viewModel.CargarAsync();
                _ = _lifecycleService.AddEventSafeAsync("Página Inicio abierta");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error HomePage OnAppearing: {ex}");
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _ = _lifecycleService.AddEventSafeAsync("Página Inicio cerrada");
        }
    }
}