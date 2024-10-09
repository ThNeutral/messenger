using Microsoft.EntityFrameworkCore;
using server.internals.dbMigrations;
using server.internals.dbMigrations.tables;
using server.internals.helpers;

namespace server.internals.dbServices
{
    public class UserService
    {
        private MessengerDBContext _dbContext;
        public UserService(MessengerDBContext dbContext) { _dbContext = dbContext; }
        public async Task<ErrorCodes> AddUser(User user)
        {
            try
            {
                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync();
                return ErrorCodes.NO_ERROR;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return ErrorCodes.DB_TRANSACTION_FAILED;
            }
        }
        public async Task<(User user, ErrorCodes error)> GetUserByUsernameAndPassword(string username, string password)
        {
            try
            {
                var user = await _dbContext.Users.SingleAsync(u => u.Username.Equals(username));
                if (user != null && user.Password == BCrypt.Net.BCrypt.HashPassword(password, user.Salt)) 
                {
                    return (user, ErrorCodes.NO_ERROR);                
                }
                return (null, ErrorCodes.NO_ERROR);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return (null, ErrorCodes.DB_TRANSACTION_FAILED);
            }
        }
        public async Task<(User user, ErrorCodes error)> GetUserByEmailAndPassword(string email, string password)
        {
            try
            {
                var user = await _dbContext.Users.SingleAsync(u => u.Email.Equals(email));
                if (user != null && user.Password == BCrypt.Net.BCrypt.HashPassword(password, user.Salt))
                {
                    return (user, ErrorCodes.NO_ERROR);
                }
                return (null, ErrorCodes.NO_ERROR);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return (null, ErrorCodes.DB_TRANSACTION_FAILED);
            }
        }

        public async Task<(User user, ErrorCodes code)> GetUserByToken(string token)
        {
            try
            {
                var user = await _dbContext.Users.SingleAsync(u => u.Token.JWToken.Equals(token));
                return (user, ErrorCodes.NO_ERROR);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return (null, ErrorCodes.DB_TRANSACTION_FAILED);
            }
        }
        public async Task<(List<User> users, ErrorCodes code)> GetUsersByIDs(ulong[] ids)
        {
            try
            {
                var user = _dbContext.Users.Where(u => ids.Contains(u.UserID)).ToList();
                return (user, ErrorCodes.NO_ERROR);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return (null, ErrorCodes.DB_TRANSACTION_FAILED);
            }
        }
    }
}
