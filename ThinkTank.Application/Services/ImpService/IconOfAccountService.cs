using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Net;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.Helpers;
using ThinkTank.Application.Services.IService;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.Services.ImpService
{
    public class IconOfAccountService : IIconOfAccountService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public IconOfAccountService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IconOfAccountResponse> CreateIconOfAccount(CreateIconOfAccountRequest createIconRequest)
        {
            try
            {
                if(createIconRequest.AccountId <=0 || createIconRequest.IconId <=0 || createIconRequest.AccountId ==null || createIconRequest.IconId==null)
                    throw new CrudException(HttpStatusCode.BadRequest, "Information is invalid", "");

                IconOfAccount iconOfAccount = _unitOfWork.Repository<IconOfAccount>()
                      .Find(c => c.IconId == createIconRequest.IconId && c.AccountId==createIconRequest.AccountId);

                if (iconOfAccount != null)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, $"Account Id {createIconRequest.AccountId} purchased this icon {createIconRequest.IconId}", "");
                }

                Account account = _unitOfWork.Repository<Account>().Find(x => x.Id == createIconRequest.AccountId);

                if (account == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found account with id {createIconRequest.AccountId}", "");
                if (account.Status.Equals(false))
                {
                    throw new CrudException(HttpStatusCode.BadRequest, $"Account Id {account.Id} Not Available!!!!!", "");
                }

                Icon icon = _unitOfWork.Repository<Icon>().Find(x => x.Id == createIconRequest.IconId);
                if (icon == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found icon with id {createIconRequest.IconId}", "");
                if (icon.Status ==false)
                    throw new CrudException(HttpStatusCode.BadRequest, $"Icon Id {createIconRequest.IconId} is not available", "");

                var rs = _mapper.Map<CreateIconOfAccountRequest, IconOfAccount>(createIconRequest);
                rs.IsAvailable = true;

                if (account.Coin < icon.Price)
                    throw new CrudException(HttpStatusCode.BadRequest, "Not enough coin to buy icon", "");

                account.Coin = account.Coin - icon.Price;

                await _unitOfWork.Repository<IconOfAccount>().CreateAsync(rs);

                var badge = _unitOfWork.Repository<Badge>().GetAll().Include(x => x.Challenge).SingleOrDefault(x => x.AccountId == account.Id && x.Challenge.Name.Equals("The Tycoon"));
                if (badge.CompletedDate == null && badge.CompletedLevel < badge.Challenge.CompletedMilestone)
                {
                    badge.CompletedLevel = (int)account.Coin;
                    await _unitOfWork.Repository<Badge>().Update(badge, badge.Id);
                }

                await _unitOfWork.Repository<Account>().Update(account, createIconRequest.AccountId);
                await _unitOfWork.CommitAsync();

                var response = _mapper.Map<IconOfAccountResponse>(rs);
                response.UserName = account.UserName;
                response.IconAvatar = icon.Avatar;
                response.IconName = icon.Name;
                return response;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Buy Icon Error!!!!!", ex.Message);
            }
        }
        public async Task<PagedResults<IconOfAccountResponse>> GetIconOfAccounts(IconOfAccountRequest request, PagingRequest paging)
        {
            try
            {
                var filter = _mapper.Map<IconOfAccountResponse>(request);

                var iconOfAccountResponses = _unitOfWork.Repository<IconOfAccount>().GetAll().AsNoTracking()
                    .Include(x=>x.Account).Include(x=>x.Icon)
                    .Select(x=>new IconOfAccountResponse
                    {
                        Id = x.Id,
                        AccountId=x.AccountId,
                        UserName=x.Account.UserName,
                        IconAvatar=x.Icon.Avatar,
                        IconId=x.IconId,
                        IsAvailable=x.IsAvailable,
                        IconName=x.Icon.Name
                    }).DynamicFilter(filter).ToList();
                var sort = PageHelper<IconOfAccountResponse>.Sorting(paging.SortType, iconOfAccountResponses, paging.ColName);
                var result = PageHelper<IconOfAccountResponse>.Paging(sort, paging.Page, paging.PageSize);

                return result;
            }
            catch (CrudException ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get icon of account list error!!!!!", ex.Message);
            }
        }

        public async Task<IconOfAccountResponse> GetIconOfAccountById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Id Icon Invalid", "");
                }

                var response = _unitOfWork.Repository<IconOfAccount>().GetAll().AsNoTracking().Include(x => x.Icon).Include(x=>x.Account)
                     .Select(x => new IconOfAccountResponse
                     {
                         Id = x.Id,
                         AccountId = x.AccountId,
                         UserName = x.Account.UserName,
                         IconAvatar = x.Icon.Avatar,
                         IconId = x.IconId,
                         IsAvailable = x.IsAvailable,
                         IconName = x.Icon.Name
                     }).SingleOrDefault(x => x.Id == id);

                if (response == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found icon of account with id {id}", "");
                }
                return _mapper.Map<IconOfAccountResponse>(response);
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get icon of account By ID Error!!!", ex.InnerException?.Message);
            }
        }
    }
}
