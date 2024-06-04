namespace ThinkTank.Application.CQRS.Accounts.DomainServices.IService
{
    public interface IHashPasswordService
    {
        void CreatPasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt);
        bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt);
        string GenerateRandomPassword();
    }
}
