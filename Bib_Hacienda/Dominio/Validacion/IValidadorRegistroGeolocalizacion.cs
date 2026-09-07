namespace Bib_Hacienda.Dominio.Validacion
{
    // ISP: un validador por entidad, igual que IValidadorEventoClinico,
    // IValidadorVacuna, etc. SC-2: valida RegistroGeolocalizacion antes
    // de persistirlo.
    public interface IValidadorRegistroGeolocalizacion
    {
        bool EsValido(RegistroGeolocalizacion registro);
    }
}
