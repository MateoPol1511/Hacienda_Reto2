using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bib_Hacienda.Aplicacion;
using Bib_Hacienda.Dominio;
using Bib_Hacienda.Dominio.Validacion;

namespace Bib_Hacienda.Infraestructura
{
    // SC-2: mismo patrón que RepositorioHistoriaClinicaTexto (a su vez
    // igual que RepositorioVacunasTexto): un archivo de texto plano en la
    // carpeta Datos (DirectorioDatos), validación explícita antes de
    // persistir (sin AOP), altas por apéndice.
    //
    // Igual que la historia clínica, la geolocalización solo pide
    // registrar y consultar lecturas (SC-2 no pide "borrar" una lectura
    // de posición ni corregirla), por lo que el archivo solo crece por
    // apéndice.
    public class RepositorioGeolocalizacionTexto : IRepositorioGeolocalizacion
    {
        private const string ArchivoGeolocalizacion = "Geolocalizacion.txt";

        private readonly ISerializador<RegistroGeolocalizacion> serializadorRegistroGeolocalizacion;
        private readonly IValidadorRegistroGeolocalizacion validadorRegistroGeolocalizacion;

        public RepositorioGeolocalizacionTexto(ISerializador<RegistroGeolocalizacion> serializadorRegistroGeolocalizacion, IValidadorRegistroGeolocalizacion validadorRegistroGeolocalizacion)
        {
            this.serializadorRegistroGeolocalizacion = serializadorRegistroGeolocalizacion;
            this.validadorRegistroGeolocalizacion = validadorRegistroGeolocalizacion;
        }

        public List<RegistroGeolocalizacion> ObtenerPorRes(string potreroId, string nombreRes)
        {
            try
            {
                return CargarRegistrosDesdeArchivo()
                    .Where(r => string.Equals(r.PotreroId, potreroId, StringComparison.OrdinalIgnoreCase)
                             && string.Equals(r.NombreRes, nombreRes, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(r => r.Fecha)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al cargar geolocalización: {ex.Message}", ex);
            }
        }

        public void Agregar(RegistroGeolocalizacion registro)
        {
            if (registro == null)
            {
                throw new ArgumentNullException(nameof(registro));
            }

            if (!validadorRegistroGeolocalizacion.EsValido(registro))
            {
                throw new Exception("Error de validación en el registro de geolocalización");
            }

            string ruta = Path.Combine(DirectorioDatos.ObtenerRuta(), ArchivoGeolocalizacion);
            File.AppendAllLines(ruta, new[] { serializadorRegistroGeolocalizacion.Serializar(registro) });
        }

        private List<RegistroGeolocalizacion> CargarRegistrosDesdeArchivo()
        {
            string ruta = Path.Combine(DirectorioDatos.ObtenerRuta(), ArchivoGeolocalizacion);
            var registros = new List<RegistroGeolocalizacion>();

            if (!File.Exists(ruta))
            {
                return registros;
            }

            foreach (var linea in File.ReadAllLines(ruta))
            {
                if (string.IsNullOrWhiteSpace(linea))
                {
                    continue;
                }

                registros.Add(serializadorRegistroGeolocalizacion.Deserializar(linea));
            }

            return registros;
        }
    }
}
