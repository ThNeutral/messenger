using server.internals.dbMigrations;
using server.internals.dbMigrations.tables;
using server.internals.helpers;

namespace server.internals.dbServices
{
    public class ChatService
    {
        private MessengerDBContext _dbContext;
        public ChatService(MessengerDBContext dbContext) { _dbContext = dbContext; }

        public async Task<(Chat, ErrorCodes)> CreateChat(User user, string chatName)
        {

            var Chat = new Chat
            {
                ChatID = CustomRandom.GenerateRandomULong(),
                ChatName = chatName,
                ChatsToUsers = new List<ChatToUser>()
            };
            var ChatToUser = new ChatToUser 
            { 
                User = user,
                Chat = Chat
            };
            Chat.ChatsToUsers.Add(ChatToUser);
            try
            {
                _dbContext.Chats.Add(Chat);
                await _dbContext.SaveChangesAsync();
                return (Chat, ErrorCodes.NO_ERROR); 
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return (null, ErrorCodes.DB_TRANSACTION_FAILED);
            }
        }
        public async Task<ErrorCodes> AddUsersToExistingChat(ulong chat_id, IEnumerable<User> users)
        {
            var chat = _dbContext.Chats.Single(c => c.ChatID == chat_id);
            var ctus = new List<ChatToUser>();
            foreach (var user in users)
            {
                var ChatToUser = new ChatToUser
                {
                    User = user,
                    Chat = chat
                };
                ctus.Add(ChatToUser);
            }
            try
            {
                _dbContext.ChatToUsers.AddRange(ctus);
                await _dbContext.SaveChangesAsync();
                return ErrorCodes.NO_ERROR;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return ErrorCodes.DB_TRANSACTION_FAILED;
            }
        }
        public async Task<(List<User>, ErrorCodes)> GetUsersFromChat(Chat chat)
        {
            try
            {
                var users = _dbContext.ChatToUsers
                    .Where(ctu => ctu.ChatID == chat.ChatID)
                    .Select(ctu => ctu.User).ToList();

                return (users, ErrorCodes.NO_ERROR);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return (null, ErrorCodes.DB_TRANSACTION_FAILED);
            }
        }
    }
}
