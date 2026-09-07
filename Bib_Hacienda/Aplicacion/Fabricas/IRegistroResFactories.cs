namespace Bib_Hacienda.Aplicacion
{
    // Antes: IRegistroFabricasRes. Renombrado junto con IFabricaRes (ver
    // IResFactory.cs) para alinear código y diagrama TO-BE.
    public interface IRegistroResFactories
    {
        IResFactory ObtenerFabrica(string tipo);
    }
}
