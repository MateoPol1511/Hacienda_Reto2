using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bib_Hacienda.Aplicacion;
using Bib_Hacienda.Dominio;
using Bib_Hacienda.Dominio.Validacion;

namespace Bib_Hacienda.Infraestructura
{
    // H-11/H-12: la validación que en el AS-IS hacía PersistenciaService
    // mediante proxies de Castle DynamicProxy (InterceptorValidarInformacion)
    // ahora es una llamada explícita del repositorio al validador
    // correspondiente antes de persistir. Mismo comportamiento observable,
    // sin AOP.
    //
    // Persiste en dos archivos, igual que el AS-IS:
    // - Potreros.txt: una línea por potrero (serializadorPotrero).
    // - Reses.txt: una línea por res, con el mismo formato de
    //   PersistenciaService.GuardarReses/CargarReses
    //   ("PotreroId|<línea de serializadorRes>"). serializadorRes solo
    //   conoce los campos propios de la res (ver SerializadorRes, Bloque
    //   3A); este repositorio antepone/quita el PotreroId, que es la
    //   relación potrero-res, no un dato de la res.
    //
    // CORRECCIÓN (post-registro de inconsistencia): IRepositorioPotreros no
    // tenía forma de volver a persistir un Potrero ya existente después de
    // mutarlo (alta, cambio o baja de una de sus reses): Agregar(potrero)
    // solo se invocaba al crear un potrero nuevo. Eso hacía que
    // AnadirResAPotrero (alta), ServicioAlimentacion.AlimentarRes (peso) y
    // ServicioVentas.vender_res (baja) mostraran el mensaje de éxito pero
    // solo mutaran el grafo en memoria: en la siguiente lectura desde
    // archivo, Reses.txt no reflejaba el cambio. Se añaden tres métodos a
    // IRepositorioPotreros —AgregarRes, ActualizarRes y RemoverRes—
    // siguiendo el mismo patrón que ya usan IRepositorioVentas/
    // IRepositorioVacunas (repositorio dueño de su propio archivo, con sus
    // propios métodos de escritura). Actualizar/Remover exigen reescribir
    // Reses.txt completo (ver ReescribirLineasReses) porque el archivo es
    // una línea por res sin índice.
    //
    // CORRECCIÓN (bug reportado: una vacuna aplicada no se veía como
    // aplicada): ServicioVacunacion.aplicar_vacuna agregaba la vacuna a
    // Res.L_vacunas_aplicadas solo en memoria; como CargarResesDentroDePotreros
    // reconstruye cada Res desde cero en cada lectura, la siguiente petición
    // (p. ej. el redirect a Vacuna/Index o abrir Res/DetalleVacunas) volvía a
    // mostrar la res "sin vacunar". Se agrega RegistrarVacunaAplicada
    // (persiste en VacunasAplicadas.txt) y se completa la carga en
    // CargarResesDentroDePotreros, reutilizando el mismo
    // ISerializador&lt;Vacuna&gt; (SerializadorVacuna) que ya existía para
    // Vacunas.txt, tal como ese serializador ya documentaba como diseño
    // previsto: no se inventa formato ni serializador nuevos.
    public class RepositorioPotrerosTexto : IRepositorioPotreros
    {
        private const string ArchivoPotreros = "Potreros.txt";
        private const string ArchivoReses = "Reses.txt";
        private const string ArchivoVacunasAplicadas = "VacunasAplicadas.txt";

        private readonly ISerializador<Potrero> serializadorPotrero;
        private readonly ISerializador<Res> serializadorRes;
        private readonly ISerializador<Vacuna> serializadorVacuna;
        private readonly IValidadorPotrero validadorPotrero;
        private readonly IValidadorRes validadorRes;

