namespace Cuidexa.Web.Services;

// Fila del informe "Residentes y cuidados". Dietas activas y patologías más
// frecuentes son "a fecha de hoy" (esas tablas no llevan histórico por
// fecha); medicación y sesiones sí están acotadas al rango pedido.
public class FilaMedicacionAdministrada
{
    public string Residente { get; set; } = string.Empty;
    public string Medicamento { get; set; } = string.Empty;
    public int VecesAdministrada { get; set; }
}

public class FilaSesionTerapia
{
    public string Residente { get; set; } = string.Empty;
    public string Profesional { get; set; } = string.Empty;
    public string Especialidad { get; set; } = string.Empty;
    public int SesionesRealizadas { get; set; }
}

public class InformeResidentesCuidados
{
    public DateOnly Desde { get; set; }
    public DateOnly Hasta { get; set; }
    public List<(string Dieta, int Cantidad)> DietasActivas { get; set; } = new();
    public List<(string Patologia, int Cantidad)> PatologiasFrecuentes { get; set; } = new();
    public List<FilaMedicacionAdministrada> MedicacionAdministrada { get; set; } = new();
    public List<FilaSesionTerapia> SesionesRealizadas { get; set; } = new();
}
