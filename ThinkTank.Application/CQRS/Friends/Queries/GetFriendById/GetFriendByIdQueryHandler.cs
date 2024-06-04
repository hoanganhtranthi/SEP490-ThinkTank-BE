

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Net;
using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.CQRS.Friends.Queries.GetFriendById
{
    public class GetFriendByIdQueryHandler : IQueryHandler<GetFriendByIdQuery, FriendResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public GetFriendByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<FriendResponse> Handle(GetFriendByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var response = _unitOfWork.Repository<Friend>().GetAll()
                .AsNoTracking().Include(x => x.AccountId1Navigation).Include(x => x.AccountId2Navigation).FirstOrDefault(u => u.Id ==request.Id);

                if (response == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found friendship with id {request.Id}", "");
                }

                var rs = _mapper.Map<FriendResponse>(response);
                rs.UserName1 = response.AccountId1Navigation.UserName;
                rs.UserName2 = response.AccountId2Navigation.UserName;
                rs.Avatar1 = response.AccountId1Navigation.Avatar;
                rs.Avatar2 = response.AccountId2Navigation.Avatar;
                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get Friendship By ID Error!!!", ex.InnerException?.Message);
            }
        }
    }
}
