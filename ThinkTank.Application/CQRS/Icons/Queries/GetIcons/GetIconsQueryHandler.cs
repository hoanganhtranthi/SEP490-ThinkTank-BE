

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Net.NetworkInformation;
using System.Net;
using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.Helpers;
using ThinkTank.Application.UnitOfWork;
using static ThinkTank.Domain.Enums.Enum;
using ThinkTank.Domain.Entities;
using AutoMapper.QueryableExtensions;

namespace ThinkTank.Application.CQRS.Icons.Queries.GetIcons
{
    public class GetIconsQueryHandler : IQueryHandler<GetIconsQuery, PagedResults<IconResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public GetIconsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PagedResults<IconResponse>> Handle(GetIconsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var icons = _unitOfWork.Repository<Icon>().GetAll()
                    .Include(x => x.IconOfAccounts)
                    .AsNoTracking()
                    .ProjectTo<IconResponse>(_mapper.ConfigurationProvider);

                if (request.IconRequest.AccountId != null)
                {
                    var acc = _unitOfWork.Repository<Account>().Find(x => x.Id == request.IconRequest.AccountId);
                    if (acc == null)
                        throw new CrudException(HttpStatusCode.NotFound, $"Account Id {acc.Id} is not found", "");
                    if (acc.Status == false)
                        throw new CrudException(HttpStatusCode.BadRequest, $"Account Id {acc.Id} is block", "");

                    var isTrue = request.IconRequest.StatusIcon == StatusIconType.True;
                    var isFalse = request.IconRequest.StatusIcon == StatusIconType.False;
                    var iconsOfAccount = await GetIconIdsByAccountId((int)(request.IconRequest.AccountId));

                    if (isTrue)
                    {
                        icons = icons.Where(icon => iconsOfAccount.Contains(icon.Id));
                    }
                    if (isFalse)
                    {
                        icons = icons.Where(icon => !iconsOfAccount.Contains(icon.Id));
                    }
                }

                if (request.IconRequest.MinPrice != null || request.IconRequest.MaxPrice != null)
                {
                    icons = icons.Where(icon => (request.IconRequest.MinPrice == null || icon.Price >= request.IconRequest.MinPrice) && (request.IconRequest.MaxPrice == null || icon.Price <= request.IconRequest.MaxPrice));
                }
                var filter = _mapper.Map<IconResponse>(request);
                icons = icons.DynamicFilter(filter);

                var sortedIcons = PageHelper<IconResponse>.Sorting(request.PagingRequest.SortType, icons, request.PagingRequest.ColName);
                var result = PageHelper<IconResponse>.Paging(sortedIcons, request.PagingRequest.Page, request.PagingRequest.PageSize);

                return result;
            }
            catch (CrudException ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get icon list error!!!!!", ex.Message);
            }
        }
        private async Task<List<int>> GetIconIdsByAccountId(int accountId)
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
        }
    }
