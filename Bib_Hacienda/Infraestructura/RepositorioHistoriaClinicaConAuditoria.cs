using System;
using System.Collections.Generic;
using System.IO;
using Bib_Hacienda.Aplicacion;
using Bib_Hacienda.Dominio;

namespace Bib_Hacienda.Infraestructura
{
    // Patrón Decorator (Reto 2, P-04 / SC-3, E-04).
    // Envuelve un IRepositorioHistoriaClinica (normalmente
    // RepositorioHistoriaClinicaTexto) para registrar, en cada Agregar,
    // quién y cuándo guardó el evento clínico, sin que ServicioHistoriaClinica
    // ni el repositorio base se enteren de la auditoría (LSP: sigue siendo,
    // desde afuera, un IRepositorioHistoriaClinica cualquiera).
    public class RepositorioHistoriaClinicaConAuditoria : IRepositorioHistoriaClinica
    {
        private const string ArchivoAuditoria = "AuditoriaHistoriaClinica.txt";

        private readonly IRepositorioHistoriaClinica decorando;
        private readonly IContextoEjecucion contextoEjecucion;

        public RepositorioHistoriaClinicaConAuditoria(IRepositorioHistoriaClinica decorando, IContextoEjecucion contextoEjecucion)
        {
            this.decorando = decorando;
            this.contextoEjecucion = contextoEjecucion;
        }

        // Consulta: se delega sin cambios. La auditoría de SC-3 exige
        // trazabilidad de quién ESCRIBE una entrada clínica, no de quién la
        // consulta (ver ficha del patrón, entregable 3.3).
        public List<EventoClinico> ObtenerPorRes(string potreroId, string nombreRes)
        {
            return decorando.ObtenerPorRes(potreroId, nombreRes);
        }

        public void Agregar(EventoClinico evento)
        {
            decorando.Agregar(evento);
            RegistrarAuditoria(evento);
        }

        private void RegistrarAuditoria(EventoClinico evento)
        {
            try
            {
                string usuario = contextoEjecucion.ObtenerUsuarioActual();
                string linea = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}|{usuario}|{evento.PotreroId}|{evento.NombreRes}|{evento.TipoEvento}";

                string ruta = Path.Combine(DirectorioDatos.ObtenerRuta(), ArchivoAuditoria);
                File.AppendAllLines(ruta, new[] { linea });
            }
            catch
            {
                // La auditoría es un efecto secundario de trazabilidad: si
                // falla (p. ej. sin permisos de escritura), no debe impedir
                // que el evento clínico, ya guardado por "decorando", se
                // reporte como exitoso al llamador (comportamiento observable
                // de RegistrarEvento no cambia).
            }
        }
    }
}
