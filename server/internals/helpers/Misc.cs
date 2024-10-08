using System.Security.Cryptography;

namespace server.internals.helpers
{
    public enum ErrorCodes
    {
        NO_ERROR,
        DB_TRANSACTION_FAILED
    }
    public enum UserStatuses
    {
        ONLINE, 
        OFFLINE
    }

    public class CustomRandom
    {
        public static ulong GenerateRandomULong()
        {
            byte[] buffer = RandomNumberGenerator.GetBytes(8);
            return BitConverter.ToUInt64(buffer, 0);
        }
    }
}
