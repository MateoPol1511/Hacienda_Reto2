using Bib_Hacienda.Dominio;
using System.Collections.Generic;

namespace Bib_Hacienda.Aplicacion
{
    // DIP (H-02, H-03, H-04, H-11, H-13): un repositorio por entidad. Los
    // servicios de aplicación dependen de esta abstracción; la implementación
    // concreta vive en Bib_Hacienda.Infraestructura (fuera de este bloque).
    public interface IRepositorioPotreros
    {
        List<Potrero> ObtenerTodos();
        Potrero ObtenerPorId(string identificacion);
        void Agregar(Potrero potrero);
        bool Existe(string identificacion);

        // Persiste una res que se añade a un potrero YA existente (distinto
        // de Agregar(Potrero), que solo escribe potreros nuevos). Cierra la
        // inconsistencia registrada en RepositorioPotrerosTexto: sin este
        // método, AnadirResAPotrero solo mutaba el grafo en memoria y la res
        // desaparecía en la siguiente lectura desde archivo.
        void AgregarRes(string potreroId, Res res);

        // Vuelve a escribir en Reses.txt los datos actuales de una res que
        // ya existía (mismo Potrero + Nombre), tras una mutación en memoria
        // como ServicioAlimentacion.AlimentarRes (res.Peso). Sin este
        // método, el peso alimentado se perdía en la siguiente lectura
        // desde archivo, igual que pasaba con AgregarRes.
        void ActualizarRes(string potreroId, Res res);

        // Elimina la línea de una res de Reses.txt cuando deja de
        // pertenecer al potrero (p. ej. tras ServicioVentas.vender_res).
        // Sin este método, Potrero.L_reses.Remove(res) solo la quitaba del
        // grafo en memoria y la res vendida seguía apareciendo viva en la
        // siguiente lectura desde archivo.
        void RemoverRes(string potreroId, string nombreRes);

        // Persiste en VacunasAplicadas.txt una vacuna que
        // ServicioVacunacion.aplicar_vacuna ya agregó a
        // Res.L_vacunas_aplicadas en memoria. Misma causa raíz que
        // AgregarRes/ActualizarRes/RemoverRes: sin este método, la vacuna
        // aplicada solo vivía en el grafo en memoria de esa petición y
        // desaparecía (la res volvía a verse "sin vacunar") en la
        // siguiente lectura desde archivo. No inventa formato ni
        // serializador nuevos: reutiliza ISerializador&lt;Vacuna&gt;, tal
        // como ya documentaba SerializadorVacuna.
        void RegistrarVacunaAplicada(string potreroId, string nombreRes, Vacuna vacuna);
    }
}
