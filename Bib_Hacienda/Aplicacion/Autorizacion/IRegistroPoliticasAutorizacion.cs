namespace Bib_Hacienda.Aplicacion
{
    // Resuelve, en tiempo de ejecución, qué política de autorización
    // (Strategy) corresponde a un rol. Mismo estilo que
    // IRegistroResFactories (Factory Method, P-02): agregar un rol nuevo es
    // registrar una política más en el composition root (Program.cs), sin
    // tocar ServicioAutenticacion (OCP).
    public interface IRegistroPoliticasAutorizacion
    {
        IPoliticaAutorizacion Obtener(string rol);
    }
}
