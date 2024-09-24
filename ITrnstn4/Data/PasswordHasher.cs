using System.Security.Cryptography;
using ITrnstn4.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace ITrnstn4Old.Data
{
    public static class PasswordHasher
    {
        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public static bool VerifyPassword(string password, User user)
        {
            return BCrypt.Net.BCrypt.Verify(password, user.Password);
        }
    }

}
