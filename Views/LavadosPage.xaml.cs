namespace CarWashFacil.Views
{
    public partial class LavadosPage : ContentPage
    {
        private readonly LavadosViewModel _viewModel;
        private readonly LifecycleService _lifecycleService;

        public LavadosPage(LavadosViewModel viewModel, LifecycleService lifecycleService)
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
                _ = _lifecycleService.AddEventSafeAsync("Página Lavados abierta");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en LavadosPage OnAppearing: {ex}");
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _ = _lifecycleService.AddEventSafeAsync("Página Lavados cerrada");
        }
    }
}