using System;
using System.Collections.Generic;
using System.IO;
using Bib_Hacienda.Aplicacion;
using Bib_Hacienda.Dominio;

namespace Bib_Hacienda.Infraestructura
{
    // Patrón Decorator (Reto 2, P-04 / SC-2, E-11).
    // Envuelve un IRepositorioGeolocalizacion (normalmente
    // RepositorioGeolocalizacionTexto) para registrar, en cada Agregar,
    // quién y cuándo quedó registrada una lectura del chip GPS, sin que
    // ServicioGeolocalizacion ni el repositorio base se enteren de la
    // auditoría (LSP: sigue siendo, desde afuera, un
    // IRepositorioGeolocalizacion cualquiera).
    //
    // Es la MISMA idea de decorador que RepositorioHistoriaClinicaConAuditoria
    // (mismo contrato de auditoría, mismo IContextoEjecucion), aplicada
    // ahora sobre un repositorio distinto. Se deja así, deliberadamente,
    // como evidencia de que el Decorator generaliza: envolver un
    // repositorio nuevo con la misma clase de comportamiento adicional no
    // exige tocar Decorator alguno ya escrito ni el repositorio que
    // envuelve (OCP).
    public class RepositorioGeolocalizacionConAuditoria : IRepositorioGeolocalizacion
    {
        private const string ArchivoAuditoria = "AuditoriaGeolocalizacion.txt";

        private readonly IRepositorioGeolocalizacion decorando;
        private readonly IContextoEjecucion contextoEjecucion;

        public RepositorioGeolocalizacionConAuditoria(IRepositorioGeolocalizacion decorando, IContextoEjecucion contextoEjecucion)
        {
            this.decorando = decorando;
            this.contextoEjecucion = contextoEjecucion;
        }

        // Consulta: se delega sin cambios. La auditoría de SC-2 exige
        // trazabilidad de quién ESCRIBE una lectura de posición, no de
        // quién la consulta (mismo criterio que la historia clínica).
        public List<RegistroGeolocalizacion> ObtenerPorRes(string potreroId, string nombreRes)
        {
            return decorando.ObtenerPorRes(potreroId, nombreRes);
        }

        public void Agregar(RegistroGeolocalizacion registro)
        {
            decorando.Agregar(registro);
            RegistrarAuditoria(registro);
        }

        private void RegistrarAuditoria(RegistroGeolocalizacion registro)
        {
            try
            {
                string usuario = contextoEjecucion.ObtenerUsuarioActual();
                string linea = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}|{usuario}|{registro.PotreroId}|{registro.NombreRes}|{registro.CodigoChip}";

                string ruta = Path.Combine(DirectorioDatos.ObtenerRuta(), ArchivoAuditoria);
                File.AppendAllLines(ruta, new[] { linea });
            }
            catch
            {
                // La auditoría es un efecto secundario de trazabilidad: si
                // falla (p. ej. sin permisos de escritura), no debe impedir
                // que la lectura de posición, ya guardada por "decorando",
                // se reporte como exitosa al llamador (comportamiento
                // observable de RegistrarUbicacion no cambia).
            }
        }
    }
}
