using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoModel;

namespace TodoService.Controllers
{
   [Route("api/[controller]")]
   [ApiController]
   public class TodoItemsController : ControllerBase
   {
      /// <summary>
      /// ref: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-6.0
      /// </summary>
      private readonly IHttpClientFactory _httpClientFactory;

      /// <summary>
      /// Use HttpClient extension methods for json serialise/deserialise ( System.Text.Json implementation )
      /// ref: https://docs.microsoft.com/en-us/dotnet/api/system.net.http.json.httpclientjsonextensions?view=net-5.0
      /// alternatively create similar extension methods with Newtonsoft implementation.
      /// </summary>
      private readonly HttpClient _httpClient;

      public TodoItemsController(IHttpClientFactory httpClientFactory)
      {
         _httpClientFactory = httpClientFactory;
         _httpClient = _httpClientFactory.CreateClient("TodoService");
         _httpClient.BaseAddress = new Uri(Environment.GetEnvironmentVariable("TODO_SERVICE_BASE_URL") ??
                                           throw new InvalidOperationException());
      }

      #region helpers

      /// <summary>
      /// Relays simple no object results from internal HTTP Service endpoints.
      /// </summary>
      /// <param name="responseMessage"></param>
      /// <returns></returns>
      private IActionResult GetActionResult(HttpResponseMessage responseMessage)
      {
         switch (responseMessage.StatusCode)
         {
            case HttpStatusCode.BadRequest:
               return BadRequest();
            case HttpStatusCode.NotFound:
               return NotFound();
            default:
               return NoContent();
         }
      }

      #endregion

      #region snippet

      // GET: api/TodoItems
      [HttpGet]
      public async Task<ActionResult<IEnumerable<TodoItemDTO>>> GetTodoItems()
      {
         // TODO: Override .NET HttpClient ... extensions to handle expected results without exceptions - eg empty array. May want to use NewtonSoft if going to that trouble!
         var response = await _httpClient.GetFromJsonAsync<ActionResult<IEnumerable<TodoItemDTO>>>($"/api/TodoItems");
         return response;
      }

      [HttpGet("{id}")]
      public async Task<ActionResult<TodoItemDTO>> GetTodoItem(long id)
      {
         // TODO: Override .NET HttpClient ... extensions to handle expected results without exceptions - eg not found which has useful response from DB
         var response = await _httpClient.GetFromJsonAsync<ActionResult<TodoItemDTO>>($"/api/TodoItems/{id}");
         return response;
      }

      [HttpPut("{id}")]
      public async Task<IActionResult> UpdateTodoItem(long id, TodoItemDTO todoItemDTO)
      {
         var response = await _httpClient.PutAsJsonAsync<TodoItemDTO>($"/api/TodoItems/{id}", todoItemDTO);
         return GetActionResult(response);
      }

      [HttpPost]
      public async Task<ActionResult<TodoItemDTO>> CreateTodoItem(TodoItemDTO todoItemDTO)
      {
         var response = await _httpClient.PostAsJsonAsync<TodoItemDTO>($"/api/TodoItems", todoItemDTO);
         TodoItemDTO todoItemDto = await response.Content.ReadFromJsonAsync<TodoItemDTO>();

         return CreatedAtAction(nameof(GetTodoItem), new {id = todoItemDto.Id}, todoItemDto);
      }

      [HttpDelete("{id}")]
      public async Task<IActionResult> DeleteTodoItem(long id)
      {
         var response = await _httpClient.DeleteAsync($"/api/TodoItems/{id}");
         return GetActionResult(response);
      }

      #endregion

      // This method is just for testing populating the secret field
      // POST: api/TodoItems/test
      [HttpPost("test")]
      public async Task<ActionResult<TodoItem>> PostTestTodoItem(TodoItem todoItem)
      {
         var response = await _httpClient.PostAsJsonAsync<TodoItem>($"/api/TodoItems/test", todoItem);
         TodoItem responseTodoItem = await response.Content.ReadFromJsonAsync<TodoItem>();

         return CreatedAtAction(nameof(GetTodoItem), new {id = responseTodoItem.Id}, responseTodoItem);
      }

      // This method is just for testing
      // GET: api/TodoItems/test
      [HttpGet("test")]
      public async Task<ActionResult<IEnumerable<TodoItem>>> GetTestTodoItems()
      {
         var response = await _httpClient.GetFromJsonAsync<ActionResult<IEnumerable<TodoItem>>>($"/api/TodoItems/test");
         return response;
      }
   }
}