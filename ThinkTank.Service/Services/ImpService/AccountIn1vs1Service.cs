using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Data.UnitOfWork;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;
using ThinkTank.Service.Services.IService;

namespace ThinkTank.Service.Services.ImpService
{
    public class AccountIn1vs1Service : IAccountIn1vs1Service
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public AccountIn1vs1Service(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public Task<AccountIn1vs1Response> CreateAccount1vs1(CreateAccountIn1vs1Request createAccount1vs1Request)
        {
            throw new NotImplementedException();
        }

        public Task<AccountIn1vs1Response> GetAccount1vs1ById(int id)
        {
            throw new NotImplementedException();
        }

        public Task<PagedResults<AccountIn1vs1Response>> GetAccount1vs1s(AccountIn1vs1Request request, PagingRequest paging)
        {
            throw new NotImplementedException();
        }
    }
}
