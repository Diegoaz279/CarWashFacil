using Microsoft.Maui.Controls;
using System.Linq;

namespace CarWashFacil
{
    public class AppShell : Shell
    {
        private readonly LoginPage _loginPage;

        public AppShell(
            HomePage homePage,
            LavadosPage lavadosPage,
            EmpleadosPage empleadosPage,
            CajaPage cajaPage,
            ReportesPage reportesPage,
            EstadosPage estadosPage,
            LoginPage loginPage)
        {
            _loginPage = loginPage;

            this.FlyoutBehavior = FlyoutBehavior.Flyout;

            this.FlyoutHeader = new VerticalStackLayout
            {
                Padding = new Thickness(20, 40, 20, 20),
                BackgroundColor = Color.FromArgb("#111827"),
                Children =
                {
                    new Label
                    {
                        Text = "CarWash Fácil",
                        TextColor = Colors.White,
                        FontSize = 24,
                        FontAttributes = FontAttributes.Bold
                    },
                    new Label
                    {
                        Text = "Panel principal",
                        TextColor = Color.FromArgb("#D1D5DB"),
                        FontSize = 14
                    }
                }
            };

            var inicioItem = CrearItem("Inicio", homePage);
            var lavadosItem = CrearItem("Lavados", lavadosPage);
            var empleadosItem = CrearItem("Empleados", empleadosPage);
            var cajaItem = CrearItem("Caja", cajaPage);
            var reportesItem = CrearItem("Reportes", reportesPage);
            var estadosItem = CrearItem("Estados", estadosPage);

            // Opción de cerrar sesión como página del menú
            var cerrarSesionItem = new FlyoutItem
            {
                Title = "Cerrar sesión",
                FlyoutDisplayOptions = FlyoutDisplayOptions.AsSingleItem
            };

            var cerrarSesionContent = new ShellContent
            {
                Title = "Cerrar sesión",
                Content = new ContentPage
                {
                    Title = "Cerrar sesión",
                    Content = new VerticalStackLayout
                    {
                        Padding = 30,
                        Spacing = 20,
                        VerticalOptions = LayoutOptions.Center,
                        Children =
                        {
                            new Label
                            {
                                Text = "¿Deseas cerrar la sesión actual?",
                                FontSize = 22,
                                FontAttributes = FontAttributes.Bold,
                                HorizontalTextAlignment = TextAlignment.Center,
                                TextColor = Color.FromArgb("#111827")
                            },
                            new Button
                            {
                                Text = "Cerrar sesión",
                                BackgroundColor = Color.FromArgb("#DC2626"),
                                TextColor = Colors.White,
                                CornerRadius = 14,
                                HeightRequest = 50,
                                Command = new Command(async () =>
                                {
                                    bool confirmar = await Application.Current!.Windows[0].Page!
                                        .DisplayAlert("Cerrar sesión",
                                                      "¿Deseas cerrar la sesión actual?",
                                                      "Sí",
                                                      "No");

                                    if (!confirmar)
                                        return;

                                    Application.Current.Windows[0].Page = new NavigationPage(_loginPage);
                                })
                            }
                        }
                    }
                }
            };

            cerrarSesionItem.Items.Add(cerrarSesionContent);

            this.Items.Add(inicioItem);
            this.Items.Add(lavadosItem);
            this.Items.Add(empleadosItem);
            this.Items.Add(cajaItem);
            this.Items.Add(reportesItem);
            this.Items.Add(estadosItem);
            this.Items.Add(cerrarSesionItem);

            this.CurrentItem = inicioItem;
        }

        private FlyoutItem CrearItem(string titulo, Page pagina)
        {
            var flyoutItem = new FlyoutItem
            {
                Title = titulo,
                Route = titulo,
                FlyoutDisplayOptions = FlyoutDisplayOptions.AsSingleItem
            };

            var shellContent = new ShellContent
            {
                Title = titulo,
                Route = titulo,
                Content = pagina
            };

            flyoutItem.Items.Add(shellContent);

            return flyoutItem;
        }
    }
}