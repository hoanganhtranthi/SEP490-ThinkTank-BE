
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.RegularExpressions;
using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.CQRS.Accounts.DomainServices.IService;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.Services.IService;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.Accounts.Queries.GetAccountToLogin
{
    public class GetAccountToLoginQueryHandler : IQueryHandler<GetAccountToLoginQuery, AccountResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHashPasswordService _hashPasswordService;
        private readonly ISlackService _slackService;
        public GetAccountToLoginQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IHashPasswordService hashPasswordService, ISlackService slackService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _hashPasswordService = hashPasswordService;
            _slackService = slackService;
        }

        public async Task<AccountResponse> Handle(GetAccountToLoginQuery request, CancellationToken cancellationToken)
        {
            try
            {

                Account user = new Account();

                if (request.Username != null && request.Password == null || request.Username == null && request.Password != null || request.Username != "" && request.Password == ""
                    || request.Username == "" && request.Password != "")
                    throw new CrudException(HttpStatusCode.BadRequest, "Please enter both of username and password to get account", "");

                if (request.Username != null && request.Password != null && request.Username != "" && request.Password != "")
                {
                    user = _unitOfWork.Repository<Account>().Find(u => u.UserName.Equals(request.Username));

                    if (user == null) throw new CrudException(HttpStatusCode.NotFound, "Account Not Found", "");

                    if (user.GoogleId != null && user.GoogleId != "")
                        throw new CrudException(HttpStatusCode.BadRequest, "Login Google cannot login by username and password", "");

                    if (user.Status == false) throw new CrudException(HttpStatusCode.BadRequest, "Your account is block", "");

                    if (!_hashPasswordService.VerifyPasswordHash(request.Password.Trim(), user.PasswordHash, user.PasswordSalt))
                        throw new CrudException(HttpStatusCode.BadRequest, "Password is incorrect", "");

                    if (!Regex.IsMatch(request.Password, "^(?=.*\\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[a-zA-Z]).{8,12}$"))
                        throw new CrudException(HttpStatusCode.BadRequest, "Password is invalid", "");
                }
                if (request.GoogleId != null)
                {
                    user = _unitOfWork.Repository<Account>().GetAll().AsNoTracking().SingleOrDefault(u => u.GoogleId.Equals( request.GoogleId));

                    if (user == null)
                        throw new CrudException(HttpStatusCode.NotFound, "Account Not Found", "");

                    if (user.Status == false)
                        throw new CrudException(HttpStatusCode.BadRequest, "Your account is block", "");
                }
                return _mapper.Map<Account, AccountResponse>(user);
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                await _slackService.SendMessage(_slackService.CreateMessage(ex, "Get account to login error!!!!!"));
                throw new CrudException(HttpStatusCode.InternalServerError, "Get account to login error!!!!!", ex.Message);
            }
        }
    }
}
