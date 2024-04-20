using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;

namespace ThinkTank.Service.Services.IService
{
    public interface IIconOfAccountService
    {
        Task<PagedResults<IconOfAccountResponse>> GetIconOfAccounts(IconOfAccountRequest request, PagingRequest paging);
        Task<IconOfAccountResponse> CreateIconOfAccount(CreateIconOfAccountRequest createIconRequest);
        Task<IconOfAccountResponse> GetIconOfAccountById(int id);
    }
}
