using devops_jenkins_aspnet_todo.api.Controllers;
using Microsoft.AspNetCore.Mvc;
using static devops_jenkins_aspnet_todo.api.Controllers.TodoController;

namespace devops_jenkins_aspnet_todo.test;

// The store is static and pre-seeded with SeedItems. Tests never modify seed items;
// tests that mutate data create their own items and assert by id.
public class TodoControllerTests
{
    private readonly TodoController _controller = new();

    private TodoItem CreateItem(string title)
    {
        var result = Assert.IsType<CreatedAtActionResult>(_controller.Create(new CreateTodoRequest(title)).Result);
        return Assert.IsType<TodoItem>(result.Value);
    }

    [Fact]
    public void SeedItems_AreNotEmpty()
    {
        Assert.NotEmpty(SeedItems);
        Assert.Equal(SeedItems.Count, SeedItems.Select(t => t.Id).Distinct().Count());
    }

    [Fact]
    public void GetAll_ReturnsSeedItems()
    {
        var result = Assert.IsType<OkObjectResult>(_controller.GetAll().Result);
        var items = Assert.IsAssignableFrom<IEnumerable<TodoItem>>(result.Value).ToList();

        Assert.NotEmpty(items);
        foreach (var seed in SeedItems)
        {
            Assert.Contains(seed, items);
        }
    }

    [Fact]
    public void GetById_SeedItem_ReturnsItem()
    {
        var seed = SeedItems[0];

        var result = Assert.IsType<OkObjectResult>(_controller.GetById(seed.Id).Result);

        Assert.Equal(seed, result.Value);
    }

    [Fact]
    public void Create_ReturnsCreatedItem()
    {
        var item = CreateItem("  Buy milk  ");

        Assert.True(item.Id > SeedItems.Max(t => t.Id));
        Assert.Equal("Buy milk", item.Title);
        Assert.False(item.IsCompleted);
    }

    [Fact]
    public void Create_WithBlankTitle_ReturnsBadRequest()
    {
        var result = _controller.Create(new CreateTodoRequest("   "));

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public void GetById_ReturnsItem()
    {
        var created = CreateItem("Read book");

        var result = Assert.IsType<OkObjectResult>(_controller.GetById(created.Id).Result);

        Assert.Equal(created, result.Value);
    }

    [Fact]
    public void GetById_Unknown_ReturnsNotFound()
    {
        Assert.IsType<NotFoundResult>(_controller.GetById(-1).Result);
    }

    [Fact]
    public void GetAll_ContainsCreatedItem()
    {
        var created = CreateItem("Walk dog");

        var result = Assert.IsType<OkObjectResult>(_controller.GetAll().Result);
        var items = Assert.IsAssignableFrom<IEnumerable<TodoItem>>(result.Value);

        Assert.Contains(created, items);
    }

    [Fact]
    public void Update_ChangesTitleAndCompletion()
    {
        var created = CreateItem("Old title");

        var result = Assert.IsType<OkObjectResult>(
            _controller.Update(created.Id, new UpdateTodoRequest("New title", true)).Result);
        var updated = Assert.IsType<TodoItem>(result.Value);

        Assert.Equal(created.Id, updated.Id);
        Assert.Equal("New title", updated.Title);
        Assert.True(updated.IsCompleted);
        Assert.Equal(created.CreatedAtUtc, updated.CreatedAtUtc);
    }

    [Fact]
    public void Update_Unknown_ReturnsNotFound()
    {
        Assert.IsType<NotFoundResult>(_controller.Update(-1, new UpdateTodoRequest("x", false)).Result);
    }

    [Fact]
    public void Delete_RemovesItem()
    {
        var created = CreateItem("Temp");

        Assert.IsType<NoContentResult>(_controller.Delete(created.Id));
        Assert.IsType<NotFoundResult>(_controller.GetById(created.Id).Result);
    }

    [Fact]
    public void Delete_Unknown_ReturnsNotFound()
    {
        Assert.IsType<NotFoundResult>(_controller.Delete(-1));
    }
}
