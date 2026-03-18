namespace CarWashFacil.Views
{
    public partial class CajaPage : ContentPage
    {
        private readonly CajaViewModel _viewModel;
        private readonly LifecycleService _lifecycleService;

        public CajaPage(CajaViewModel viewModel, LifecycleService lifecycleService)
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
                _ = _lifecycleService.AddEventSafeAsync("Página Caja abierta");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error CajaPage OnAppearing: {ex}");
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _ = _lifecycleService.AddEventSafeAsync("Página Caja cerrada");
        }
    }
}