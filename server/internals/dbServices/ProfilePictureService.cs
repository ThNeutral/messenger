using Microsoft.Identity.Client;
using server.internals.dbMigrations;
using server.internals.dbMigrations.tables;
using server.internals.helpers;

namespace server.internals.dbServices
{
    public class ProfilePictureService
    {
        private MessengerDBContext _dbContext;
        public ProfilePictureService(MessengerDBContext dbContext) { _dbContext = dbContext; }
        public async Task<ErrorCodes> SetProfilePictureForUser(User user, string base64encodedimage)
        {

            return ErrorCodes.NO_ERROR;
        }
    }
}
