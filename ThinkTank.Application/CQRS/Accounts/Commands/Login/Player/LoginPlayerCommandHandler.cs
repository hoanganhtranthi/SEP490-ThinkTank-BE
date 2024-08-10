
using AutoMapper;
using System.Net;
using System.Text.RegularExpressions;
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.CQRS.Accounts.DomainServices.IService;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.Services.IService;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.Accounts.Commands.Login.Player
{
    public class LoginPlayerCommandHandler : ICommandHandler<LoginPlayerCommand, AccountResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHashPasswordService _hashPasswordHandler;
        private readonly ITokenService _tokenHandler;
        private readonly DateTime date;
        private readonly IBadgesService _badgesService;
        private readonly ISlackService _slackService;
        public LoginPlayerCommandHandler(IUnitOfWork unitOfWork,
            IMapper mapper,IHashPasswordService hashPasswordHandler, ITokenService tokenHandler, IBadgesService badgesService, ISlackService slackService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _hashPasswordHandler = hashPasswordHandler;
            _tokenHandler = tokenHandler;
            if (TimeZoneInfo.Local.BaseUtcOffset != TimeSpan.FromHours(7))
                date = DateTime.UtcNow.ToLocalTime().AddHours(7);
            else date = DateTime.Now;
            _badgesService = badgesService;
            _slackService = slackService;
        }

        public async Task<AccountResponse> Handle(LoginPlayerCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.LoginRequest.UserName == null || request.LoginRequest.UserName == "" || request.LoginRequest.Password == null || request.LoginRequest.Password == "")
                    throw new CrudException(HttpStatusCode.BadRequest, "Information is required", "");

                Account user = _unitOfWork.Repository<Account>().Find(u => u.UserName.Equals(request.LoginRequest.UserName));
                if (user == null) throw new CrudException(HttpStatusCode.BadRequest, "Account Not Found", "");

                if (user.GoogleId != null && user.GoogleId != "")
                    throw new CrudException(HttpStatusCode.BadRequest, "Login Google cannot login by username and password", "");

                if (!Regex.IsMatch(request.LoginRequest.Password, "^(?=.*\\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[a-zA-Z]).{8,12}$"))
                    throw new CrudException(HttpStatusCode.BadRequest, "Password is invalid", "");

                if (!_hashPasswordHandler.VerifyPasswordHash(request.LoginRequest.Password.Trim(), user.PasswordHash, user.PasswordSalt))
                    throw new CrudException(HttpStatusCode.BadRequest, "Password is incorrect", "");

                if (user.Status == false) throw new CrudException(HttpStatusCode.BadRequest, "Your account is block", "");

                user.VersionToken = user.VersionToken + 1;
                user.RefreshToken = _tokenHandler.GenerateRefreshToken(user, date);
                user.Fcm = (request.LoginRequest.Fcm == null || request.LoginRequest.Fcm == "") ? null : request.LoginRequest.Fcm;

                await _unitOfWork.Repository<Account>().Update(user, user.Id);

                DateTime newDateTime = new DateTime(date.Year, date.Month, date.Day, 21, 0, 0);
                if (date > newDateTime)
                    await _badgesService.GetBadge(user, "Nocturnal");

                await _unitOfWork.CommitAsync();
                var rs = _mapper.Map<Account, AccountResponse>(user);
                rs.AccessToken = _tokenHandler.GenerateJwtToken(user, date);
                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                await _slackService.SendMessage(_slackService.CreateMessage(ex, "Login  Fail"));
                throw new CrudException(HttpStatusCode.InternalServerError, "Login  Fail", ex.InnerException?.Message);
            }
        }
    }
}
