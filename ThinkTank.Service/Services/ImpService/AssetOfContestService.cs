using AutoMapper;
using Repository.Extensions;
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
using ThinkTank.Service.Services.IService;

namespace ThinkTank.Service.Services.ImpService
{
    public class AssetOfContestService : IAssetOfContestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public AssetOfContestService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<AssetOfContestResponse> CreateAssetOfContest(CreateAssetOfContestRequest request)
        {
            try
            {
                var asset = _mapper.Map<CreateAssetOfContestRequest, AssetOfContest>(request);
                var c = _unitOfWork.Repository<Contest>().Find(c => c.Id == request.ContestId);
                if (c == null)
                {
                    throw new CrudException(HttpStatusCode.InternalServerError, "Contest Not Found!!!!!", "");
                }
                var t = _unitOfWork.Repository<TypeOfAssetInContest>().Find(t => t.Id == request.TypeOfAssetId);
                if (t == null)
                {
                    throw new CrudException(HttpStatusCode.InternalServerError, "Type Of Asset In Contest Not Found!!!!!", "");
                }

                AssetOfContest assetOfContest = new AssetOfContest();
                assetOfContest.Value = request.Value;
                assetOfContest.TypeOfAssetId = request.TypeOfAssetId;
                assetOfContest.ContestId = c.Id;
                await _unitOfWork.Repository<AssetOfContest>().CreateAsync(assetOfContest);
                await _unitOfWork.CommitAsync();// 'An error occurred while saving the entity changes. See the inner exception for details.'

                AssetOfContestResponse response = new AssetOfContestResponse();
                response.Id = assetOfContest.Id;
                response.Value = assetOfContest.Value;
                response.NameOfContest = c.Name;
                response.Type = t.Type;
                return response;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Create Asset Of Contest Error!!!", ex?.Message);
            }
        }
    }
}
