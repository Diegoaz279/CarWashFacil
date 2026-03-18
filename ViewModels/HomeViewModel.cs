namespace CarWashFacil.ViewModels
{
    public partial class HomeViewModel : BaseViewModel
    {
        private readonly LavadoService _lavadoService;
        private readonly CajaService _cajaService;
        private readonly AppStateService _appStateService;

        [ObservableProperty]
        private int lavadosHoy;

        [ObservableProperty]
        private decimal ingresosHoy;

        [ObservableProperty]
        private decimal gastosHoy;

        [ObservableProperty]
        private decimal balanceHoy;

        public HomeViewModel(
            LavadoService lavadoService,
            CajaService cajaService,
            AppStateService appStateService)
        {
            _lavadoService = lavadoService;
            _cajaService = cajaService;
            _appStateService = appStateService;

            Titulo = "Inicio";

            _appStateService.DatosActualizados += async () =>
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await CargarAsync();
                });
            };
        }

        public async Task CargarAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;

                LavadosHoy = await _lavadoService.TotalLavadosHoyAsync();
                IngresosHoy = await _cajaService.TotalIngresosHoyAsync();
                GastosHoy = await _cajaService.TotalGastosHoyAsync();
                BalanceHoy = await _cajaService.BalanceHoyAsync();
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task IrALavados()
        {
            if (Shell.Current != null)
                await Shell.Current.GoToAsync("//Lavados");
        }

        [RelayCommand]
        private async Task IrACaja()
        {
            if (Shell.Current != null)
                await Shell.Current.GoToAsync("//Caja");
        }
    }
}