using System;
using System.Globalization;
using Bib_Hacienda.Aplicacion;
using Bib_Hacienda.Dominio;

namespace Bib_Hacienda.Infraestructura
{
    // Conserva el formato por res del AS-IS
    // (PersistenciaService.GuardarReses / CargarReses):
    // "Nombre|Peso|Edad|Tipo" (Tipo = res.GetType().Name).
    //
    // Nota de diseño (SRP, H-11): en el AS-IS cada línea de Reses.txt
    // empezaba con la Identificacion del potrero
    // ("{potrero.Identificacion}|{res.Nombre}|..."), porque
    // PersistenciaService mezclaba en un mismo método la relación
    // potrero-res con el formato de cada res. Res no tiene una referencia a
    // su Potrero (ni la tenía en el AS-IS ni la define el UML TO-BE), así
    // que ISerializador<Res> solo puede serializar los datos propios de la
    // res. El campo de identificación del potrero es responsabilidad del
    // repositorio (RepositorioPotrerosTexto, fuera de este bloque), que debe
    // anteponer "{potrero.Identificacion}|" al resultado de este serializador
    // al escribir Reses.txt, y quitarlo antes de invocar Deserializar al
    // leerlo. El formato final del archivo, byte a byte, no cambia.
    // Reto 2 (P-02, E-07): antes tenía su propio switch/new para traducir el
    // texto "Ternero/Novillo/Cebon" al tipo concreto, duplicado con el mismo
    // switch de SerializadorVenta. Ahora delega la creación en
    // IRegistroResFactories (Factory Method), la misma fábrica que usa
    // ServicioPotreros para altas nuevas: un tipo de Res nuevo ya no exige
    // tocar los dos serializadores, solo registrar su fábrica en Program.cs.
    public class SerializadorRes : ISerializador<Res>
    {
        private readonly IRegistroResFactories registroFabricas;

        public SerializadorRes(IRegistroResFactories registroFabricas)
        {
            this.registroFabricas = registroFabricas;
        }

        public string Serializar(Res entidad)
        {
            if (entidad == null)
            {
                throw new ArgumentNullException(nameof(entidad));
            }

            string tipo = entidad.GetType().Name;
            return $"{entidad.Nombre}|{entidad.Peso}|{entidad.Edad}|{tipo}";
        }

        public Res Deserializar(string linea)
        {
            if (string.IsNullOrWhiteSpace(linea))
            {
                throw new ArgumentException("La línea de res a deserializar no puede estar vacía.", nameof(linea));
            }

            var partes = linea.Split('|');
            if (partes.Length < 4)
            {
                throw new FormatException($"Línea de res con formato inválido: '{linea}'");
            }

            string nombre = partes[0];
            uint peso = uint.Parse(partes[1], CultureInfo.InvariantCulture);
            ushort edad = ushort.Parse(partes[2], CultureInfo.InvariantCulture);
            string tipo = partes[3].Trim();

            return CrearPorTipo(tipo, nombre, peso, edad);
        }

        // Mismo comportamiento observable que el switch anterior (y que
        // PersistenciaService.CargarVentas en el AS-IS): si el tipo persistido
        // no corresponde a ninguna fábrica registrada, se asume Ternero por
        // defecto en vez de fallar la deserialización.
        private Res CrearPorTipo(string tipo, string nombre, uint peso, ushort edad)
        {
            try
            {
                return registroFabricas.ObtenerFabrica(tipo.ToLowerInvariant()).Crear(nombre, peso, edad);
            }
            catch (Exception)
            {
                return registroFabricas.ObtenerFabrica("ternero").Crear(nombre, peso, edad);
            }
        }
    }
}
