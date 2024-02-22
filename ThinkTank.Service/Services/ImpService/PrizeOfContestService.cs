using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
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
    public class PrizeOfContestService : IPrizeOfContestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PrizeOfContestService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PrizeOfContestResponse> CreatePrizeOfContest(CreatePrizeOfContestRequest createPrizeOfContestRequest)
        {
            try
            {
                var prize = _mapper.Map<CreatePrizeOfContestRequest, PrizeOfContest>(createPrizeOfContestRequest);
                var s = _unitOfWork.Repository<Contest>().Find(s => s.Id == createPrizeOfContestRequest.ContestId);
                if (s == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $" Contest Id {createPrizeOfContestRequest.ContestId} is not found !!!", "");
                }
                else
                {
                    var p = _unitOfWork.Repository<PrizeOfContest>().GetAll().Where(p => p.ContestId == s.Id).Select(c => new PrizeOfContestRequest
                    {
                        FromRank = c.FromRank,
                        ToRank = c.ToRank,
                    });
                    if (p == null)
                    {
                        prize.FromRank = createPrizeOfContestRequest.FromRank;
                        prize.ToRank = createPrizeOfContestRequest.ToRank;
                        prize.Prize = createPrizeOfContestRequest.Prize;
                        prize.ContestId = s.Id;
                        if (prize.FromRank >= prize.ToRank)
                        {
                            throw new CrudException(HttpStatusCode.BadRequest, "FromRank or ToRank is invalid", "");
                        }
                        await _unitOfWork.Repository<PrizeOfContest>().CreateAsync(prize);
                        await _unitOfWork.CommitAsync();
                    }
                    else
                    {
                        foreach (PrizeOfContestRequest p1 in p)
                        {
                            //if (khoang1[1] < khoang2[0] || khoang2[1] < khoang1[0])
                            if (!(createPrizeOfContestRequest.ToRank < p1.FromRank || p1.ToRank < createPrizeOfContestRequest.FromRank))
                            {
                                throw new CrudException(HttpStatusCode.BadRequest, "FromRank or ToRank is invalid", "");
                            }                           
                        }
                        prize.FromRank = createPrizeOfContestRequest.FromRank;
                        prize.ToRank = createPrizeOfContestRequest.ToRank;
                        prize.Prize = createPrizeOfContestRequest.Prize;
                        prize.ContestId = s.Id;
                        if (prize.FromRank >= prize.ToRank)
                        {
                            throw new CrudException(HttpStatusCode.BadRequest, "FromRank or ToRank is invalid", "");
                        }
                        await _unitOfWork.Repository<PrizeOfContest>().CreateAsync(prize);
                        await _unitOfWork.CommitAsync();
                    }
                    return _mapper.Map<PrizeOfContestResponse>(prize);
                }
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Create Prize of Contest Error!!!", ex?.Message);
            }
        }

        public async Task<PagedResults<PrizeOfContestResponse>> GetPrizeOfContests(ResourceOfContestRequest request, PagingRequest paging)
        {
            try
            {
                var filter = _mapper.Map<PrizeOfContestResponse>(request);
                var prizes = _unitOfWork.Repository<PrizeOfContest>().GetAll()
                                           .ProjectTo<PrizeOfContestResponse>(_mapper.ConfigurationProvider)
                                           .DynamicFilter(filter)
                                           .ToList();
                var sort = PageHelper<PrizeOfContestResponse>.Sorting(paging.SortType, prizes, paging.ColName);
                var result = PageHelper<PrizeOfContestResponse>.Paging(sort, paging.Page, paging.PageSize);
                return result;
            } catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get prize of contest list error!!!!!", ex.Message);
            }
        }
    }
}
//1-5: 200 6-10: 150
//create contest -> create prize 
// prize1: 3-5: 200 prize2: 11-20: 120 prize3: 50-100: 50
// prize3: <3 || >100 || >5 && <50

// prize1: 1-5: ?
// prize2: 6-10: ??

// if from > 5,20,100 && to < 3,11,50
//f = 6 t = 10
// foreach (from)
//  if (f > from[5;20;100]) //6>5 true
//   foreach (to)
//    if (t < to[3;11;50]) // 10<3 false


// prize1: 3-5: 200 prize2: 11-20: 120 prize3: 50-100: 50
// prize3: <3 || >100 && >5 && <50
//from = 5 to = 50

//data = [5;10] vs [50;100]
//[1;2]
/*
static bool KiemTraTrungKhoang(int[] khoang1, int[] khoang2)
{
    // Kiểm tra điều kiện để hai khoảng không trùng nhau:
    // Nếu khoảng thứ nhất kết thúc trước khi khoảng thứ hai bắt đầu,
    // hoặc nếu khoảng thứ hai kết thúc trước khi khoảng thứ nhất bắt đầu,
    // thì chúng không trùng nhau.
    if (khoang1[1] < khoang2[0] || khoang2[1] < khoang1[0]) // 2<3 || 3<1  => true; 7<3 || 7<6 => false
    {
        return false;
    }

    return true;
}

static void Main(string[] args)
{
    // Ví dụ với hai khoảng giá trị
    int[] khoang1 = { 1, 2 };
    int[] khoang3 = { 6, 7 };
    int[] khoang2 = { 3, 7 };

    // Kiểm tra xem hai khoảng có trùng nhau không
    bool trungNhau = KiemTraTrungKhoang(khoang1, khoang2);

    // Hiển thị kết quả
    if (trungNhau)
    {
        Console.WriteLine("Hai khoảng giá trị trùng nhau.");
    }
    else
    {
        Console.WriteLine("Hai khoảng giá trị không trùng nhau.");
    }
}
*/