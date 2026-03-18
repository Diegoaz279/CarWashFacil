namespace CarWashFacil.Views
{
    public partial class ReportesPage : ContentPage
    {
        private readonly ReportesViewModel _viewModel;
        private readonly LifecycleService _lifecycleService;

        public ReportesPage(ReportesViewModel viewModel, LifecycleService lifecycleService)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
            _lifecycleService = lifecycleService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.CargarAsync();
            await _lifecycleService.AddEventAsync("Página Reportes abierta");
        }

        protected override async void OnDisappearing()
        {
            base.OnDisappearing();
            await _lifecycleService.AddEventAsync("Página Reportes cerrada");
        }
    }
}