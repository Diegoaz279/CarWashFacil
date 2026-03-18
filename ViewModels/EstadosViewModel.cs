using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarWashFacil.ViewModels
{
    public partial class EstadosViewModel : BaseViewModel
    {
        private readonly LifecycleService _lifecycleService;

        public ObservableCollection<EventoSistema> Eventos { get; } = new();

        public EstadosViewModel(LifecycleService lifecycleService)
        {
            _lifecycleService = lifecycleService;
            Titulo = "Estados";
        }

        public async Task CargarAsync()
        {
            Eventos.Clear();
            var data = await _lifecycleService.GetEventsAsync();

            foreach (var item in data)
                Eventos.Add(item);
        }

        [RelayCommand]
        private async Task Limpiar()
        {
            await _lifecycleService.ClearAsync();
            await CargarAsync();
        }
    }
}