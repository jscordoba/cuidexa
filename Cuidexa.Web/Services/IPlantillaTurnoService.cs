using Cuidexa.Web.Models;
using Cuidexa.Web.Models.Enums;

namespace Cuidexa.Web.Services;

public interface IPlantillaTurnoService
{
    Task<List<PlantillaTurno>> ObtenerActivasAsync();
    Task<List<PlantillaTurno>> ObtenerTodasAsync();
    Task<PlantillaTurno?> ObtenerPorIdAsync(int id);
    Task CrearAsync(string nombre, TimeOnly horaInicio, TimeOnly horaFin, TipoDia tipoDia);
    Task EditarAsync(int id, string nombre, TimeOnly horaInicio, TimeOnly horaFin, TipoDia tipoDia);
    Task CambiarActivaAsync(int id, bool activo);
}
