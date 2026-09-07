using Bib_Hacienda.Dominio;

namespace Bib_Hacienda.Aplicacion
{
    // Antes: FabricaNovillo. Ver nota de renombre en ResFactoryTernero.cs.
    public class ResFactoryNovillo : IResFactory
    {
        public Res Crear(string nombre, uint peso, ushort edad)
        {
            return new Novillo(nombre, peso, edad);
        }
    }
}
