using Bib_Hacienda.Aplicacion;
using Bib_Hacienda.Dominio;
using Bib_Hacienda.Dominio.Eventos;
using Bib_Hacienda.Dominio.Validacion;
using Bib_Hacienda.Infraestructura;
using p_mvcHacienda.Infraestructura;

namespace p_mvcHacienda
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // --- Configuración de Autenticación por Cookies ---
            builder.Services.AddAuthentication("CookieAuth")
                .AddCookie("CookieAuth", options =>
                {
                    options.Cookie.Name = "HaciendaSoft.Auth";
                    options.LoginPath = "/Account/Login"; // Página de login
                    options.AccessDeniedPath = "/Account/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Duración de la sesión
                });

            // Agregar HttpContextAccessor
            builder.Services.AddHttpContextAccessor();

            // ==================================================================
            // COMPOSITION ROOT (3C-2B)
            // Arquitectura respetada: Controller -> Servicio de Aplicación ->
            // Interfaz -> Infraestructura (ver UML TO-BE). Todo se registra
            // como Scoped (una instancia por request HTTP):
            // - Los repositorios *Texto leen/escriben archivo en cada llamada,
            //   sin estado en memoria que deba sobrevivir entre requests (a
            //   diferencia del AS-IS, que cargaba todo en un "Hacienda"
            //   Singleton al arrancar).
            // - Los Publisher* (PublisherPesoMin, PublisherVacunaVencida, etc.)
            //   acumulan suscriptores en cada llamada a sus métodos
            //   (evt_x += ...). Si se registraran como Singleton, esas
            //   suscripciones se acumularían entre requests y duplicarían
            //   mensajes; Scoped evita ese problema.
            // ==================================================================

            // --- Infraestructura: serializadores de texto ---
            // ISerializador<T> se mantiene como ÚNICA interfaz genérica
            // (no se crean cinco interfaces distintas).
            builder.Services.AddScoped<ISerializador<Potrero>, SerializadorPotrero>();
            builder.Services.AddScoped<ISerializador<Res>, SerializadorRes>();
            builder.Services.AddScoped<ISerializador<Usuario>, SerializadorUsuario>();
            builder.Services.AddScoped<ISerializador<Vacuna>, SerializadorVacuna>();
            builder.Services.AddScoped<ISerializador<Venta>, SerializadorVenta>();
            // SC-3: serializador de eventos de historia clínica (HistoriaClinica.txt).
            builder.Services.AddScoped<ISerializador<EventoClinico>, SerializadorEventoClinico>();
            // SC-2: serializador de lecturas de geolocalización (Geolocalizacion.txt).
            builder.Services.AddScoped<ISerializador<RegistroGeolocalizacion>, SerializadorRegistroGeolocalizacion>();

            // --- Dominio: validadores (ISP, uno por entidad) ---
            builder.Services.AddScoped<IValidadorPotrero, ValidadorPotrero>();
            builder.Services.AddScoped<IValidadorRes, ValidadorRes>();
            builder.Services.AddScoped<IValidadorVacuna, ValidadorVacuna>();
            builder.Services.AddScoped<IValidadorVenta, ValidadorVenta>();
            // SC-3: validador de eventos de historia clínica.
            builder.Services.AddScoped<IValidadorEventoClinico, ValidadorEventoClinico>();
            // SC-2: validador de lecturas de geolocalización.
            builder.Services.AddScoped<IValidadorRegistroGeolocalizacion, ValidadorRegistroGeolocalizacion>();

            // --- Infraestructura: repositorios (uno por entidad, DIP) ---
            builder.Services.AddScoped<IRepositorioPotreros, RepositorioPotrerosTexto>();
            builder.Services.AddScoped<IRepositorioUsuarios, RepositorioUsuariosTexto>();
            builder.Services.AddScoped<IRepositorioVacunas, RepositorioVacunasTexto>();
            builder.Services.AddScoped<IRepositorioVentas, RepositorioVentasTexto>();

            // Reto 2 (P-04, Decorator): IContextoEjecucion es el puerto que
            // usa el decorator para saber "quién" guarda un evento clínico;
            // la implementación real depende de IHttpContextAccessor (ver
            // AddHttpContextAccessor() más arriba).
            builder.Services.AddScoped<IContextoEjecucion, ContextoEjecucionHttp>();

            // SC-3: repositorio base de historia clínica (uno por res, ver
            // EventoClinico), envuelto por el Decorator de auditoría. Los
            // servicios siguen dependiendo solo de IRepositorioHistoriaClinica
            // y no conocen la envoltura (ver diagrama TO-BE, "Composición final").
            builder.Services.AddScoped<RepositorioHistoriaClinicaTexto>();
            builder.Services.AddScoped<IRepositorioHistoriaClinica>(sp =>
                new RepositorioHistoriaClinicaConAuditoria(
                    sp.GetRequiredService<RepositorioHistoriaClinicaTexto>(),
                    sp.GetRequiredService<IContextoEjecucion>()));

            // SC-2 (P-04, Decorator): repositorio base de geolocalización (una
            // lectura por chip/res, ver RegistroGeolocalizacion), envuelto por
            // el MISMO Decorator de auditoría que envuelve la historia
            // clínica (misma clase de comportamiento, repositorio distinto).
            // ServicioGeolocalizacion solo depende de IRepositorioGeolocalizacion
            // y no conoce la envoltura.
            builder.Services.AddScoped<RepositorioGeolocalizacionTexto>();
            builder.Services.AddScoped<IRepositorioGeolocalizacion>(sp =>
                new RepositorioGeolocalizacionConAuditoria(
                    sp.GetRequiredService<RepositorioGeolocalizacionTexto>(),
                    sp.GetRequiredService<IContextoEjecucion>()));

            // --- Aplicacion: fábricas de vacunas ---
            builder.Services.AddScoped<IFabricaVacunaBacteriana, FabricaVacunaBacteriana>();
            builder.Services.AddScoped<IFabricaVacunaViva, FabricaVacunaViva>();

            // --- Aplicacion: registro de fábricas de Res, indexado por Tipo_potrero ---
            // Claves = mismos valores de texto que el enum l_tipos_potreros del
            // AS-IS (Bib_Hacienda.Clases.Potrero.l_tipos_potreros: ternero,
            // cebon, novillo), que en el TO-BE Potrero.Tipo_potrero pasó de enum
            // a string (ver Potrero.cs y RegistroFabricasRes.cs). No se inventan
            // claves nuevas: son las mismas 3 que ya usaban las Views migradas
            // (Views/Potrero/Create.cshtml).
            builder.Services.AddScoped<IRegistroResFactories>(sp =>
            {
                var fabricas = new Dictionary<string, IResFactory>
                {
                    { "ternero", new ResFactoryTernero() },
                    { "cebon", new ResFactoryCebon() },
                    { "novillo", new ResFactoryNovillo() }
                };
                return new RegistroResFactories(fabricas);
            });

            // --- Aplicacion: autorización ---
            // IServicioHash no tenía implementación concreta (ver Bloque
            // 3C-2A); se agrega ServicioHashSha256 (Infraestructura), la
            // mínima necesaria para que ServicioAutenticacion pueda
            // hashear/verificar contraseñas.
            builder.Services.AddScoped<IServicioHash, ServicioHashSha256>();

            // Reto 2 (P-03, Strategy): antes ProveedorPermisosPorRol resolvía
            // el permiso con un if/else interno; ahora cada rol es una
            // política intercambiable, registrada aquí igual que las fábricas
            // de Res (mismo estilo que IRegistroResFactories más abajo).
            builder.Services.AddScoped<IRegistroPoliticasAutorizacion>(sp =>
            {
                var politicas = new Dictionary<string, IPoliticaAutorizacion>
                {
                    { "admin", new PoliticaAdministrador() },
                    { "empleado", new PoliticaEmpleado() },
                    { "visitante", new PoliticaVisitante() }
                };
                return new RegistroPoliticasAutorizacion(politicas);
            });

            // --- Dominio: publishers de eventos ---
            // PublisherPotreroMitad y PublisherPotreroLleno NO se registran
            // aquí: el UML no le da a ServicioPotreros esa dependencia por
            // constructor, así que CrearPotrero los sigue instanciando con
            // "new" directamente (ver nota en ServicioPotreros.cs).
            builder.Services.AddScoped<PublisherPesoMin>();
            builder.Services.AddScoped<PublisherPesoVenta>();
            builder.Services.AddScoped<PublisherVacunaVencida>();
            builder.Services.AddScoped<PublisherVacunacionCompletada>();

            // --- Aplicacion: servicios de aplicación (los que consumen los Controllers) ---
            builder.Services.AddScoped<ServicioAutenticacion>();
            builder.Services.AddScoped<ServicioPotreros>();
            builder.Services.AddScoped<ServicioAlimentacion>();
            builder.Services.AddScoped<ServicioVentas>();
            builder.Services.AddScoped<ServicioInventarioVacunas>();
            builder.Services.AddScoped<ServicioVacunacion>();
            // SC-3: servicio de aplicación para registrar/consultar la historia clínica de una Res.
            builder.Services.AddScoped<ServicioHistoriaClinica>();
            // SC-2: servicio de aplicación para registrar/consultar la geolocalización (chip GPS) de una Res.
            builder.Services.AddScoped<ServicioGeolocalizacion>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // --- Habilitar Autenticación y Autorización ---
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Account}/{action=Login}/{id?}");

            app.Run();
        }
    }
}
