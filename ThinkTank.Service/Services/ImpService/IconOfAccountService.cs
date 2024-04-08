using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Data.Entities;
using ThinkTank.Data.UnitOfWork;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;
using ThinkTank.Service.Exceptions;
using ThinkTank.Service.Helpers;
using ThinkTank.Service.Services.IService;
using ThinkTank.Service.Utilities;

namespace ThinkTank.Service.Services.ImpService
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
                    throw new CrudException(HttpStatusCode.BadRequest, "Account Not Available!!!!!", "");
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
                var query = _unitOfWork.Repository<IconOfAccount>().GetAll().AsNoTracking().Include(x=>x.Account).Include(x=>x.Icon)
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
                var sort = PageHelper<IconOfAccountResponse>.Sorting(paging.SortType, query, paging.ColName);
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
