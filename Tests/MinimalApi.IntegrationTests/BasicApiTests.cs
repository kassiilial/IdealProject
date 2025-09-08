using System.Net;
using System.Net.Http.Json;
using Xunit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Models;
using System.Threading.Tasks;
using System.Text.Json;

namespace MinimalApi.IntegrationTests;

// Use a custom factory so tests run with a non-development environment and avoid middleware
// (Swagger UI / DeveloperExceptionPage) that can trigger incompatible PipeWriter paths
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Force a non-development environment during tests
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // No-op: we keep default services but environment will prevent Dev-only middleware
        });
    }
}

public class BasicApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly WebApplicationFactory<Program> _factory;

    public BasicApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_root_returns_ok()
    {
        var client = _factory.CreateClient();
        var res = await client.GetAsync("/");
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await res.Content.ReadFromJsonAsync<JsonElement?>();
        body.HasValue.Should().BeTrue();
        var message = body.Value.GetProperty("message").GetString();
        message.Should().Contain("Hello");
    }

    [Fact]
    public async Task Items_crud_via_api()
    {
        var client = _factory.CreateClient();

        // Get all
        var all = await client.GetFromJsonAsync<Item[]>("/items");
        all.Should().NotBeNull();

        // Create
        var toCreate = new Item { Name = "FromTest" };
        var createResp = await client.PostAsJsonAsync("/items", toCreate);
        createResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResp.Content.ReadFromJsonAsync<Item>();
        created.Should().NotBeNull();

        // Get by id
        var getResp = await client.GetAsync($"/items/{created!.Id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // Update
        created.Name = "FromTestUpdated";
        var putResp = await client.PutAsJsonAsync($"/items/{created.Id}", created);
        putResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // Delete
        var delResp = await client.DeleteAsync($"/items/{created.Id}");
        delResp.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
