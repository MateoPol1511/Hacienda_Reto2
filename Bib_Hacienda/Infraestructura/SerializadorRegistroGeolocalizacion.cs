using System;
using System.Globalization;
using Bib_Hacienda.Dominio;

namespace Bib_Hacienda.Infraestructura
{
    // SC-2: reutiliza ISerializador<T> (no se crea una interfaz nueva, ver
    // nota en ISerializador.cs). Formato de línea para Geolocalizacion.txt:
    // "PotreroId|NombreRes|CodigoChip|Fecha(yyyy-MM-dd HH:mm:ss)|Latitud|Longitud".
    //
    // A diferencia de SerializadorEventoClinico, aquí la fecha incluye hora
    // (una res puede recibir varias lecturas de posición el mismo día) y
    // las coordenadas se formatean con CultureInfo.InvariantCulture para
    // que el separador decimal sea siempre '.', sin importar la
    // configuración regional del servidor donde corra la aplicación.
    public class SerializadorRegistroGeolocalizacion : ISerializador<RegistroGeolocalizacion>
    {
        private const string FormatoFecha = "yyyy-MM-dd HH:mm:ss";

        public string Serializar(RegistroGeolocalizacion entidad)
        {
            if (entidad == null)
            {
                throw new ArgumentNullException(nameof(entidad));
            }

            string fecha = entidad.Fecha.ToString(FormatoFecha, CultureInfo.InvariantCulture);
            string latitud = entidad.Latitud.ToString("F6", CultureInfo.InvariantCulture);
            string longitud = entidad.Longitud.ToString("F6", CultureInfo.InvariantCulture);

            return $"{entidad.PotreroId}|{entidad.NombreRes}|{entidad.CodigoChip}|{fecha}|{latitud}|{longitud}";
        }

        public RegistroGeolocalizacion Deserializar(string linea)
        {
            if (string.IsNullOrWhiteSpace(linea))
            {
                throw new ArgumentException("La línea de geolocalización a deserializar no puede estar vacía.", nameof(linea));
            }

            var partes = linea.Split('|');
            if (partes.Length < 6)
            {
                throw new FormatException($"Línea de geolocalización con formato inválido: '{linea}'");
            }

            string potreroId = partes[0];
            string nombreRes = partes[1];
            string codigoChip = partes[2];

            if (!DateTime.TryParseExact(partes[3].Trim(), FormatoFecha, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fecha))
            {
                throw new FormatException($"Fecha de geolocalización con formato inválido: '{partes[3]}'");
            }

            if (!double.TryParse(partes[4].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double latitud))
            {
                throw new FormatException($"Latitud con formato inválido: '{partes[4]}'");
            }

            if (!double.TryParse(partes[5].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double longitud))
            {
                throw new FormatException($"Longitud con formato inválido: '{partes[5]}'");
            }

            return new RegistroGeolocalizacion(potreroId, nombreRes, codigoChip, fecha, latitud, longitud);
        }
    }
}
