using System;
using Bib_Hacienda.Dominio;

namespace Bib_Hacienda.Infraestructura
{
    // Conserva el formato de Usuarios.txt del AS-IS
    // (PersistenciaService.GuardarUsuarios / CargarUsuarios): "Nombre|Contrasena".
    //
    // Diferencia de tipos frente al AS-IS: Usuario.Contrasena pasó a
    // Usuario.ContrasenaHash (ver Usuario.cs del TO-BE): el dominio ya no
    // transporta la contraseña en texto plano, sino el hash calculado por
    // IServicioHash (Infraestructura, fuera de este bloque). Este
    // serializador no calcula ni verifica el hash: solo vuelca/lee el valor
    // que ya trae el objeto Usuario, igual que el AS-IS volcaba/leía
    // Contrasena tal cual.
    //
    // Corrección de inconsistencia (Strategy, P-03): se agrega un tercer
    // campo "Rol" ("Nombre|ContrasenaHash|Rol") porque la autorización ahora
    // se resuelve con Usuario.Rol, no con Usuario.Nombre. Se mantiene
    // compatibilidad con líneas heredadas de solo 2 campos, asignándoles
    // "visitante" (el rol de menor privilegio) para no dejarlas sin rol.
    public class SerializadorUsuario : ISerializador<Usuario>
    {
        private const string RolPorDefectoLegado = "visitante";

        public string Serializar(Usuario entidad)
        {
            if (entidad == null)
            {
                throw new ArgumentNullException(nameof(entidad));
            }

            return $"{entidad.Nombre}|{entidad.ContrasenaHash}|{entidad.Rol}";
        }

        public Usuario Deserializar(string linea)
        {
            if (string.IsNullOrWhiteSpace(linea))
            {
                throw new ArgumentException("La línea de usuario a deserializar no puede estar vacía.", nameof(linea));
            }

            var partes = linea.Split('|');
            if (partes.Length < 2)
            {
                throw new FormatException($"Línea de usuario con formato inválido: '{linea}'");
            }

            string nombre = partes[0];
            string contrasenaHash = partes[1];
            string rol = partes.Length >= 3 && !string.IsNullOrWhiteSpace(partes[2])
                ? partes[2]
                : RolPorDefectoLegado;

            return new Usuario(nombre, contrasenaHash, rol);
        }
    }
}
