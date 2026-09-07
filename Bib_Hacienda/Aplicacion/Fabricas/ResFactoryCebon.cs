using Bib_Hacienda.Dominio;

namespace Bib_Hacienda.Aplicacion
{
    // Antes: FabricaCebon. Ver nota de renombre en ResFactoryTernero.cs.
    public class ResFactoryCebon : IResFactory
    {
        public Res Crear(string nombre, uint peso, ushort edad)
        {
            return new Cebon(nombre, peso, edad);
        }
    }
}
