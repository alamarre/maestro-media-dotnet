namespace Maestro.Web.Api.Tests;

[Collection("Database collection")]
public class UnitTest1(AppFixture dbFixture)
{
    [Fact]
    public async Task PingTeest()
    {
        var result = await dbFixture.Client.GetAsync("/ping");
        result.EnsureSuccessStatusCode();
        Assert.Equal("pong", await result.Content.ReadAsStringAsync());
    }
}