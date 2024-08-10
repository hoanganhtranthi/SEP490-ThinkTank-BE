
using AutoMapper;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.CQRS.Accounts.DomainServices.IService;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.Services.IService;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.Accounts.Commands.VerifyAndGenerateToken
{
    public class VerifyAndGenerateTokenCommandHandler : ICommandHandler<VerifyAndGenerateTokenCommand, AccountResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenService;
        private readonly DateTime date;
        private readonly IFirebaseRealtimeDatabaseService _firebaseRealtimeDatabaseService;
        private readonly ISlackService _slackService;
        public VerifyAndGenerateTokenCommandHandler(IUnitOfWork unitOfWork, IMapper mapper
            , IFirebaseRealtimeDatabaseService firebaseRealtimeDatabaseService,ITokenService tokenService, ISlackService slackService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            if (TimeZoneInfo.Local.BaseUtcOffset != TimeSpan.FromHours(7))
                date = DateTime.UtcNow.ToLocalTime().AddHours(7);
            else date = DateTime.Now;
            _firebaseRealtimeDatabaseService = firebaseRealtimeDatabaseService;
            _tokenService = tokenService;
            _slackService = slackService;
        }

        public async Task<AccountResponse> Handle(VerifyAndGenerateTokenCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.TokenRequest.AccessToken == null || request.TokenRequest.AccessToken == "" || request.TokenRequest.RefreshToken == null || request.TokenRequest.RefreshToken == "")
                    throw new CrudException(HttpStatusCode.BadRequest, "Information is required", "");

                AccountResponse cus = new AccountResponse();
                Account acc = new Account();
                var jwtTokenHandler = new JwtSecurityTokenHandler();

                JwtSecurityToken tokenPrincipal;


                //Check xem access token đúng là một JWTToken không
                try
                {
                    tokenPrincipal = jwtTokenHandler.ReadJwtToken(request.TokenRequest.AccessToken);
                }
                catch (ArgumentException)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Invalid Access Token", "");
                }

                //Lấy thời gian hết hạn trong token
                var utcExpiredDate = long.Parse(tokenPrincipal.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);
                //chuyển thành DateTime
                var expiredDate = DateTimeOffset.FromUnixTimeSeconds(utcExpiredDate).DateTime;

                if (tokenPrincipal.Claims.FirstOrDefault(x => x.Type == "role").Value.Equals("Admin"))
                {
                    var adminAccountResponse = _firebaseRealtimeDatabaseService.GetAsync<AdminAccountResponse>("AdminAccount").Result;
                    if (adminAccountResponse != null)
                    {
                        //Kiểm tra xem refresh token đầu vào có giống với refresh token hiện tại không
                        if (!request.TokenRequest.RefreshToken.Equals(adminAccountResponse.RefreshTokenAdmin))
                            throw new CrudException(HttpStatusCode.BadRequest, "Invalid Refresh Token", "");
                    }

                    //Check xem acess token đã hết hạn hay chưa
                    if (expiredDate > DateTime.UtcNow)
                        throw new CrudException(HttpStatusCode.BadRequest, "Access Token is not expried", "");

                    acc.FullName = "Admin";
                    acc.VersionToken = adminAccountResponse.VersionTokenAdmin + 1;
                    cus = _mapper.Map<Account, AccountResponse>(acc);
                    cus.AccessToken =_tokenService.GenerateJwtToken(acc,date);
                    cus.RefreshToken =_tokenService.GenerateRefreshToken(acc,date);

                    adminAccountResponse.RefreshTokenAdmin = cus.RefreshToken;
                    adminAccountResponse.VersionTokenAdmin = (int)acc.VersionToken;
                    await _firebaseRealtimeDatabaseService.SetAsync<AdminAccountResponse>("AdminAccount", adminAccountResponse);
                }
                else
                {
                    acc = _unitOfWork.Repository<Account>().Find(a => a.RefreshToken != null && a.RefreshToken.Equals(request.TokenRequest.RefreshToken));

                    if (acc == null)
                        throw new CrudException(HttpStatusCode.BadRequest, "Invalid Refresh Token", "");

                    if (expiredDate > DateTime.UtcNow)
                        throw new CrudException(HttpStatusCode.BadRequest, "Access Token is not expried", "");

                    if (acc.Status == false)
                        throw new CrudException(HttpStatusCode.BadRequest, "Your account is block", "");

                    acc.VersionToken = acc.VersionToken + 1;
                    acc.RefreshToken = _tokenService.GenerateRefreshToken(acc,date);

                    await _unitOfWork.Repository<Account>().Update(acc, acc.Id);
                    await _unitOfWork.CommitAsync();

                    cus = _mapper.Map<Account, AccountResponse>(acc);
                    cus.AccessToken = _tokenService.GenerateJwtToken(acc,date);
                }
                return cus;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                await _slackService.SendMessage(_slackService.CreateMessage(ex, "Verify and generate token rrror!!!"));
                throw new CrudException(HttpStatusCode.InternalServerError, "Verify and generate token rrror!!!", ex.InnerException?.Message);
            }
        }
    }
}
