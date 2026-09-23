using Cuidexa.Web.Models;
using Cuidexa.Web.Models.Enums;

namespace Cuidexa.Web.Services;

public interface IDocumentoFirmadoService
{
    Task<List<DocumentoFirmado>> ObtenerPorResidenteAsync(int residenteId);
    Task<List<DocumentoFirmado>> ObtenerPorIncidenciaAsync(int incidenciaId);
    Task<List<DocumentoFirmado>> ObtenerTodosAsync();

    Task CrearAsync(CategoriaDocumentoFirmado categoria, string titulo, string descripcion,
        int? residenteId, int? incidenciaId, string firmanteNombre, string firmanteRelacion,
        string firmaImagenBase64, int? empleadoRegistraId);
}
