using Bib_Hacienda.Aplicacion;
using Microsoft.AspNetCore.Http;

namespace p_mvcHacienda.Infraestructura
{
    // Implementación real de IContextoEjecucion (Reto 2, P-04). Vive aquí y
    // no en Bib_Hacienda porque IHttpContextAccessor/ClaimsPrincipal son
    // tipos de ASP.NET Core, no del dominio (mismo criterio que la nota de
    // ServicioAutenticacion sobre dónde debe construirse el ClaimsPrincipal).
    public class ContextoEjecucionHttp : IContextoEjecucion
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public ContextoEjecucionHttp(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public string ObtenerUsuarioActual()
        {
            string nombre = httpContextAccessor.HttpContext?.User?.Identity?.Name;
            return string.IsNullOrWhiteSpace(nombre) ? "desconocido" : nombre;
        }
    }
}
