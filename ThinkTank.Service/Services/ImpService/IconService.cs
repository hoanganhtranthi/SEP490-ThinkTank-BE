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
                var response = _unitOfWork.Repository<Icon>().GetAll().Include(x=>x.IconOfAccounts).SingleOrDefault(x => x.Id == id);

                if (response == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found icon with id {id.ToString()}", "");
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
                var filter = _mapper.Map<IconResponse>(request);
                var query = _unitOfWork.Repository<Icon>().GetAll()
                    .ProjectTo<IconResponse>(_mapper.ConfigurationProvider).DynamicFilter(filter).ToList();

                if (request.MinPrice != null || request.MaxPrice != null)
                {
                    query = query.Where(a => (request.MinPrice == null || a.Price >= request.MinPrice) && (request.MaxPrice == null || a.Price <= request.MaxPrice)).ToList();
                }

                var sort = PageHelper<IconResponse>.Sorting(paging.SortType, query, paging.ColName);
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
                return _mapper.Map<IconResponse>(icon);
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
    }
}
