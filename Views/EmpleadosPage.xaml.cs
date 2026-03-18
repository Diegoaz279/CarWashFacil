namespace CarWashFacil.Views
{
    public partial class EmpleadosPage : ContentPage
    {
        private readonly EmpleadosViewModel _viewModel;
        private readonly LifecycleService _lifecycleService;

        public EmpleadosPage(EmpleadosViewModel viewModel, LifecycleService lifecycleService)
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
                _ = _lifecycleService.AddEventSafeAsync("Página Empleados abierta");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error EmpleadosPage OnAppearing: {ex}");
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _ = _lifecycleService.AddEventSafeAsync("Página Empleados cerrada");
        }
    }
}