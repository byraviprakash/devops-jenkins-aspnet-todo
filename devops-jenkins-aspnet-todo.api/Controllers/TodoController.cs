using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace devops_jenkins_aspnet_todo.api.Controllers
{
    /// <summary>
    /// Self-contained ToDo feature: models, in-memory storage and endpoints all live in this controller.
    /// Data is held in a static dictionary that is seeded with <see cref="SeedItems"/> at startup; changes live
    /// for the lifetime of the process and are reset to the seed data on restart.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class TodoController : ControllerBase
    {
        // ---------- Models ----------

        public record TodoItem(int Id, string Title, bool IsCompleted, DateTime CreatedAtUtc);

        public record CreateTodoRequest(
            [Required, StringLength(200, MinimumLength = 1)] string Title);

        public record UpdateTodoRequest(
            [Required, StringLength(200, MinimumLength = 1)] string Title,
            bool IsCompleted);

        // ---------- In-memory store ----------

        /// <summary>
        /// Items loaded into the store when the application starts, so the API returns data straight away.
        /// </summary>
        public static readonly IReadOnlyList<TodoItem> SeedItems = new List<TodoItem>
        {
            new(1, "Set up Jenkins pipeline", true, new DateTime(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc)),
            new(2, "Write unit and integration tests", false, new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc)),
            new(3, "Deploy ToDo API", false, new DateTime(2026, 1, 1, 11, 0, 0, DateTimeKind.Utc)),
        };

        private static readonly ConcurrentDictionary<int, TodoItem> Store =
            new(SeedItems.Select(item => KeyValuePair.Create(item.Id, item)));

        // New ids continue after the highest seeded id.
        private static int _lastId = SeedItems.Max(item => item.Id);

        // ---------- Endpoints ----------

        [HttpGet]
        public ActionResult<IEnumerable<TodoItem>> GetAll()
        {
            return Ok(Store.Values.OrderBy(t => t.Id).ToList());
        }

        [HttpGet("{id:int}")]
        public ActionResult<TodoItem> GetById(int id)
        {
            return Store.TryGetValue(id, out var item) ? Ok(item) : NotFound();
        }

        [HttpPost]
        public ActionResult<TodoItem> Create([FromBody] CreateTodoRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return BadRequest("Title is required.");
            }

            var id = Interlocked.Increment(ref _lastId);
            var item = new TodoItem(id, request.Title.Trim(), false, DateTime.UtcNow);
            Store[id] = item;

            return CreatedAtAction(nameof(GetById), new { id }, item);
        }

        [HttpPut("{id:int}")]
        public ActionResult<TodoItem> Update(int id, [FromBody] UpdateTodoRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return BadRequest("Title is required.");
            }

            if (!Store.TryGetValue(id, out var existing))
            {
                return NotFound();
            }

            var updated = existing with { Title = request.Title.Trim(), IsCompleted = request.IsCompleted };
            Store[id] = updated;

            return Ok(updated);
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            return Store.TryRemove(id, out _) ? NoContent() : NotFound();
        }
    }
}
