namespace Cuidexa.Web.Services;

public interface IComensalesService
{
    Task<ComensalesHoyViewModel> ObtenerComensalesAsync(DateOnly fecha);
}
