namespace CarWashFacil.Services
{
    public class AppStateService
    {
        public event Action? DatosActualizados;

        public void NotificarCambios()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                DatosActualizados?.Invoke();
            });
        }
    }
}