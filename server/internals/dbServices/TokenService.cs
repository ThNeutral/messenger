using server.internals.dbMigrations;
using server.internals.dbMigrations.tables;
using server.internals.helpers;

namespace server.internals.dbServices
{
    public class TokenService
    {
        private MessengerDBContext _dbContext;
        public TokenService(MessengerDBContext dbContext) { _dbContext = dbContext; }
        public async Task<(Token token, ErrorCodes code)> GenerateTokenForAUser(User user)
        {
            try
            {
                string t = Guid.NewGuid().ToString();
                t = t.Replace("=", "").Replace("+", "").Replace("-", "");
                var token = new Token { User = user, JWToken = t, ExpiresAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 3600L };
                _dbContext.Tokens.Add(token);
                await _dbContext.SaveChangesAsync();
                return (token, ErrorCodes.NO_ERROR);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return (null, ErrorCodes.DB_TRANSACTION_FAILED);
            }
        }
    }
}
