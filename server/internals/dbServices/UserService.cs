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
        public async Task<ErrorCodes> AddUserAsync(User user)
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
        public async Task<(User user, ErrorCodes error)> GetUserByUsername(string username)
        {
            try
            {
                var user = await _dbContext.Users.SingleAsync(u => u.Username == username);
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
