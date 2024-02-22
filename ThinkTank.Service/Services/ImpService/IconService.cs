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
    public class IconService : IIconService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public IconService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IconResponse> CreateIcon(CreateIconRequest createIconRequest)
        {
            try
            {
                var icon = _mapper.Map<CreateIconRequest, Icon>(createIconRequest);
                var s = _unitOfWork.Repository<Icon>().Find(s => s.Name == createIconRequest.Name);
                if (s != null)
                    throw new CrudException(HttpStatusCode.BadRequest, "This icon has already !!!", "");
                if (createIconRequest.Price <= 0)
                    throw new CrudException(HttpStatusCode.BadRequest, "Price is invalid", "");

                icon.Status = true;
                await _unitOfWork.Repository<Icon>().CreateAsync(icon);
                await _unitOfWork.CommitAsync();
                return _mapper.Map<IconResponse>(icon);
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Add Icon Error!!!", ex?.Message);
            }
        }

        public async Task<IconResponse> GetIconById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Id Icon Invalid", "");
                }
                var response = _unitOfWork.Repository<Icon>().GetAll().Include(x=>x.IconOfAccounts).Select(x=>new IconResponse
                {
                    Name = x.Name,
                    Avatar=x.Avatar,
                    Id=x.Id,
                    Price=x.Price,
                    Status=x.Status,
                    IconOfAccounts=new List<IconOfAccountResponse>(x.IconOfAccounts.Select(a=>new IconOfAccountResponse
                    {
                        Id=a.Id,
                        AccountId=a.AccountId,
                        IsAvailable=a.IsAvailable,
                        UserName=a.Account.UserName
                    }))
                }).SingleOrDefault(x => x.Id == id);

                if (response == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found icon with id {id.ToString()}", "");
                }
                return response;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get icon By ID Error!!!", ex.InnerException?.Message);
            }
        }

        public async Task<PagedResults<IconResponse>> GetIcons(IconRequest request, PagingRequest paging)
        {
            try
            {
                var query = _unitOfWork.Repository<Icon>().GetAll().Include(x => x.IconOfAccounts).Select(x => new IconResponse
                {
                    Name = x.Name,
                    Avatar = x.Avatar,
                    Id = x.Id,
                    Price = x.Price,
                    Status = x.Status,
                    IconOfAccounts = new List<IconOfAccountResponse>(x.IconOfAccounts.Select(a => new IconOfAccountResponse
                    {
                        Id = a.Id,
                        AccountId = a.AccountId,
                        IsAvailable = a.IsAvailable,
                        UserName = a.Account.UserName
                    })).ToList()
                });

                if (request.MinPrice != null || request.MaxPrice != null)
                {
                    query = query.Where(a => (request.MinPrice == null || a.Price >= request.MinPrice) && (request.MaxPrice == null || a.Price <= request.MaxPrice));
                }
                if (request.AccountId != null)
                {
                    query = query.AsEnumerable().Where(x => x.IconOfAccounts.Any(a => a.AccountId == request.AccountId)).AsQueryable();
                }
                var filter = _mapper.Map<IconResponse>(request);
                var rs = query.DynamicFilter(filter);

                var sort = PageHelper<IconResponse>.Sorting(paging.SortType, rs, paging.ColName);
                var result = PageHelper<IconResponse>.Paging(sort, paging.Page, paging.PageSize);

                return result;
            }
            catch (CrudException ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get icon list error!!!!!", ex.Message);
            }

        }

        public async Task<IconResponse> GetToUpdateStatus(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Id Icon Invalid", "");
                }
                Icon icon = _unitOfWork.Repository<Icon>()
                      .Find(c => c.Id == id);

                if (icon == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found icon with id{id.ToString()}", "");
                }
                icon.Status = !icon.Status;
                await _unitOfWork.Repository<Icon>().Update(icon, id);
                await _unitOfWork.CommitAsync();
                return _unitOfWork.Repository<Icon>().GetAll().Include(x => x.IconOfAccounts).Select(x => new IconResponse
                {
                    Name = x.Name,
                    Avatar = x.Avatar,
                    Id = x.Id,
                    Price = x.Price,
                    Status = x.Status,
                    IconOfAccounts = new List<IconOfAccountResponse>(x.IconOfAccounts.Select(a => new IconOfAccountResponse
                    {
                        Id = a.Id,
                        AccountId = a.AccountId,
                        IsAvailable = a.IsAvailable,
                        UserName = a.Account.UserName
                    }))
                }).SingleOrDefault(x => x.Id == id);
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Update status icon error!!!!!", ex.Message);
            }
        }
        public async Task<IconOfAccountResponse> CreateIconOfAccount(IconOfAccountRequest request)
        {
            try
            {
                Icon icon = _unitOfWork.Repository<Icon>()
                      .Find(c => c.Id == request.IconId);

                if (icon == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found icon with id{request.IconId}", "");
                }

                Account account = _unitOfWork.Repository<Account>().Find(x => x.Id == request.AccountId);
                if (account == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found account with id {request.AccountId}", "");

               var rs= _mapper.Map<IconOfAccountRequest, IconOfAccount>(request);
                rs.IsAvailable = true;

                if (account.Coin < icon.Price || account.Coin ==null)
                    throw new CrudException(HttpStatusCode.BadRequest, "Not enough coin to buy icon","");
                account.Coin = account.Coin - icon.Price;
                await _unitOfWork.Repository<IconOfAccount>().CreateAsync(rs);
                await _unitOfWork.Repository<Account>().Update(account, request.AccountId);
                await _unitOfWork.CommitAsync();
                var response = _mapper.Map<IconOfAccountResponse>(rs);
                response.UserName=account.UserName;
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
    }
}
