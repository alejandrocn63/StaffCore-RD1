using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StaffCore_RD1.Models;

namespace StaffCore_RD1.Data
{
    public class StaffDbContext : IdentityDbContext<IdentityUser>
    {
        public StaffDbContext(DbContextOptions<StaffDbContext> options) : base(options)
        {
        }

        public DbSet<Staff> Personal { get; set; }

        protected override void OnModelCreating(ModelBuilder mb)
        {
            // ¡CRÍTICO! Llama al método de la clase base para configurar las tablas de Identity
            base.OnModelCreating(mb);

            // Seed con 2 registros reales (Nombres dominicanos y departamentos distintos)
            mb.Entity<Staff>().HasData(
                new Staff
                {
                    Id = 1,
                    Nombre = "Juan Almonte",
                    Cedula = "001-1234567-8",
                    Cargo = "Soporte Técnico II",
                    Departamento = "Tecnología",
                    Salario = 45000.00m,
                    FechaIngreso = new DateTime(2024, 2, 15),
                    Activo = true
                },
                new Staff
                {
                    Id = 2,
                    Nombre = "María Rodríguez",
                    Cedula = "002-9876543-2",
                    Cargo = "Coordinadora de Personal",
                    Departamento = "Recursos Humanos",
                    Salario = 55000.00m,
                    FechaIngreso = new DateTime(2023, 11, 1),
                    Activo = true
                }
            );
        }
    }
}