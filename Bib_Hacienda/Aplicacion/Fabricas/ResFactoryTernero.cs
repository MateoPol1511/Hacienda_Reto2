using Bib_Hacienda.Dominio;

namespace Bib_Hacienda.Aplicacion
{
    // Antes: FabricaTernero. Renombrado (Reto 2) para coincidir con el
    // diagrama TO-BE (E-07): una fábrica concreta por tipo de Res, sin
    // atributos ni comportamiento adicional al contrato de IResFactory.
    public class ResFactoryTernero : IResFactory
    {
        public Res Crear(string nombre, uint peso, ushort edad)
        {
            return new Ternero(nombre, peso, edad);
        }
    }
}
