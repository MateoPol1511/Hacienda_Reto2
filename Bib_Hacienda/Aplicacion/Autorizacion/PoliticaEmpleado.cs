namespace Bib_Hacienda.Aplicacion
{
    // Antes: rama "else if (rol == \"empleado\")" de ProveedorPermisosPorRol.
    // Mismo comportamiento: el empleado puede hacer todo excepto eliminar.
    public class PoliticaEmpleado : IPoliticaAutorizacion
    {
        public bool PuedeEjecutar(string operacion)
        {
            return !operacion.Contains("Eliminar");
        }
    }
}
