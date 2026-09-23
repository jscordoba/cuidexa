using Cuidexa.Web.Models;
using Cuidexa.Web.Models.Enums;

namespace Cuidexa.Web.Services;

public class CrearIncidenciaViewModel
{
    public List<Residente> Residentes { get; set; } = new();
    public RolEmpleado RolOrigen { get; set; }
    public string RutaBase { get; set; } = string.Empty;
}
