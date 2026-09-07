using Microsoft.AspNetCore.Mvc;
using Bib_Hacienda.Aplicacion;
using Bib_Hacienda.Dominio;

namespace p_mvcHacienda.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly ServicioAutenticacion _servicioAutenticacion;

        public UsuarioController(ServicioAutenticacion servicioAutenticacion)
        {
            _servicioAutenticacion = servicioAutenticacion;
        }

        // GET: Usuario/Index - Listar todos los usuarios
        [HttpGet]
        public ActionResult Index()
        {
            var usuarios = _servicioAutenticacion.listar_usuarios();
            var estadisticas = _servicioAutenticacion.ObtenerEstadisticas();

            ViewBag.Estadisticas = estadisticas;

            return View(usuarios);
        }

        // Roles disponibles para el formulario de creación. Mismas claves
        // con las que Program.cs registra las políticas de autorización
        // (Strategy, P-03): admin/empleado/visitante.
        private static readonly string[] RolesDisponibles = { "admin", "empleado", "visitante" };

        // GET: Usuario/Create - Mostrar formulario de creación
        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.RolesDisponibles = RolesDisponibles;
            return View();
        }

        // POST: Usuario/Create - Procesar creación de usuario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(string nombre, string contrasena, string rol)
        {
            ViewBag.RolesDisponibles = RolesDisponibles;
            try
            {
                if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(contrasena) || string.IsNullOrWhiteSpace(rol))
                {
                    ViewBag.Mensaje = "❌ Todos los campos son requeridos";
                    ViewBag.TipoMensaje = "danger";
                    return View();
                }

                var resultado = _servicioAutenticacion.crear_usuario(nombre, contrasena, rol);

                if (resultado.Exito)
                {
                    TempData["Mensaje"] = resultado.Mensaje;
                    TempData["TipoMensaje"] = "success";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ViewBag.Mensaje = resultado.Mensaje;
                    ViewBag.TipoMensaje = "danger";
                    return View();
                }
            }
            catch (Exception ex)
            {
                ViewBag.Mensaje = $"❌ Error: {ex.Message}";
                ViewBag.TipoMensaje = "danger";
                return View();
            }
        }

        // Herramienta de DIAGNÓSTICO para el Strategy de autorización (P-03).
        // Sin [Authorize]: permite probar AutorizarOperacion con cualquier
        // usuario/rol a voluntad, independiente de quién esté logueado, para
        // poder capturar evidencia de los casos C-09/C-10/C-11. No agrega
        // reglas de negocio nuevas: solo llama a
        // ServicioAutenticacion.AutorizarOperacion, que ya existe y ya está
        // validado (Strategy + Null Object).

        // GET: Usuario/Autorizar - Mostrar formulario de diagnóstico
        [HttpGet]
        public ActionResult Autorizar()
        {
            ViewBag.Usuarios = _servicioAutenticacion.listar_usuarios();
            return View();
        }

        // POST: Usuario/Autorizar - Ejecutar AutorizarOperacion y mostrar el resultado
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Autorizar(string nombreUsuario, string operacion)
        {
            ViewBag.Usuarios = _servicioAutenticacion.listar_usuarios();
            try
            {
                if (string.IsNullOrWhiteSpace(nombreUsuario) || string.IsNullOrWhiteSpace(operacion))
                {
                    ViewBag.Mensaje = "❌ Debes seleccionar un usuario e indicar una operación";
                    ViewBag.TipoMensaje = "danger";
                    return View();
                }

                // AutorizarOperacion vuelve a buscar por Usuario.Nombre en el
                // repositorio internamente, así que se le pasa un usuario que
                // YA existe (buscar_usuario), no uno construido a mano.
                Usuario usuario = _servicioAutenticacion.buscar_usuario(nombreUsuario);
                ResultadoOperacion resultado = _servicioAutenticacion.AutorizarOperacion(usuario, operacion);

                ViewBag.Mensaje = resultado.Mensaje;
                ViewBag.TipoMensaje = resultado.Exito ? "success" : "danger";
                ViewBag.NombreUsuario = nombreUsuario;
                ViewBag.Operacion = operacion;
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Mensaje = $"❌ Error: {ex.Message}";
                ViewBag.TipoMensaje = "danger";
                return View();
            }
        }
    }
}
