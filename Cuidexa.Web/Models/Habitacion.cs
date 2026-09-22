namespace Cuidexa.Web.Models;

public class Habitacion : ITieneCentro
{
    public int Id { get; set; }

    public int CentroId { get; set; }
    public Centro? Centro { get; set; }

    public string Numero { get; set; } = string.Empty;
    public string Planta { get; set; } = string.Empty;

    public ICollection<Residente> Residentes { get; set; } = new List<Residente>();
    public ICollection<TareaLimpieza> Tareas { get; set; } = new List<TareaLimpieza>();
}
