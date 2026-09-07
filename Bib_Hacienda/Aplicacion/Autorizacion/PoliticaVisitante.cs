namespace Bib_Hacienda.Aplicacion
{
    // Antes: rama "else if (rol == \"visitante\")" de ProveedorPermisosPorRol.
    // Mismo comportamiento: el visitante solo puede consultar/listar.
    public class PoliticaVisitante : IPoliticaAutorizacion
    {
        public bool PuedeEjecutar(string operacion)
        {
            return operacion.Contains("Consultar") || operacion.Contains("Listar");
        }
    }
}
