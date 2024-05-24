

using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.Services.IService
{
    public interface IAccountInRoomService
    {
        Task<PagedResults<AccountInRoomResponse>> GetAccountInRooms(AccountInRoomRequest accountInRoomRequest, PagingRequest paging);
        Task<AccountInRoomResponse> UpdateAccountInRoom(string roomCode,CreateAndUpdateAccountInRoomRequest createAccountInRoomRequest);
        Task<AccountInRoomResponse> GetAccountInRoomById(int id);

    }
}
