using Microsoft.IdentityModel.Tokens;
using server.internals.helpers;
using System.Collections.Concurrent;
using Sprache;
using System.Net.WebSockets;
using System.Text.Json;
using Azure;

public struct WSMessage
{
    public string type;
    public string message;
}

namespace server.internals.websocketServices
{
    public class WebsocketChatHandler
    {
        private static ConcurrentDictionary<string, ConcurrentDictionary<WebSocket, bool>> connectedUsers = new ConcurrentDictionary<string, ConcurrentDictionary<WebSocket, bool>>();
        public static async Task HandleChat(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                await WriteError(context, "Expected WebSocket request", 400);
                return;
            }
            var id = context.Request.RouteValues["id"]?.ToString();
            await ProcessWebScoketConnection(context, id);
        }

        private static async Task ProcessWebScoketConnection(HttpContext context, string? chat_id)
        {
            if (chat_id == null)
            {
                await WriteError(context, "Expected chat id", 400);
                return;
            }
            using var ws = await context.WebSockets.AcceptWebSocketAsync();
            if (!connectedUsers.ContainsKey(chat_id))
            {
                connectedUsers[chat_id] = new ConcurrentDictionary<WebSocket, bool>();
            }
            connectedUsers[chat_id][ws] = true;
            var buffer = new byte[1024 * 4];
            while (ws.State != WebSocketState.Closed && ws.State != WebSocketState.Aborted)  
            {
                var res = await ws.ReceiveAsync(buffer, CancellationToken.None);
                var message = ProcessIncomingBytes(buffer, res.Count);
                await BroadcastMessageToAnotherUsers(message, connectedUsers[chat_id], ws);
            }
            bool a;
            connectedUsers[chat_id].Remove(ws, out a);
        }

        private static async Task WriteError(HttpContext context, string message, int code)
        {
            context.Response.StatusCode = code;
            var errorResponse = new ErrorResponse { errorMessage = message };
            var json = JsonSerializer.Serialize(errorResponse);
            await context.Response.WriteAsync(json);
        }

        private static WSMessage ProcessIncomingBytes(byte[] bytes, int len)
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
                return new WSMessage { message = "Failed to process JSON", type = "json-err"};
            }
            
        }

        private static async Task BroadcastMessageToAnotherUsers(WSMessage msg, ConcurrentDictionary<WebSocket, bool> conns, WebSocket ws)
        {
            var str = JsonSerializer.Serialize(msg);
            foreach (var conn in conns)
            {
                if (conn.Key == ws)
                {
                    continue;
                }
                await conn.Key.SendAsync(new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes(str)), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }
}