        public RepositorioPotrerosTexto(ISerializador<Potrero> serializadorPotrero, ISerializador<Res> serializadorRes, ISerializador<Vacuna> serializadorVacuna, IValidadorPotrero validadorPotrero, IValidadorRes validadorRes)
        {
            this.serializadorPotrero = serializadorPotrero;
            this.serializadorRes = serializadorRes;
            this.serializadorVacuna = serializadorVacuna;
            this.validadorPotrero = validadorPotrero;
            this.validadorRes = validadorRes;
        }

        public List<Potrero> ObtenerTodos()
        {
            try
            {
                var potreros = CargarPotrerosDesdeArchivo();
                CargarResesDentroDePotreros(potreros);
                return potreros;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al cargar potreros: {ex.Message}", ex);
            }
        }

        public Potrero ObtenerPorId(string identificacion)
        {
            if (string.IsNullOrWhiteSpace(identificacion))
            {
                return null;
            }

            return ObtenerTodos()
                .FirstOrDefault(p => string.Equals(p.Identificacion, identificacion, StringComparison.OrdinalIgnoreCase));
        }

        public bool Existe(string identificacion)
        {
            if (string.IsNullOrWhiteSpace(identificacion))
            {
                return false;
            }

            return ObtenerTodos()
                .Any(p => string.Equals(p.Identificacion, identificacion, StringComparison.OrdinalIgnoreCase));
        }

        public void Agregar(Potrero potrero)
        {
            if (potrero == null)
            {
                throw new ArgumentNullException(nameof(potrero));
            }

            if (!validadorPotrero.EsValido(potrero))
            {
                throw new Exception("Error de validación en potrero");
            }

            foreach (var res in potrero.L_reses)
            {
                if (!validadorRes.EsValido(res))
                {
                    throw new Exception("Error de validación en res");
                }
            }

            string rutaPotreros = Path.Combine(DirectorioDatos.ObtenerRuta(), ArchivoPotreros);
            File.AppendAllLines(rutaPotreros, new[] { serializadorPotrero.Serializar(potrero) });

            if (potrero.L_reses.Count > 0)
            {
                string rutaReses = Path.Combine(DirectorioDatos.ObtenerRuta(), ArchivoReses);
                var lineasReses = potrero.L_reses
                    .Select(res => $"{potrero.Identificacion}|{serializadorRes.Serializar(res)}");
                File.AppendAllLines(rutaReses, lineasReses);
            }
        }

        public void AgregarRes(string potreroId, Res res)
        {
            if (string.IsNullOrWhiteSpace(potreroId))
            {
                throw new ArgumentException(nameof(potreroId));
            }

            if (res == null)
            {
                throw new ArgumentNullException(nameof(res));
            }

            if (!validadorRes.EsValido(res))
            {
                throw new Exception("Error de validación en res");
            }

            string rutaReses = Path.Combine(DirectorioDatos.ObtenerRuta(), ArchivoReses);
            string lineaRes = $"{potreroId}|{serializadorRes.Serializar(res)}";
            File.AppendAllLines(rutaReses, new[] { lineaRes });
        }

        public void ActualizarRes(string potreroId, Res res)
        {
            if (string.IsNullOrWhiteSpace(potreroId))
            {
                throw new ArgumentException(nameof(potreroId));
            }

            if (res == null)
            {
                throw new ArgumentNullException(nameof(res));
            }

            if (!validadorRes.EsValido(res))
            {
                throw new Exception("Error de validación en res");
            }

            string lineaNueva = $"{potreroId}|{serializadorRes.Serializar(res)}";
            bool reemplazada = false;

            ReescribirLineasReses(lineas =>
            {
                var resultado = new List<string>(lineas.Count);
                foreach (var linea in lineas)
                {
                    if (!reemplazada && EsLineaDeRes(linea, potreroId, res.Nombre))
                    {
                        resultado.Add(lineaNueva);
                        reemplazada = true;
                    }
                    else
                    {
                        resultado.Add(linea);
                    }
                }

                // Si la res no tenía línea previa (dato inconsistente), se
                // agrega igual en vez de perder la actualización.
                if (!reemplazada)
                {
                    resultado.Add(lineaNueva);
                }

                return resultado;
            });
        }

