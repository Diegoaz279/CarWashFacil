🚗 CarWash Fácil

CarWash Fácil es una aplicación móvil desarrollada en .NET MAUI que permite gestionar de manera eficiente las operaciones de un carwash, incluyendo el control de lavados, empleados, ventas y reportes.

La aplicación fue diseñada con una interfaz intuitiva y moderna, enfocada en facilitar el uso diario del sistema y mejorar la organización del negocio.

🚀 Características principales

🔐 Sistema de login con validación de errores

🚘 Registro y gestión de lavados de vehículos

👨‍🔧 Administración de empleados

💰 Control de ventas y caja

📊 Generación de reportes diarios

📅 Seguimiento de estados del sistema (logs)

🔄 Actualización automática de datos en tiempo real

✏️ Edición de precios de servicios

⚠️ Manejo de errores y validaciones

🧠 Arquitectura

El proyecto implementa el patrón MVVM (Model - View - ViewModel) utilizando el MVVM Toolkit, lo que permite una mejor organización del código, separación de responsabilidades y escalabilidad.

Además, se aplica inyección de dependencias (Dependency Injection) para desacoplar componentes y mejorar la mantenibilidad.

🔄 Manejo de estados

La aplicación incluye control completo del ciclo de vida:

Estados de aplicación (OnStart, OnSleep, OnResume)

Estados de ventana (minimizar, reanudar)

Estados de página (OnAppearing, OnDisappearing)

Todos los eventos son registrados y almacenados en SQLite para su visualización.

💾 Base de datos

Se utiliza SQLite como base de datos local para almacenar:

Lavados

Empleados

Eventos del sistema

🛠️ Tecnologías utilizadas

.NET MAUI

C#

MVVM Toolkit

SQLite

Dependency Injection

🎯 Objetivo

Digitalizar y optimizar la gestión de un carwash, reemplazando procesos manuales por un sistema moderno, eficiente y fácil de usar.

📌 Estado del proyecto

✅ Funcional
✅ Listo para uso académico
🚀 Escalable para futuras mejoras

👨‍💻 Autor

Diego Castillo
Estudiante de Ingeniería de Software
