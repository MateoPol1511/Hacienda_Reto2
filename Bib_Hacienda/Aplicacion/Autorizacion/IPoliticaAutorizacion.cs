namespace Bib_Hacienda.Aplicacion
{
    // Patrón Strategy (Reto 2, P-03).
    // Antes: el if/else de ProveedorPermisosPorRol comparaba directamente el
    // "rol" contra tres cadenas de texto. Aquí cada regla de autorización se
    // aísla en una implementación concreta e intercambiable en tiempo de
    // ejecución (una por rol), sin que ServicioAutenticacion conozca cuál.
    public interface IPoliticaAutorizacion
    {
        bool PuedeEjecutar(string operacion);
    }
}
