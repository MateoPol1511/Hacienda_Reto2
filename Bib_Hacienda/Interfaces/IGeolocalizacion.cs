using Bib_Hacienda.Dominio;
using System;
using System.Collections.Generic;

namespace Bib_Hacienda.Interfaces
{
    // SC-2: contrato de aplicación para registrar y consultar las
    // lecturas de posición del chip GPS de una Res. Mismo criterio que
    // IHistoriaClinica/IVacunacion (H-14): la operación de comando
    // retorna ResultadoOperacion en vez de string o excepciones como
    // control de flujo.
    public interface IGeolocalizacion
    {
        // Registra una nueva lectura de posición para la res indicada.
        ResultadoOperacion RegistrarUbicacion(string potreroId, string nombreRes, string codigoChip, DateTime fecha, double latitud, double longitud);

        // Consulta cronológica de las lecturas de posición de una res.
        List<RegistroGeolocalizacion> ConsultarUbicaciones(string potreroId, string nombreRes);
    }
}
