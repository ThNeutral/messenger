using Microsoft.Data.SqlClient;
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
            catch (DbUpdateException ex)
            {
                Console.WriteLine(ex.ToString());
                if (ex.InnerException != null && ex.InnerException is SqlException sqlEx && (sqlEx.Number == 2627 || sqlEx.Number == 2601)) 
                {
                    return ErrorCodes.UNIQUE_CONSTRAINT_VIOLATION;
                } 
                else
                {
                    return ErrorCodes.DB_TRANSACTION_FAILED;
                }
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
                var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.Username.Equals(username));
                if (user != null && user.Password == BCrypt.Net.BCrypt.HashPassword(password, user.Salt)) 
                {
                    return (user, ErrorCodes.NO_ERROR);                
                }
                return (null, ErrorCodes.FAILED_TO_FIND_GIVEN_ENTRY);
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
                var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.Email.Equals(email));
                if (user != null && user.Password == BCrypt.Net.BCrypt.HashPassword(password, user.Salt))
                {
                    return (user, ErrorCodes.NO_ERROR);
                }
                return (null, ErrorCodes.FAILED_TO_FIND_GIVEN_ENTRY);
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
                var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.Token.JWToken.Equals(token));
                if (user  == null)
                {
                    return (null, ErrorCodes.FAILED_TO_FIND_GIVEN_ENTRY);
                }
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
                if (user == null)
                {
                    return (null, ErrorCodes.FAILED_TO_FIND_GIVEN_ENTRY);
                }
                if (user.Count != ids.Length)
                {
                    return (null, ErrorCodes.FAILED_TO_FIND_SOME_ENTRIES);
                }
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
