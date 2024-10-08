using Microsoft.IdentityModel.Tokens;
using server.internals.helpers;
using System.Net.WebSockets;
using System.Text.Json;

namespace server.internals.websocketServices
{
    public class WebsocketChatHandler
    {
        public static async Task HandleChat(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                var id = context.Request.RouteValues["id"]?.ToString();
                await ProcessWebScoketConnection(context, id);
            }
            else
            {
                await WriteError(context, "Expected WebSocket request", 400);
            }
        }

        private static async Task ProcessWebScoketConnection(HttpContext context, string? id)
        {
            using var ws = await context.WebSockets.AcceptWebSocketAsync();
            var buffer = new byte[1024 * 4];
            while (ws.State != WebSocketState.Closed && ws.State != WebSocketState.Aborted)  
            {
                var res = await ws.ReceiveAsync(buffer, CancellationToken.None);
                var message = System.Text.Encoding.UTF8.GetString(buffer, 0, res.Count);
                var response = message;
                await ws.SendAsync(new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes(response)), res.MessageType, res.EndOfMessage, CancellationToken.None);
            }
        }

        private static async Task WriteError(HttpContext context, string message, int code)
        {
            context.Response.StatusCode = code;
            var errorResponse = new ErrorResponse { errorMessage = message };
            var json = JsonSerializer.Serialize(errorResponse);
            await context.Response.WriteAsync(json);
        }
    }
}
