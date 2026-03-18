namespace CarWashFacil.Views
{
    public partial class LoginPage : ContentPage
    {
        private readonly LifecycleService _lifecycleService;

        public LoginPage(LoginViewModel viewModel, LifecycleService lifecycleService)
        {
            InitializeComponent();
            BindingContext = viewModel;
            _lifecycleService = lifecycleService;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _ = _lifecycleService.AddEventSafeAsync("Página Login abierta");
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _ = _lifecycleService.AddEventSafeAsync("Página Login cerrada");
        }
    }
}