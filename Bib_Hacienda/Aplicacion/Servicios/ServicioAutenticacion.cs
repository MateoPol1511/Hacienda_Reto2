using Bib_Hacienda.Dominio;
using Bib_Hacienda.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bib_Hacienda.Aplicacion
{
    // DIP (H-09): ya no guarda una lista estática mutable en memoria; depende
    // de IRepositorioUsuarios. El hashing de contraseñas se delega a
    // IServicioHash.
    //
    // H-16: en el AS-IS existían DOS mecanismos de autenticación -
    // Autenticacion (huérfana, implementaba IAutenticacion pero nunca se
    // instanciaba desde Program.cs/Controllers) y UsuarioService (la que de
    // verdad usa AccountController, con su propia lista estática y su propio
    // ValidateUserAsync). El TO-BE los unifica aquí: única puerta de entrada
    // para validar credenciales, consultar usuarios y autorizar operaciones.
    //
    // INCONSISTENCIA REGISTRADA (autenticación): AccountController (AS-IS)
    // generaba el ClaimsPrincipal de la cookie de sesión a partir de
    // UsuarioService.ValidateUserAsync, que devolvía una tupla
    // (bool, IEnumerable<Claim>). El UML no declara ningún método de
    // ServicioAutenticacion que devuelva Claims — ValidarCredenciales
    // devuelve bool (igual que Autenticacion.ValidarCredenciales del AS-IS).
    // La nota del UML aclara que "la generación del ClaimsPrincipal
    // permanece en AccountController porque ClaimsIdentity/ClaimTypes son
    // tipos de ASP.NET Core, no del dominio", es decir: en un bloque
    // posterior, AccountController deberá construir el ClaimsPrincipal por
    // su cuenta (con ClaimTypes.Name = usuario.Nombre, como hacía
    // UsuarioService) a partir de un ValidarCredenciales()==true + un
    // buscar_usuario(nombre), en vez de recibir los Claims ya armados desde
    // el servicio. No se implementa ese controller en este bloque (queda
    // fuera de "servicios / lógica de aplicación").
    public class ServicioAutenticacion : IAutenticacion
    {
        private IRepositorioUsuarios repositorioUsuarios;
        private IServicioHash servicioHash;
        private IRegistroPoliticasAutorizacion registroPoliticas;

        public ServicioAutenticacion(IRepositorioUsuarios repositorioUsuarios, IServicioHash servicioHash, IRegistroPoliticasAutorizacion registroPoliticas)
        {
            this.repositorioUsuarios = repositorioUsuarios;
            this.servicioHash = servicioHash;
            this.registroPoliticas = registroPoliticas;
        }

        // Roles válidos, mismas claves con las que Program.cs registra las
        // políticas en IRegistroPoliticasAutorizacion (Strategy, P-03).
        private static readonly string[] RolesValidos = { "admin", "empleado", "visitante" };

        // Misma validación que Autenticacion.crear_usuario / UsuarioService.CrearUsuario
        // del AS-IS. La contraseña ya no se guarda en texto plano (H-09): se
        // hashea con IServicioHash antes de construir el Usuario.
        //
        // Corrección de inconsistencia: se agrega el parámetro "rol" para que
        // el usuario nazca con un rol propio (admin/empleado/visitante) en
        // vez de depender de su nombre para resolver permisos.
        public ResultadoOperacion crear_usuario(string nombre, string contrasena, string rol)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nombre))
                {
                    return ResultadoOperacion.Fallo("El nombre del usuario no puede estar vacío");
                }

                if (string.IsNullOrWhiteSpace(contrasena))
                {
                    return ResultadoOperacion.Fallo("La contraseña no puede estar vacía");
                }

                if (string.IsNullOrWhiteSpace(rol) || !RolesValidos.Contains(rol, StringComparer.OrdinalIgnoreCase))
                {
                    return ResultadoOperacion.Fallo($"El rol '{rol}' no es válido. Roles permitidos: {string.Join(", ", RolesValidos)}.");
                }

                if (repositorioUsuarios.ObtenerTodos().Any(u => u.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase)))
                {
                    return ResultadoOperacion.Fallo($"Ya existe un usuario con el nombre '{nombre}'.");
                }

                string contrasenaHash = servicioHash.Hash(contrasena);
                Usuario nuevo_usuario = new Usuario(nombre, contrasenaHash, rol.ToLowerInvariant());
                repositorioUsuarios.Agregar(nuevo_usuario);

                return ResultadoOperacion.Ok($"Usuario '{nombre}' creado exitosamente en el sistema con rol '{rol}'.");
            }
            catch (Exception er)
            {
                return ResultadoOperacion.Fallo("Error inesperado en el método crear_usuario: " + er.Message);
            }
        }

        public List<Usuario> listar_usuarios()
        {
            return new List<Usuario>(repositorioUsuarios.ObtenerTodos());
        }

        public Dictionary<string, object> ObtenerEstadisticas()
        {
            return new Dictionary<string, object>
            {
                { "TotalUsuarios", repositorioUsuarios.ObtenerTodos().Count }
            };
        }

        // Misma lógica que Autenticacion.ValidarCredenciales del AS-IS, salvo
        // que la comparación ya no es de texto plano (u.Contrasena == contrasena)
        // sino vía IServicioHash.Verificar contra el hash guardado (H-09).
        public bool ValidarCredenciales(string nombre, string contrasena)
        {
            Usuario usuario = repositorioUsuarios.ObtenerTodos()
                .FirstOrDefault(u => u.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase));

            if (usuario == null)
            {
                return false;
            }

            // Compatibilidad con datos heredados: algunos archivos de usuarios
            // todavía guardan la contraseña en texto plano. Primero validamos el
            // formato nuevo (hash) y, si no coincide, aceptamos el valor legado.
            if (servicioHash.Verificar(contrasena, usuario.ContrasenaHash))
            {
                return true;
            }

            return string.Equals(contrasena, usuario.ContrasenaHash, StringComparison.Ordinal);
        }

        // Misma lógica que Autenticacion.buscar_usuario del AS-IS.
        public Usuario buscar_usuario(string nombre)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nombre))
                {
                    throw new ArgumentException("El nombre de búsqueda no puede estar vacío.");
                }

                Usuario usuario = repositorioUsuarios.BuscarPorNombre(nombre);

                if (usuario == null)
                {
                    throw new Exception($"No se encontró el usuario '{nombre}'.");
                }

                return usuario;
            }
            catch (Exception er)
            {
                throw new Exception("Error inesperado en el método buscar_usuario: " + er.Message);
            }
        }

        // LSP (ADR-5): antes lanzaba excepción tanto en éxito como en rechazo
        // (Autenticacion.AutorizarOperacion del AS-IS); ahora retorna
        // ResultadoOperacion de forma uniforme, con el MISMO texto descriptivo.
        //
        // Reto 2 (P-03, Strategy): el if/else de admin/empleado/visitante que
        // vivía en ProveedorPermisosPorRol se reemplaza por
        // IRegistroPoliticasAutorizacion + IPoliticaAutorizacion.
        //
        // Corrección de inconsistencia: antes se pasaba usuario.Nombre como si
        // fuera el "rol" (Usuario no tenía una propiedad Rol propia). Ahora
        // Usuario sí expone Rol y se resuelve la política SIEMPRE con el rol
        // del usuario registrado en el repositorio (usuarioRegistrado.Rol), no
        // con el que venga en el parámetro "usuario" ni con su Nombre.
        public ResultadoOperacion AutorizarOperacion(Usuario usuario, string operacion)
        {
            if (usuario == null)
            {
                return ResultadoOperacion.Fallo("✗ Usuario no autenticado. Debe iniciar sesión para realizar operaciones");
            }

            Usuario usuarioRegistrado = repositorioUsuarios.BuscarPorNombre(usuario.Nombre);

            if (usuarioRegistrado == null)
            {
                return ResultadoOperacion.Fallo($"✗ Usuario '{usuario.Nombre}' no está registrado en el sistema");
            }

            IPoliticaAutorizacion politica = registroPoliticas.Obtener(usuarioRegistrado.Rol);
            bool tienePermiso = politica.PuedeEjecutar(operacion);

            if (tienePermiso)
            {
                return ResultadoOperacion.Ok($"✓ Usuario '{usuario.Nombre}' autorizado para ejecutar: {operacion}");
            }
            else
            {
                return ResultadoOperacion.Fallo($"✗ Acceso DENEGADO. Usuario '{usuario.Nombre}' NO tiene permisos para: {operacion}");
            }
        }
    }
}
