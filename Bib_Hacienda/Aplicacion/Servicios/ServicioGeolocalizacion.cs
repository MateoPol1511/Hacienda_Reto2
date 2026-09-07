using Bib_Hacienda.Dominio;
using Bib_Hacienda.Interfaces;
using System;
using System.Collections.Generic;

namespace Bib_Hacienda.Aplicacion
{
    // SC-2 ("La hacienda necesita conectar a las reses chips para
    // geolocalización"): agrega la posibilidad de registrar y consultar
    // las lecturas de posición del chip GPS de cada Res, siguiendo el
    // mismo estilo que ServicioHistoriaClinica: valida que la res exista
    // dentro del potrero indicado (IRepositorioPotreros + Potrero.BuscarRes)
    // antes de registrar la lectura, y delega la persistencia en
    // IRepositorioGeolocalizacion (DIP).
    public class ServicioGeolocalizacion : IGeolocalizacion
    {
        private IRepositorioPotreros repositorioPotreros;
        private IRepositorioGeolocalizacion repositorioGeolocalizacion;

        public ServicioGeolocalizacion(IRepositorioPotreros repositorioPotreros, IRepositorioGeolocalizacion repositorioGeolocalizacion)
        {
            this.repositorioPotreros = repositorioPotreros;
            this.repositorioGeolocalizacion = repositorioGeolocalizacion;
        }

        public ResultadoOperacion RegistrarUbicacion(string potreroId, string nombreRes, string codigoChip, DateTime fecha, double latitud, double longitud)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(codigoChip))
                {
                    return ResultadoOperacion.Fallo("El código del chip no puede estar vacío");
                }

                if (latitud < -90.0 || latitud > 90.0)
                {
                    return ResultadoOperacion.Fallo("La latitud debe estar entre -90 y 90 grados");
                }

                if (longitud < -180.0 || longitud > 180.0)
                {
                    return ResultadoOperacion.Fallo("La longitud debe estar entre -180 y 180 grados");
                }

                Potrero potrero = repositorioPotreros.ObtenerPorId(potreroId);
                if (potrero == null)
                {
                    return ResultadoOperacion.Fallo($"No se encontró el potrero '{potreroId}'");
                }

                Res res;
                try
                {
                    res = potrero.BuscarRes(nombreRes);
                }
                catch (Exception ex)
                {
                    return ResultadoOperacion.Fallo($"No se encontró la res '{nombreRes}' en el potrero '{potreroId}': {ex.Message}");
                }

                var registro = new RegistroGeolocalizacion(potrero.Identificacion, res.Nombre, codigoChip, fecha, latitud, longitud);
                repositorioGeolocalizacion.Agregar(registro);

                return ResultadoOperacion.Ok($"Ubicación registrada correctamente para la res {res.Nombre}.");
            }
            catch (Exception ex)
            {
                return ResultadoOperacion.Fallo("Error inesperado en el método RegistrarUbicacion: " + ex.Message);
            }
        }

        public List<RegistroGeolocalizacion> ConsultarUbicaciones(string potreroId, string nombreRes)
        {
            try
            {
                return repositorioGeolocalizacion.ObtenerPorRes(potreroId, nombreRes);
            }
            catch
            {
                return new List<RegistroGeolocalizacion>();
            }
        }
    }
}
