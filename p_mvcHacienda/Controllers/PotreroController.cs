using Microsoft.AspNetCore.Mvc;
using Bib_Hacienda.Aplicacion;

namespace p_mvcHacienda.Controllers
{
    public class PotreroController : Controller
    {
        //Atributos
        private readonly ServicioPotreros _servicioPotreros;
        private readonly ServicioAutenticacion _servicioAutenticacion;

        //Inyección de dependencias del servicio
        public PotreroController(ServicioPotreros servicioPotreros, ServicioAutenticacion servicioAutenticacion)
        {
            _servicioPotreros = servicioPotreros;
            _servicioAutenticacion = servicioAutenticacion;
        }

        // Solo admin y empleado pueden crear potreros (visitante NO).
        // Reutiliza el mismo Strategy de autorización (IPoliticaAutorizacion)
        // que ya existe en el proyecto: PoliticaVisitante.PuedeEjecutar solo
        // permite operaciones que contengan "Consultar" o "Listar", por lo
        // que "CrearPotrero" ya queda denegada para ese rol sin tocar las
        // políticas; PoliticaAdministrador y PoliticaEmpleado la permiten.
        private bool PuedeCrearPotrero(out string mensaje)
        {
            var nombreUsuario = User?.Identity?.Name;

            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                mensaje = "Debe iniciar sesión para crear un potrero";
                return false;
            }

            var usuario = _servicioAutenticacion.buscar_usuario(nombreUsuario);
            var resultado = _servicioAutenticacion.AutorizarOperacion(usuario, "CrearPotrero");

            mensaje = resultado.Mensaje;
            return resultado.Exito;
        }

        // GET
        [HttpGet]

        //Mostrar la lista de potreros y estadisticas
        public ActionResult Index()
        {
            var potreros = _servicioPotreros.ObtenerTodosLosPotreros();
            var estadisticas = _servicioPotreros.ObtenerEstadisticas();
      
            ViewBag.Estadisticas = estadisticas;
            // Controla si la vista muestra el botón "Crear Potrero" (solo admin/empleado).
            ViewBag.PuedeCrearPotrero = PuedeCrearPotrero(out _);

            return View(potreros);
        }

        
        // GET: Potrero/Create - Mostrar formulario de creación
        public ActionResult Create()
        {
            if (!PuedeCrearPotrero(out string mensaje))
            {
                TempData["Mensaje"] = mensaje;
                TempData["TipoMensaje"] = "danger";
                return RedirectToAction(nameof(Index));
            }

            return View();
        }

        //Detalles de un potrero
        public ActionResult Details(string id)
        {
            var potrero = _servicioPotreros.ObtenerPotreroPorIdentificacion(id);

            if (potrero == null)
            {
                TempData["Mensaje"] = "Potrero no encontrado";
                TempData["TipoMensaje"] = "danger";
                return RedirectToAction(nameof(Index));
            }

            return View(potrero);
        }

        // POST:
        [HttpPost]

        // Procesar creación de potrero
        // NOTA: Tipo_potrero pasa de "enum l_tipos_potreros" (AS-IS) a "string" en el
        // TO-BE (clave que indexa RegistroResFactories, ver Bib_Hacienda.Dominio.Potrero).
        // El formulario (Views/Potrero/Create.cshtml) sigue posteando el mismo valor de
        // texto que antes, por lo que el binding a string es compatible sin tocar la vista.
        public ActionResult Create(string identificacion, string tipo)
        {
            try
            {
                // Verificación de autorización (solo admin/empleado): se repite
                // aquí además del GET porque un visitante podría intentar
                // postear directamente al endpoint sin pasar por el formulario.
                if (!PuedeCrearPotrero(out string mensajeAutorizacion))
                {
                    TempData["Mensaje"] = mensajeAutorizacion;
                    TempData["TipoMensaje"] = "danger";
                    return RedirectToAction(nameof(Index));
                }

                // Validar entrada
                if (string.IsNullOrWhiteSpace(identificacion))
                {
                    ViewBag.Mensaje = "La identificación no puede estar vacía";
                    ViewBag.TipoMensaje = "danger";
                    return View();
                }

                // Llamar al servicio para crear potrero (persiste internamente vía repositorio)
                var resultado = _servicioPotreros.CrearPotrero(identificacion, tipo);

                if (!resultado.Exito)
                {
                    ViewBag.Mensaje = resultado.Mensaje;
                    ViewBag.TipoMensaje = "danger";
                    return View();
                }

                // Si es exitoso, redirigir con mensaje de éxito
                TempData["Mensaje"] = resultado.Mensaje;
                TempData["TipoMensaje"] = "success";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.Mensaje = $"{ex.Message}";
                ViewBag.TipoMensaje = "danger";
            }
  
            return View();
        }
    }
}
