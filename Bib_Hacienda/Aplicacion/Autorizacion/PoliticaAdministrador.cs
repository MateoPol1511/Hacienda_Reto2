namespace Bib_Hacienda.Aplicacion
{
    // Antes: rama "if (rol == \"admin\")" de ProveedorPermisosPorRol.
    // Mismo comportamiento: el administrador tiene todos los permisos.
    public class PoliticaAdministrador : IPoliticaAutorizacion
    {
        public bool PuedeEjecutar(string operacion)
        {
            return true;
        }
    }
}
