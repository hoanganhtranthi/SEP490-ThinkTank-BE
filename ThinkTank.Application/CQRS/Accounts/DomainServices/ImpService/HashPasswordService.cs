

using System.Security.Cryptography;
using System.Text;
using ThinkTank.Application.CQRS.Accounts.DomainServices.IService;

namespace ThinkTank.Application.Accounts.DomainServices.ImpServices
{
    public class HashPasswordService : IHashPasswordService
    {
        public void CreatPasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        public bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }
        public string GenerateRandomPassword()
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random();
            StringBuilder passwordBuilder = new StringBuilder();

            // Thêm một ký tự chữ thường, một ký tự chữ in hoa và một chữ số vào mật khẩu
            passwordBuilder.Append(validChars[random.Next(validChars.Length)]);
            passwordBuilder.Append(validChars[random.Next(26, 52)]); // Chữ in hoa
            passwordBuilder.Append(validChars[random.Next(52, 62)]); // Số

            int requiredLength = random.Next(8, 13); // Random độ dài từ 8 đến 12 ký tự
            while (passwordBuilder.Length < requiredLength)
            {
                passwordBuilder.Append(validChars[random.Next(validChars.Length)]);
            }

            return passwordBuilder.ToString();
        }
    }
}
