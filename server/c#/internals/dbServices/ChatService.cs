using Microsoft.EntityFrameworkCore;
using server.internals.dbMigrations;
using server.internals.dbMigrations.tables;
using server.internals.helpers;
using System;

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
        public async Task<ErrorCodes> AddUsersToExistingChat(Chat chat, IEnumerable<User> users)
        {
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
        public async Task<(Chat, ErrorCodes)> GetChatByChatID(ulong chat_id)
        {
            try
            {
                var chat = await _dbContext.Chats.SingleOrDefaultAsync(c => c.ChatID == chat_id);
                if (chat == null)
                {
                    return (null, ErrorCodes.FAILED_TO_FIND_GIVEN_ENTRY);
                }
                return (chat, ErrorCodes.NO_ERROR);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return (null, ErrorCodes.DB_TRANSACTION_FAILED);
            }
        }

        public async Task<ErrorCodes> DeleteChat(Chat chat)
        {
            try
            {
                _dbContext.Chats.Remove(chat);
                await _dbContext.SaveChangesAsync();
                return ErrorCodes.NO_ERROR;
            } 
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return ErrorCodes.DB_TRANSACTION_FAILED;
            }
        }

        public async Task<ErrorCodes> DeleteUsersFromChat(Chat chat, User[] users)
        {
            try
            {
                var ids = new List<ulong>();
                foreach (var user in users)
                {
                    ids.Add(user.UserID);
                }
                var toRemove = _dbContext.ChatToUsers.Where(ctu => ctu.ChatID == chat.ChatID && ids.Contains(ctu.UserID)).ToList();
                _dbContext.ChatToUsers.RemoveRange(toRemove);
                await _dbContext.SaveChangesAsync();
                return ErrorCodes.NO_ERROR;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return ErrorCodes.DB_TRANSACTION_FAILED;
            }
        }

        public async Task<(IEnumerable<Chat>, ErrorCodes)> GetChatsOfUser(User user)
        {
            try
            {
                var chats = _dbContext.ChatToUsers.Where(ctu => ctu.UserID == user.UserID).Select(ctu => ctu.Chat).ToList();
                return (chats, ErrorCodes.NO_ERROR);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return (null, ErrorCodes.DB_TRANSACTION_FAILED);
            }
        }
    }
}
