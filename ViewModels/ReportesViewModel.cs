using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarWashFacil.ViewModels
{
    public partial class ReportesViewModel : BaseViewModel
    {
        private readonly LavadoService _lavadoService;
        private readonly CajaService _cajaService;

        public ObservableCollection<Lavado> LavadosMes { get; } = new();

        [ObservableProperty]
        private int totalLavadosMes;

        [ObservableProperty]
        private decimal ingresosMes;

        [ObservableProperty]
        private decimal gastosMes;

        [ObservableProperty]
        private decimal balanceMes;

        public ReportesViewModel(LavadoService lavadoService, CajaService cajaService)
        {
            _lavadoService = lavadoService;
            _cajaService = cajaService;
            Titulo = "Reportes";
        }

        public async Task CargarAsync()
        {
            LavadosMes.Clear();

            var all = await _lavadoService.GetAllAsync();
            var now = DateTime.Now;

            var delMes = all.Where(x => x.Fecha.Month == now.Month && x.Fecha.Year == now.Year)
                            .OrderByDescending(x => x.Fecha)
                            .ToList();

            foreach (var item in delMes)
                LavadosMes.Add(item);

            TotalLavadosMes = await _lavadoService.TotalLavadosMesAsync();
            IngresosMes = await _cajaService.TotalIngresosMesAsync();
            GastosMes = await _cajaService.TotalGastosMesAsync();
            BalanceMes = await _cajaService.BalanceMesAsync();
        }
    }
}
