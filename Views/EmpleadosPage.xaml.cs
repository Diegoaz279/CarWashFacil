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

        // ? AGREGAR ESTE MÉTODO
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.CargarAsync();
        }
    }
}