namespace Bib_Hacienda.Aplicacion
{
    // Antes: el "return false;" final de ProveedorPermisosPorRol para un rol
    // desconocido no cubierto por los 3 roles del AS-IS. Se explicita como
    // política propia (Null Object) en vez de un caso especial dentro del
    // registro, para que RegistroPoliticasAutorizacion no tenga que decidir
    // lógica de negocio de autorización.
    public class PoliticaSinPermisos : IPoliticaAutorizacion
    {
        public bool PuedeEjecutar(string operacion)
        {
            return false;
        }
    }
}
