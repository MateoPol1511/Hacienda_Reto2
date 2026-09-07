namespace Bib_Hacienda.Dominio.Validacion
{
    // Misma filosofía que ValidadorEventoClinico: registro no nulo y con
    // los campos mínimos que exige SC-2 (identificación de la res,
    // código del chip) presentes, además de coordenadas dentro del
    // rango físicamente válido (latitud [-90, 90], longitud [-180, 180]).
    // Este último chequeo no existe en ningún otro validador del
    // sistema: es un punto de dolor propio de la geolocalización (un
    // chip con GPS defectuoso podría reportar una lectura fuera de
    // rango) y no del resto de entidades.
    public class ValidadorRegistroGeolocalizacion : IValidadorRegistroGeolocalizacion
    {
        private const double LatitudMinima = -90.0;
        private const double LatitudMaxima = 90.0;
        private const double LongitudMinima = -180.0;
        private const double LongitudMaxima = 180.0;

        public bool EsValido(RegistroGeolocalizacion registro)
        {
            if (registro == null
                || string.IsNullOrWhiteSpace(registro.PotreroId)
                || string.IsNullOrWhiteSpace(registro.NombreRes)
                || string.IsNullOrWhiteSpace(registro.CodigoChip))
            {
                return false;
            }

            if (registro.Latitud < LatitudMinima || registro.Latitud > LatitudMaxima)
            {
                return false;
            }

            if (registro.Longitud < LongitudMinima || registro.Longitud > LongitudMaxima)
            {
                return false;
            }

            return true;
        }
    }
}
