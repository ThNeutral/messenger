using Microsoft.EntityFrameworkCore;
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
            try
            {
                var profilePicture = await _dbContext.ProfilePictures.SingleOrDefaultAsync(pp => pp.UserID == user.UserID);

                if (profilePicture == null)
                {
                    profilePicture = new ProfilePicture
                    {
                        UserID = user.UserID,
                        Base64EncodedImage = base64encodedimage,
                    };

                    _dbContext.ProfilePictures.Add(profilePicture);
                }
                else
                {
                    profilePicture.Base64EncodedImage = base64encodedimage;
                }

                await _dbContext.SaveChangesAsync();

                return ErrorCodes.NO_ERROR;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return ErrorCodes.DB_TRANSACTION_FAILED;
            }
        }
        public async Task<(ProfilePicture profilePicture, ErrorCodes error)> GetProfilePictureOfUser(User user)
        {
            try
            {
                var profilePicture = await _dbContext.ProfilePictures.SingleOrDefaultAsync(pp => pp.UserID == user.UserID);
                if (profilePicture == null)
                {
                    return (null, ErrorCodes.FAILED_TO_FIND_GIVEN_ENTRY);
                }
                return (profilePicture, ErrorCodes.NO_ERROR);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return (null, ErrorCodes.DB_TRANSACTION_FAILED);
            }
        }
    }
}
