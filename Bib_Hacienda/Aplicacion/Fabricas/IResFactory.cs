using Bib_Hacienda.Dominio;

namespace Bib_Hacienda.Aplicacion
{
    // Patrón Factory Method (Reto 2, P-01/P-02).
    // Antes: IFabricaRes. Se renombra para que el código coincida con el
    // diagrama TO-BE y la tabla de cambio estructural (E-07) del documento
    // de sustentación. Mismo contrato, mismo comportamiento observable.
    public interface IResFactory
    {
        Res Crear(string nombre, uint peso, ushort edad);
    }
}
