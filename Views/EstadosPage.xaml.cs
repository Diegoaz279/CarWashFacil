namespace CarWashFacil.Views
{
    public partial class EstadosPage : ContentPage
    {
        private readonly EstadosViewModel _viewModel;
        private readonly LifecycleService _lifecycleService;

        public EstadosPage(EstadosViewModel viewModel, LifecycleService lifecycleService)
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
                _ = _lifecycleService.AddEventSafeAsync("Página Estados abierta");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error EstadosPage OnAppearing: {ex}");
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _ = _lifecycleService.AddEventSafeAsync("Página Estados cerrada");
        }
    }
}