        public void RemoverRes(string potreroId, string nombreRes)
        {
            if (string.IsNullOrWhiteSpace(potreroId))
            {
                throw new ArgumentException(nameof(potreroId));
            }

            if (string.IsNullOrWhiteSpace(nombreRes))
            {
                throw new ArgumentException(nameof(nombreRes));
            }

            ReescribirLineasReses(lineas => lineas
                .Where(linea => !EsLineaDeRes(linea, potreroId, nombreRes))
                .ToList());
        }

        // Persiste una vacuna que ServicioVacunacion.aplicar_vacuna ya
        // agregó a Res.L_vacunas_aplicadas en memoria. Mismo formato de
        // línea que el AS-IS documentaba para VacunasAplicadas.txt:
        // "PotreroId|NombreRes|" + los mismos 6 campos que serializadorVacuna
        // ya produce para Vacunas.txt (Nombre|Lote|FechaVenc|FechaAplic|Tipo|Periodo).
        public void RegistrarVacunaAplicada(string potreroId, string nombreRes, Vacuna vacuna)
        {
            if (string.IsNullOrWhiteSpace(potreroId))
            {
                throw new ArgumentException(nameof(potreroId));
            }

            if (string.IsNullOrWhiteSpace(nombreRes))
            {
                throw new ArgumentException(nameof(nombreRes));
            }

            if (vacuna == null)
            {
                throw new ArgumentNullException(nameof(vacuna));
            }

            string ruta = Path.Combine(DirectorioDatos.ObtenerRuta(), ArchivoVacunasAplicadas);
            string linea = $"{potreroId}|{nombreRes}|{serializadorVacuna.Serializar(vacuna)}";
            File.AppendAllLines(ruta, new[] { linea });
        }

        // Lee Reses.txt completo, aplica la transformación (reemplazar o
        // quitar una línea) y reescribe el archivo. Necesario porque el
        // archivo es una línea por res sin índice: actualizar o borrar una
        // res existente exige reescribir el archivo completo, a diferencia
        // de Agregar/AgregarRes que solo hacen append.
        private void ReescribirLineasReses(Func<List<string>, List<string>> transformar)
        {
            string ruta = Path.Combine(DirectorioDatos.ObtenerRuta(), ArchivoReses);
            var lineas = File.Exists(ruta)
                ? File.ReadAllLines(ruta).Where(l => !string.IsNullOrWhiteSpace(l)).ToList()
                : new List<string>();

            var lineasFinales = transformar(lineas);
            File.WriteAllLines(ruta, lineasFinales);
        }

        // Compara una línea de Reses.txt ("PotreroId|Nombre|Peso|Edad|Tipo")
        // contra un potreroId + nombre de res, igual que
        // CargarResesDentroDePotreros al indexar por PotreroId.
        private bool EsLineaDeRes(string linea, string potreroId, string nombreRes)
        {
            if (string.IsNullOrWhiteSpace(linea))
            {
                return false;
            }

            int separador = linea.IndexOf('|');
            if (separador < 0)
            {
                return false;
            }

            string lineaPotreroId = linea.Substring(0, separador).Trim();
            string restoLinea = linea.Substring(separador + 1);

            int separadorNombre = restoLinea.IndexOf('|');
            string lineaNombreRes = (separadorNombre < 0 ? restoLinea : restoLinea.Substring(0, separadorNombre)).Trim();

            return string.Equals(lineaPotreroId, potreroId, StringComparison.OrdinalIgnoreCase)
                && string.Equals(lineaNombreRes, nombreRes, StringComparison.OrdinalIgnoreCase);
        }

