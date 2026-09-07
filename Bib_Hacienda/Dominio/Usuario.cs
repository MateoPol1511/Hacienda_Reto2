using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bib_Hacienda.Dominio
{
    // DIP (H-09): ya no almacena la contraseña en texto plano (AS-IS: "Contrasena");
    // guarda el hash calculado por IServicioHash (Infraestructura, fuera de este
    // bloque), inyectado en ServicioAutenticacion. Usuario deja de tener que saber
    // CÓMO se hashea, solo transporta el hash ya calculado.
    //
    // Corrección de inconsistencia (Strategy, P-03): el diseño (UML TO-BE)
    // resuelve la política de autorización POR ROL (RegistroPoliticasAutorizacion
    // .Obtener(rol)), pero Usuario no tenía una propiedad propia para el rol y
    // ServicioAutenticacion terminaba usando usuario.Nombre como si fuera el rol.
    // Se agrega Rol (admin/empleado/visitante, mismas claves con las que
    // Program.cs registra las políticas) para que Usuario transporte su propio
    // rol y la autorización deje de depender del nombre de usuario.
    public class Usuario
    {
        private string nombre;
        private string contrasenaHash;
        private string rol;

        public Usuario(string nombre, string contrasenaHash, string rol)
        {
            this.Nombre = nombre;
            this.ContrasenaHash = contrasenaHash;
            this.Rol = rol;
        }

        public string Nombre { get => nombre; set => nombre = value; }
        public string ContrasenaHash { get => contrasenaHash; set => contrasenaHash = value; }
        public string Rol { get => rol; set => rol = value; }
    }
}
