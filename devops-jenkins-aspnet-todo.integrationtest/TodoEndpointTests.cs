using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using static devops_jenkins_aspnet_todo.api.Controllers.TodoController;

namespace devops_jenkins_aspnet_todo.integrationtest;

public class TodoEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public TodoEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_OnStartup_ReturnsSeedData()
    {
        var response = await _client.GetAsync("/Todo");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var all = await response.Content.ReadFromJsonAsync<TodoItem[]>();
        Assert.NotNull(all);
        Assert.NotEmpty(all);
        foreach (var seed in SeedItems)
        {
            Assert.Contains(all, t => t.Id == seed.Id && t.Title == seed.Title);
        }
    }

    [Fact]
    public async Task GetById_SeedItem_ReturnsItem()
    {
        var seed = SeedItems[0];

        var fetched = await _client.GetFromJsonAsync<TodoItem>($"/Todo/{seed.Id}");

        Assert.NotNull(fetched);
        Assert.Equal(seed.Id, fetched.Id);
        Assert.Equal(seed.Title, fetched.Title);
        Assert.Equal(seed.IsCompleted, fetched.IsCompleted);
    }

    [Fact]
    public async Task FullCrudFlow_Works()
    {
        // Create
        var createResponse = await _client.PostAsJsonAsync("/Todo", new CreateTodoRequest("Integration item"));
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<TodoItem>();
        Assert.NotNull(created);
        Assert.True(created.Id > SeedItems.Max(t => t.Id));
        Assert.Equal($"/Todo/{created.Id}", createResponse.Headers.Location?.AbsolutePath);

        // Read
        var fetched = await _client.GetFromJsonAsync<TodoItem>($"/Todo/{created.Id}");
        Assert.Equal(created, fetched);

        var all = await _client.GetFromJsonAsync<TodoItem[]>("/Todo");
        Assert.NotNull(all);
        Assert.Contains(all, t => t.Id == created.Id);

        // Update
        var updateResponse = await _client.PutAsJsonAsync($"/Todo/{created.Id}", new UpdateTodoRequest("Updated", true));
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<TodoItem>();
        Assert.NotNull(updated);
        Assert.Equal("Updated", updated.Title);
        Assert.True(updated.IsCompleted);

        // Delete
        var deleteResponse = await _client.DeleteAsync($"/Todo/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var afterDelete = await _client.GetAsync($"/Todo/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, afterDelete.StatusCode);
    }

    [Fact]
    public async Task Create_WithEmptyTitle_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/Todo", new { title = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Get_UnknownId_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/Todo/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
