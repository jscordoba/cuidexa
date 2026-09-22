using Cuidexa.Web.ViewHelpers;

namespace Cuidexa.Web.Tests;

public class ColorHelperTests
{
    [Fact]
    public void Oscurecer_ConColorValido_ReduceCadaCanalPorElFactor()
    {
        var resultado = ColorHelper.Oscurecer("#4f46e5");

        Assert.Equal("#3b34ab", resultado);
    }

    [Theory]
    [InlineData("no-es-un-color")]
    [InlineData("#fff")] // 3 dígitos, no soportado
    [InlineData("")]
    public void Oscurecer_ConColorInvalido_DevuelveElValorOriginalSinLanzar(string entrada)
    {
        var resultado = ColorHelper.Oscurecer(entrada);

        Assert.Equal(entrada, resultado);
    }
}
