
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.Services.IService
{
    public interface IIconOfAccountService
    {
        Task<PagedResults<IconOfAccountResponse>> GetIconOfAccounts(IconOfAccountRequest request, PagingRequest paging);
        Task<IconOfAccountResponse> CreateIconOfAccount(CreateIconOfAccountRequest createIconRequest);
        Task<IconOfAccountResponse> GetIconOfAccountById(int id);
    }
}
