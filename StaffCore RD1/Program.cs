using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using StaffCore_RD1.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuración de Base de Datos SQL Server
builder.Services.AddDbContext<StaffDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("StaffCore")));

// 2. Configuración de Identity con validación personalizada para las contraseñas
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<StaffDbContext>()
.AddDefaultTokenProviders();

// 3. Configuración de Rutas de redirección para Cookies
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
});

// 4. Agregar soporte para Controladores y Vistas (MVC)
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ¡CRÍTICO! Autenticación siempre antes que Autorización
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Staff}/{action=Index}/{id?}");

// 5. Creación automática de Roles y Usuarios en el arranque
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    // 5.1 Crear los roles si no existen
    string[] roles = { "Administrador", "RRHH", "Viewer" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // 5.2 Crear 4 usuarios de prueba con sus respectivos roles y nombres
    var usuariosPrueba = new (string Email, string Nombre, string Rol, string Password)[]
     {
        // Usamos correos nuevos (.uce) para forzar a la base de datos a crearlos desde cero con el nombre correcto
        ("admin.uce@staffcore.com", "Alejandro (Administrador)", "Administrador", "Clave123!"),
        ("rrhh.uce@staffcore.com", "Laura Rosario", "RRHH", "Clave123!"),
        ("visor1.uce@staffcore.com", "Carlos Mateo", "Viewer", "Clave123!"),
        ("visor2.uce@staffcore.com", "Ana Julia Peña", "Viewer", "Clave123!")
     };

    foreach (var usuario in usuariosPrueba)
    {
        if (await userManager.FindByEmailAsync(usuario.Email) == null)
        {
            var user = new IdentityUser { UserName = usuario.Email, Email = usuario.Email };
            var result = await userManager.CreateAsync(user, usuario.Password);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, usuario.Rol);

                // Aquí se inyecta el nombre real que aparecerá en el Navbar
                await userManager.AddClaimAsync(user, new Claim("NombreCompleto", usuario.Nombre));
            }
        }
    }
}

    app.Run();