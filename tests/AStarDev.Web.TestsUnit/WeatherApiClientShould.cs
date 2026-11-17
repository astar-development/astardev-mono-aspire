namespace AStarDev.Web.TestsUnit;

public class WeatherApiClientShould
{
    [Fact]
    public async Task throw_when_null_passed_to_the_constructor()
    {
        var sut = new WeatherApiClient(null!);

        await Should.ThrowAsync<ArgumentNullException>(() => sut.GetWeatherAsync());
    }
}
