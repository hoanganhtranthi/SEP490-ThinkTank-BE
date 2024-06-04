
using AutoMapper;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Text.RegularExpressions;
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.CQRS.Accounts.DomainServices.IService;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.Services.IService;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.Accounts.Commands.Login.Admin
{
    public class LoginAdminCommandHandler : ICommandHandler<LoginAdminCommand, AccountResponse>
    {
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;
        private readonly ITokenService _tokenHandler;
        private readonly DateTime date;
        private readonly IFirebaseRealtimeDatabaseService _firebaseRealtimeDatabaseService;

        public LoginAdminCommandHandler( IMapper mapper, IConfiguration config, ITokenService tokenHandler, IFirebaseRealtimeDatabaseService firebaseRealtimeDatabaseService)
        {
            _mapper = mapper;
            _config = config;
            _tokenHandler = tokenHandler;
            if (TimeZoneInfo.Local.BaseUtcOffset != TimeSpan.FromHours(7))
                date = DateTime.UtcNow.ToLocalTime().AddHours(7);
            else date = DateTime.Now;
            _firebaseRealtimeDatabaseService = firebaseRealtimeDatabaseService;
        }

        public async Task<AccountResponse> Handle(LoginAdminCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.LoginRequest.UserName == null || request.LoginRequest.UserName == "" || request.LoginRequest.Password == null || request.LoginRequest.Password == "")
                    throw new CrudException(HttpStatusCode.BadRequest, "Information is required", "");

                if (!Regex.IsMatch(request.LoginRequest.Password, "^(?=.*\\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[a-zA-Z]).{8,12}$"))
                    throw new CrudException(HttpStatusCode.BadRequest, "Password is invalid", "");

                Account user = new Account();
                AccountResponse rs = new AccountResponse();

                if (request.LoginRequest.UserName.Equals(_config["UsernameAdmin"]))
                {
                    if (!request.LoginRequest.Password.Equals(_config["PasswordAdmin"]))
                        throw new CrudException(HttpStatusCode.BadRequest, "Password is incorrect", "");

                    rs = _mapper.Map<Account, AccountResponse>(user);
                    rs.UserName = "Admin";
                    user.FullName = "Admin";

                    var adminAccount = _firebaseRealtimeDatabaseService.GetAsync<AdminAccountResponse>("AdminAccount").Result;
                    if (adminAccount != null)
                    {
                        adminAccount.VersionTokenAdmin += 1;
                        user.VersionToken = adminAccount.VersionTokenAdmin;

                        rs.AccessToken = _tokenHandler.GenerateJwtToken(user,date);
                        var token = _tokenHandler.GenerateRefreshToken(user,date);
                        rs.RefreshToken = token;
                        adminAccount.RefreshTokenAdmin = token;

                        await _firebaseRealtimeDatabaseService.SetAsync<AdminAccountResponse>("AdminAccount", adminAccount);
                    }
                    else
                    {
                        adminAccount = new AdminAccountResponse();
                        adminAccount.VersionTokenAdmin = 1;
                        user.VersionToken = 1;

                        rs.AccessToken = _tokenHandler.GenerateJwtToken(user, date);
                        var token = _tokenHandler.GenerateRefreshToken(user, date);
                        rs.RefreshToken = token;
                        adminAccount.RefreshTokenAdmin = token;

                        await _firebaseRealtimeDatabaseService.SetAsync<AdminAccountResponse>("AdminAccount", adminAccount);
                    }
                }
                else throw new CrudException(HttpStatusCode.NotFound, "Account Not Found", "");
                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Login  Fail", ex.InnerException?.Message);
            }
        }
    }
}
