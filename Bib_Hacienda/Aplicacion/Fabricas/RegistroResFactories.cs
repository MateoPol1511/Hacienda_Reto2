using System;
using System.Collections.Generic;

namespace Bib_Hacienda.Aplicacion
{
    // OCP (H-07): una fábrica por tipo de Res, indexadas por clave de texto
    // (Potrero.Tipo_potrero). Un 4º tipo se agrega registrando una clase
    // nueva en el composition root, sin tocar Potrero ni ServicioPotreros.
    // Ver ADR-03. Antes: RegistroFabricasRes (renombrado junto con
    // IFabricaRes/IRegistroFabricasRes para alinear código y diagrama TO-BE,
    // Reto 2 / E-07). Mismo comportamiento observable: sigue lanzando
    // excepción para un tipo no registrado.
    public class RegistroResFactories : IRegistroResFactories
    {
        private Dictionary<string, IResFactory> fabricas;

        public RegistroResFactories(Dictionary<string, IResFactory> fabricas)
        {
            this.fabricas = fabricas;
        }

        public IResFactory ObtenerFabrica(string tipo)
        {
            if (tipo == null || !fabricas.TryGetValue(tipo, out IResFactory fabrica))
            {
                throw new Exception($"No existe una fábrica registrada para el tipo de res '{tipo}'.");
            }
            return fabrica;
        }
    }
}
