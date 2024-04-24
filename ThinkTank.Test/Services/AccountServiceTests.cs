using Microsoft.Extensions.DependencyInjection;
using ThinkTank.Data.Entities;
using ThinkTank.Service.Services.IService;

namespace ThinkTank.Test.Services
{
    public class AccountServiceTests : BaseTest
    {
        private IAccountService _accountService = null!;

        [SetUp]
        public void SetUp()
        {
            _accountService = ServiceProvider.GetRequiredService<IAccountService>();
        }

        [Test]
        public async Task Get()
        {
            // Arrange
            var project = new Account();
            await Context.AddAsync(project);
            await Context.SaveChangesAsync();

            // Act
            var result = await _accountService.CreateAccount(project.Id);

            // Assert
            Assert.IsNotNull(result);
        }
    }
}