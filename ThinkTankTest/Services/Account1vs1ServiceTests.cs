using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using ThinkTank.Application.Services.IService;
using ThinkTank.Test;

namespace ThinkTank.Test.Services
{
    public class Account1vs1ServiceTests : BaseTest
    {
        private IAccountIn1vs1Service _accountIn1vs1;

        [SetUp]
        public void SetUp()
        {
            _accountIn1vs1 = ServiceProvider.GetRequiredService<IAccountIn1vs1Service>();
        }

        [Test]
        public async Task Get()
        {
            // Arrange

            var result =  _accountIn1vs1.FindAccountTo1vs1(19, 20, 1);
            

            var result1 =  _accountIn1vs1.FindAccountTo1vs1(14, 20, 1);
            

            var result2 =  _accountIn1vs1.FindAccountTo1vs1(64, 20, 1);

            var result3 =  _accountIn1vs1.FindAccountTo1vs1(67, 20, 1);


            var result4 = _accountIn1vs1.FindAccountTo1vs1(68, 20, 1);


            var result5 = _accountIn1vs1.FindAccountTo1vs1(69, 20, 1);


            var result6 =  _accountIn1vs1.FindAccountTo1vs1(70, 20, 1);
      

            var result7 = _accountIn1vs1.FindAccountTo1vs1(74, 20, 1);


            var result8 =  _accountIn1vs1.FindAccountTo1vs1(75, 20, 1);


            var rs=await Task.WhenAll(result, result1, result2, result3,result4,result5,result6,result7,result8);
            for (int i = 0; i < rs.Length; i++)
            {
                Console.WriteLine($"Result {i + 1}: {rs[i]}");
            }
            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result1);
            Assert.IsNotNull(result2);
            Assert.IsNotNull(result3);


        }
    }
}