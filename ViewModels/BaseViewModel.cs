using CommunityToolkit.Mvvm.ComponentModel;

namespace CarWashFacil.ViewModels
{
    /// <summary>
    /// VIEWMODEL BASE: Clase base para todos los ViewModels de la aplicación.
    /// 
    /// CICLO DE VIDA MVVM:
    /// - ObservableObject: Implementa INotifyPropertyChanged automáticamente
    /// - Los ViewModels se crean mediante DI (Inyección de Dependencias) con Scoped/Singleton/Transient
    /// - Se destruyen cuando la página se cierra (si son Transient) o persisten (Singleton)
    /// </summary>
    public partial class BaseViewModel : ObservableObject
    {
        /// <summary>
        /// IsBusy: Controla visibilidad de ActivityIndicator en la UI.
        /// Grid.Row/Column: Se usa en XAML para posicionar el loading en cualquier celda del Grid.
        /// </summary>
        [ObservableProperty]
        private bool isBusy;

        /// <summary>
        /// Titulo: Se muestra en el Header de la página (NavigationPage/Shell).
        /// </summary>
        [ObservableProperty]
        private string titulo = string.Empty;
    }
}