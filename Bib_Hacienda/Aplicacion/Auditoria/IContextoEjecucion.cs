namespace Bib_Hacienda.Aplicacion
{
    // Reto 2 (P-04, Decorator): puerto técnico para que Bib_Hacienda (sin
    // referencia a ASP.NET Core) pueda preguntar "quién ejecuta la
    // operación actual" sin conocer HttpContext/ClaimsPrincipal. La
    // implementación real (basada en IHttpContextAccessor) vive en
    // p_mvcHacienda, igual que el UML aclara para la generación del
    // ClaimsPrincipal en ServicioAutenticacion (ver nota en ese archivo).
    public interface IContextoEjecucion
    {
        string ObtenerUsuarioActual();
    }
}
