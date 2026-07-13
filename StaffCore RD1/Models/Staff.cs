using System;
using System.ComponentModel.DataAnnotations;

namespace StaffCore_RD1.Models
{
    public class Staff
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string Nombre { get; set; }  // Nombre completo

        [Required(ErrorMessage = "La cédula es obligatoria.")]
        [RegularExpression(@"^\d{3}-\d{7}-\d{1}$", ErrorMessage = "El formato de cédula debe ser 001-0000000-0")]
        public string Cedula { get; set; }  // Formato: 001-0000000-0

        [Required(ErrorMessage = "El cargo es obligatorio.")]
        public string Cargo { get; set; }  // Ej: Analista de Sistemas

        [Required(ErrorMessage = "El departamento es obligatorio.")]
        public string Departamento { get; set; }  // Tecnología / RRHH / Finanzas / Operaciones

        [Required(ErrorMessage = "El salario es obligatorio.")]
        [Range(23223, double.MaxValue, ErrorMessage = "Mínimo RD$23,223")]
        public decimal Salario { get; set; }

        [Required(ErrorMessage = "La fecha de ingreso es obligatoria.")]
        [DataType(DataType.Date)]
        public DateTime FechaIngreso { get; set; }

        public bool Activo { get; set; } = true;
    }
}