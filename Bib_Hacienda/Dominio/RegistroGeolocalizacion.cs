using System;

namespace Bib_Hacienda.Dominio
{
    // SC-2 ("La hacienda necesita conectar a las reses chips para
    // geolocalización"): clase de datos simple, mismo estilo que
    // EventoClinico (atributos privados + accesores get/set), que
    // representa una lectura individual de posición reportada por el
    // chip GPS instalado en una Res.
    //
    // Identificación de la Res: igual que EventoClinico (ver nota en ese
    // archivo), Res solo tiene nombre, único DENTRO de un Potrero. Por
    // eso un RegistroGeolocalizacion se identifica con la pareja
    // PotreroId + NombreRes, sin inventar un identificador global que el
    // dominio actual no tiene.
    //
    // CodigoChip identifica el dispositivo físico conectado a la res
    // (una res puede, en teoría, cambiar de chip si se avería el
    // dispositivo; por eso viaja en cada lectura y no solo una vez).
    public class RegistroGeolocalizacion
    {
        //Atributos
        private string potreroId;
        private string nombreRes;
        private string codigoChip;
        private DateTime fecha;
        private double latitud;
        private double longitud;

        //Constructor
        public RegistroGeolocalizacion(string potreroId, string nombreRes, string codigoChip, DateTime fecha, double latitud, double longitud)
        {
            this.PotreroId = potreroId;
            this.NombreRes = nombreRes;
            this.CodigoChip = codigoChip;
            this.Fecha = fecha;
            this.Latitud = latitud;
            this.Longitud = longitud;
        }

        //Accesores
        public string PotreroId { get => potreroId; set => potreroId = value; }
        public string NombreRes { get => nombreRes; set => nombreRes = value; }
        public string CodigoChip { get => codigoChip; set => codigoChip = value; }
        public DateTime Fecha { get => fecha; set => fecha = value; }
        public double Latitud { get => latitud; set => latitud = value; }
        public double Longitud { get => longitud; set => longitud = value; }
    }
}
