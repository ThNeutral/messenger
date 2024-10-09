using System.Security.Cryptography;

namespace server.internals.helpers
{
    public enum ErrorCodes
    {
        NO_ERROR,
        DB_TRANSACTION_FAILED,
        UNIQUE_CONSTRAINT_VIOLATION,
        FAILED_TO_FIND_GIVEN_ENTRY,
        FAILED_TO_FIND_SOME_ENTRIES
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
