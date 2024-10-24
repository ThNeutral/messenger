using Microsoft.IdentityModel.Tokens;
using server.internals.helpers;
using System.Collections.Concurrent;
using Sprache;
using System.Net.WebSockets;
using System.Text.Json;
using Azure;
using Microsoft.AspNetCore.Mvc;
using server.internals.dbServices;

public struct WSMessage
{
    public string type;
    public string message;
}

namespace server.Controllers
{
    [ApiController]
    public class WebsocketChatController : AbstractController
    {
        private readonly UserService _userService;
        private ConcurrentDictionary<string, ConcurrentDictionary<WebSocket, bool>> connectedUsers;
        public WebsocketChatController(UserService userService) 
        {
            _userService = userService;
            connectedUsers = new ConcurrentDictionary<string, ConcurrentDictionary<WebSocket, bool>>();
        }
        [HttpGet("/chat/{id}")]
        public async Task<IActionResult> HandleChat()
        {
            var errorResponse = new ErrorResponse();
            HttpContext context = ControllerContext.HttpContext;
            if (!context.WebSockets.IsWebSocketRequest)
            {
                errorResponse.errorMessage = "Expected WebSocket request";
                return Unauthorized(errorResponse);
            }
            var chat_id = context.Request.RouteValues["id"]?.ToString();
            if (chat_id == null)
            {
                errorResponse.errorMessage = "Expected a route parameter";
                return BadRequest(errorResponse);
            }
            await ProcessWebSocketConnection(context, chat_id);
            return new EmptyResult();
        }

        private async Task ProcessWebSocketConnection(HttpContext context, string chat_id)
        {
            using var ws = await context.WebSockets.AcceptWebSocketAsync();
            if (!connectedUsers.ContainsKey(chat_id))
            {
                connectedUsers[chat_id] = new ConcurrentDictionary<WebSocket, bool>();
            }
            connectedUsers[chat_id][ws] = true;
            var buffer = new byte[1024 * 4];
            while (ws.State != WebSocketState.Closed && ws.State != WebSocketState.Aborted)
            {
                try
                {
                    var res = await ws.ReceiveAsync(buffer, CancellationToken.None);
                    var message = ProcessIncomingBytes(buffer, res.Count);
                    await BroadcastMessageToAnotherUsers(message, connectedUsers[chat_id], ws);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ws.State);
                    Console.WriteLine($"{ex.Message}");
                    continue;
                }
            }
            bool a;
            connectedUsers[chat_id].Remove(ws, out a);
        }

        private WSMessage ProcessIncomingBytes(byte[] bytes, int len)
        {
            try
            {
                var str = System.Text.Encoding.UTF8.GetString(bytes, 0, len);
                var msg = JsonSerializer.Deserialize<WSMessage>(str);
                switch (msg.type)
                {
                    case "msg":
                        {
                            return msg;
                        }
                    default:
                        {
                            return new WSMessage { };
                        }
                }
            }
            catch (JsonException e)
            {
                return new WSMessage { message = "Failed to process JSON", type = "json-err" };
            }

        }

        private async Task BroadcastMessageToAnotherUsers(WSMessage msg, ConcurrentDictionary<WebSocket, bool> conns, WebSocket ws)
        {
            var tasks = new List<Task>();
            var str = JsonSerializer.Serialize(msg);
            foreach (var conn in conns)
            {
                if (conn.Key == ws)
                {
                    continue;
                }
                tasks.Add(conn.Key.SendAsync(new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes(str)), WebSocketMessageType.Text, true, CancellationToken.None));
            }
            Parallel.ForEach(tasks, async task => await task);
        }
    }
}
