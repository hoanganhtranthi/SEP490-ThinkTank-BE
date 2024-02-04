using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;

namespace ThinkTank.Service.Services.IService
{
    public interface IAccountIn1vs1Service
    {
        Task<PagedResults<AccountIn1vs1Response>> GetAccount1vs1s(AccountIn1vs1Request request, PagingRequest paging);
        Task<AccountIn1vs1Response> CreateAccount1vs1(CreateAccountIn1vs1Request createAccount1vs1Request);
        Task<AccountIn1vs1Response> GetAccount1vs1ById(int id);
    }
}
