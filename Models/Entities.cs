using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarWashFacil.Models
{
    [Table("Usuarios")]
    public class Usuario
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Unique]
        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }

    [Table("Empleados")]
    public class Empleado
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;

        [Ignore]
        public string NombreCompleto => $"{Nombre} {(Activo ? "" : "(Inactivo)")}";
    }

    [Table("Lavados")]
    public class Lavado
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Cliente { get; set; } = string.Empty;
        public string Placa { get; set; } = string.Empty;
        public string TipoLavado { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public string Estado { get; set; } = "Pendiente";
        public DateTime Fecha { get; set; } = DateTime.Now;

        public int EmpleadoId { get; set; }
        public string EmpleadoNombre { get; set; } = string.Empty;
    }

    [Table("MovimientosCaja")]
    public class MovimientoCaja
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Tipo { get; set; } = string.Empty; // Ingreso o Gasto
        public string Descripcion { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;
    }

    [Table("EventosSistema")]
    public class EventoSistema
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Mensaje { get; set; } = string.Empty;
        public DateTime Fecha { get; set; } = DateTime.Now;

        [Ignore]
        public string FechaTexto => Fecha.ToString("dd/MM/yyyy hh:mm:ss tt");
    }
}
