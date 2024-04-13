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
        public async Task<IconResponse> GetIconById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Id Icon Invalid", "");
                }
                var response = _unitOfWork.Repository<Icon>().Find(x => x.Id == id);

                if (response == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found icon with id {id}", "");
                }
                return _mapper.Map<IconResponse>(response);
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
                var query = _unitOfWork.Repository<Icon>().GetAll()
                    .Include(x => x.IconOfAccounts)
                    .AsNoTracking()
                    .ProjectTo<IconResponse>(_mapper.ConfigurationProvider);


                if (request.AccountId != null)
                {
                    var isTrue = request.StatusIcon == Helpers.Enum.StatusIconType.True;
                    var isFalse = request.StatusIcon == Helpers.Enum.StatusIconType.False;
                    var iconIds = await GetIconIdsByAccountId((int)request.AccountId);
                    if (isTrue)
                    {
                        query = query.Where(icon => iconIds.Contains(icon.Id));
                    }
                    if(isFalse)
                    {
                        query = query.Where(icon => !iconIds.Contains(icon.Id));
                    }
                }

                if (request.MinPrice != null || request.MaxPrice != null)
                {
                    query = query.Where(icon => (request.MinPrice == null || icon.Price >= request.MinPrice) && (request.MaxPrice == null || icon.Price <= request.MaxPrice));
                }


                var filter = _mapper.Map<IconResponse>(request);
                query = query.DynamicFilter(filter);

                var sortedIcons = PageHelper<IconResponse>.Sorting(paging.SortType, query, paging.ColName);
                var result = PageHelper<IconResponse>.Paging(sortedIcons, paging.Page, paging.PageSize);

                return result;
            }
            catch (CrudException ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get icon list error!!!!!", ex.Message);
            }
        }

        private async Task<List<int>> GetIconIdsByAccountId(int accountId)
        {
            try
            {
                var account = await _unitOfWork.Repository<Account>()
                    .GetAll()
                    .Include(x => x.IconOfAccounts)
                    .AsNoTracking()
                    .SingleOrDefaultAsync(a => a.Id == accountId);

                if (account != null)
                {
                    return account.IconOfAccounts.Select(x => x.IconId).ToList();
                }

                return new List<int>();
            }
            catch (CrudException ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get list icons of account id error!!!!!", ex.Message);
            }
        }


    }
}
