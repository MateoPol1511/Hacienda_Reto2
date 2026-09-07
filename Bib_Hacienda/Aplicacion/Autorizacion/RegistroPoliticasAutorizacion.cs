using System.Collections.Generic;

namespace Bib_Hacienda.Aplicacion
{
    // Antes: el if/else de ProveedorPermisosPorRol.TienePermiso indexaba la
    // regla directamente por el string de rol. Aquí el registro solo resuelve
    // QUÉ política aplica; la regla de negocio de cada rol vive en su propia
    // clase (PoliticaAdministrador/Empleado/Visitante). Mismo comportamiento
    // observable que el AS-IS: un rol no registrado no tiene permisos
    // (PoliticaSinPermisos), en vez de lanzar una excepción.
    public class RegistroPoliticasAutorizacion : IRegistroPoliticasAutorizacion
    {
        private readonly Dictionary<string, IPoliticaAutorizacion> politicas;
        private readonly IPoliticaAutorizacion politicaPorDefecto = new PoliticaSinPermisos();

        public RegistroPoliticasAutorizacion(Dictionary<string, IPoliticaAutorizacion> politicas)
        {
            this.politicas = politicas;
        }

        public IPoliticaAutorizacion Obtener(string rol)
        {
            if (rol == null || !politicas.TryGetValue(rol, out IPoliticaAutorizacion politica))
            {
                return politicaPorDefecto;
            }
            return politica;
        }
    }
}
