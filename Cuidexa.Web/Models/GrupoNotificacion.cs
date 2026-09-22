namespace Cuidexa.Web.Models;

// Grupo ad-hoc de empleados (de cualquier departamento) para dirigir avisos
// manuales sin notificar a todo un departamento entero (Fase 7).
public class GrupoNotificacion : ITieneCentro
{
    public int Id { get; set; }

    public int CentroId { get; set; }
    public Centro? Centro { get; set; }

    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;

    public List<GrupoNotificacionEmpleado> Miembros { get; set; } = new();
}