        // Misma lógica que PersistenciaService.CargarPotreros del AS-IS:
        // normaliza identificaciones y evita duplicados (case-insensitive).
        private List<Potrero> CargarPotrerosDesdeArchivo()
        {
            string ruta = Path.Combine(DirectorioDatos.ObtenerRuta(), ArchivoPotreros);
            var potreros = new List<Potrero>();

            if (!File.Exists(ruta))
            {
                return potreros;
            }

            foreach (var linea in File.ReadAllLines(ruta))
            {
                if (string.IsNullOrWhiteSpace(linea))
                {
                    continue;
                }

                var potrero = serializadorPotrero.Deserializar(linea);

                if (!potreros.Any(p => string.Equals(p.Identificacion, potrero.Identificacion, StringComparison.OrdinalIgnoreCase)))
                {
                    potreros.Add(potrero);
                }
            }

            return potreros;
        }

        // Misma lógica que PersistenciaService.CargarReses del AS-IS: cada
        // línea trae el PotreroId al frente; el resto de la línea es el
        // formato propio de serializadorRes.
        private void CargarResesDentroDePotreros(List<Potrero> potreros)
        {
            string ruta = Path.Combine(DirectorioDatos.ObtenerRuta(), ArchivoReses);

            if (!File.Exists(ruta))
            {
                return;
            }

            foreach (var linea in File.ReadAllLines(ruta))
            {
                if (string.IsNullOrWhiteSpace(linea))
                {
                    continue;
                }

                int separador = linea.IndexOf('|');
                if (separador < 0)
                {
                    continue;
                }

                string potreroId = linea.Substring(0, separador).Trim();
                string lineaRes = linea.Substring(separador + 1);

                var potrero = potreros.FirstOrDefault(p => string.Equals(p.Identificacion, potreroId, StringComparison.OrdinalIgnoreCase));
                if (potrero == null)
                {
                    continue;
                }

                var res = serializadorRes.Deserializar(lineaRes);
                potrero.L_reses.Add(res);
            }

            CargarVacunasAplicadasDentroDeReses(potreros);
        }

        // Lee VacunasAplicadas.txt ("PotreroId|NombreRes|" + los mismos 6
        // campos de SerializadorVacuna) y agrega cada vacuna a
        // Res.L_vacunas_aplicadas de la res correspondiente. Se llama
        // siempre después de CargarResesDentroDePotreros para que las
        // vacunas registradas por RegistrarVacunaAplicada sobrevivan a la
        // siguiente lectura desde archivo (mismo problema que ya resolvían
        // AgregarRes/ActualizarRes/RemoverRes para el resto de campos de Res).
        private void CargarVacunasAplicadasDentroDeReses(List<Potrero> potreros)
        {
            string ruta = Path.Combine(DirectorioDatos.ObtenerRuta(), ArchivoVacunasAplicadas);

            if (!File.Exists(ruta))
            {
                return;
            }

            foreach (var linea in File.ReadAllLines(ruta))
            {
                if (string.IsNullOrWhiteSpace(linea))
                {
                    continue;
                }

                int separadorPotrero = linea.IndexOf('|');
                if (separadorPotrero < 0)
                {
                    continue;
                }

                string potreroId = linea.Substring(0, separadorPotrero).Trim();
                string resto = linea.Substring(separadorPotrero + 1);

                int separadorRes = resto.IndexOf('|');
                if (separadorRes < 0)
                {
                    continue;
                }

                string nombreRes = resto.Substring(0, separadorRes).Trim();
                string lineaVacuna = resto.Substring(separadorRes + 1);

                var potrero = potreros.FirstOrDefault(p => string.Equals(p.Identificacion, potreroId, StringComparison.OrdinalIgnoreCase));
                var res = potrero?.L_reses.FirstOrDefault(r => string.Equals(r.Nombre, nombreRes, StringComparison.OrdinalIgnoreCase));

                if (res == null)
                {
                    continue;
                }

                var vacuna = serializadorVacuna.Deserializar(lineaVacuna);

                if (!res.L_vacunas_aplicadas.Any(v => v.Nombre == vacuna.Nombre && v.Lote == vacuna.Lote))
                {
                    res.L_vacunas_aplicadas.Add(vacuna);
                }
            }
        }
    }
}
