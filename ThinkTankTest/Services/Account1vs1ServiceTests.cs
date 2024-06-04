using MediatR;
using Microsoft.Extensions.DependencyInjection;
using ThinkTank.Application.CQRS.AccountIn1vs1s.Commands.FindAccountTo1vs1;

namespace ThinkTank.Test.Services
{
    public class Account1vs1ServiceTests : BaseTest
    {
        private IMediator mediator;

        [SetUp]
        public void SetUp()
        {
            mediator = ServiceProvider.GetRequiredService<IMediator>();
        }

        [Test]
        public async Task Get()
        {
            // Arrange

            var result =  mediator.Send(new FindAccountTo1vs1Command(1, 1, 20));
            
            var result1= mediator.Send(new FindAccountTo1vs1Command(1, 2, 20));

            var rs=await Task.WhenAll(result,result1);
            for (int i = 0; i < rs.Length; i++)
            {
                Console.WriteLine($"Result {i + 1}: {rs[i]}");
            }
            // Assert
            Assert.IsNotNull(result.Result);
            Assert.IsNotNull(result1.Result);
  


        }
    }
}