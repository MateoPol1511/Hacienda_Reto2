using Bib_Hacienda.Dominio;
using System.Collections.Generic;

namespace Bib_Hacienda.Aplicacion
{
    // DIP. Ver nota completa en IRepositorioPotreros. SC-2: repositorio
    // dedicado a las lecturas de geolocalización del chip GPS, una por
    // res (identificada por PotreroId + NombreRes, ver
    // RegistroGeolocalizacion).
    public interface IRepositorioGeolocalizacion
    {
        // Lecturas de ubicación de una res concreta, en orden cronológico.
        List<RegistroGeolocalizacion> ObtenerPorRes(string potreroId, string nombreRes);

        // Agrega una nueva lectura de ubicación.
        void Agregar(RegistroGeolocalizacion registro);
    }
}
