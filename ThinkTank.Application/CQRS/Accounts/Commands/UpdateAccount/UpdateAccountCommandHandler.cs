

using AutoMapper;
using System.Net;
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.CQRS.Accounts.DomainServices.IService;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.Accounts.Commands.UpdateAccount
{
    public class UpdateAccountCommandHandler : ICommandHandler<UpdateAccountCommand, AccountResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHashPasswordService _hashPasswordHandler;
        private readonly ITokenService _tokenHandler;
        private readonly DateTime date;
        public UpdateAccountCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IHashPasswordService hashPasswordHandler, ITokenService tokenHandler)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _hashPasswordHandler = hashPasswordHandler;
            _tokenHandler = tokenHandler;
            if (TimeZoneInfo.Local.BaseUtcOffset != TimeSpan.FromHours(7))
                date = DateTime.UtcNow.ToLocalTime().AddHours(7);
            else date = DateTime.Now;
        }

        public async Task<AccountResponse> Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
        {
            try
            {

                if (request.UpdateAccountRequest.FullName == "" || request.UpdateAccountRequest.FullName == null || request.UpdateAccountRequest.Email == "" || request.UpdateAccountRequest.Email == null)
                    throw new CrudException(HttpStatusCode.BadRequest, "Information is required", "");


                Account account = _unitOfWork.Repository<Account>()
                     .Find(c => c.Id == request.UserId);

                if (account.Status == false) throw new CrudException(HttpStatusCode.BadRequest, "Your account is block", "");

                if (account == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found account with id{request.UserId}", "");

                if (request.UpdateAccountRequest.DateOfBirth != null && (date.Year - request.UpdateAccountRequest.DateOfBirth.Value.Year) < 5)
                    throw new CrudException(HttpStatusCode.BadRequest, "Date Of Birth is invalid", "");

                var existingEmailAccount = _unitOfWork.Repository<Account>().Find(c => c.Email.Equals(request.UpdateAccountRequest.Email) && c.Id != request.UserId);
                if (existingEmailAccount != null)
                    throw new CrudException(HttpStatusCode.BadRequest, "Email has already been registered", "");

                if (account.GoogleId != null && account.GoogleId != "" && account.Email != request.UpdateAccountRequest.Email)
                    throw new CrudException(HttpStatusCode.BadRequest, $"Login Google cannot update email", "");

                request.UpdateAccountRequest.Email = (account.GoogleId != null && account.GoogleId != "") ? account.Email : request.UpdateAccountRequest.Email;

                _mapper.Map<UpdateAccountRequest, Account>(request.UpdateAccountRequest, account);

                account.Avatar = request.UpdateAccountRequest.Avatar ?? account.Avatar;
                account.Id = request.UserId;
                account.VersionToken = account.VersionToken + 1;
                account.RefreshToken = _tokenHandler.GenerateRefreshToken(account,date);

                if (request.UpdateAccountRequest.OldPassword != null && request.UpdateAccountRequest.NewPassword != null && request.UpdateAccountRequest.OldPassword != "" && request.UpdateAccountRequest.NewPassword != "")
                {
                    if (account.GoogleId != null && account.GoogleId != "")
                        throw new CrudException(HttpStatusCode.BadRequest, "Login Google cannot update password", "");

                    if (!_hashPasswordHandler.VerifyPasswordHash(request.UpdateAccountRequest.OldPassword.Trim(), account.PasswordHash, account.PasswordSalt))
                        throw new CrudException(HttpStatusCode.BadRequest, "Old Password is not match", "");

                    _hashPasswordHandler.CreatPasswordHash(request.UpdateAccountRequest.NewPassword, out byte[] passwordHash, out byte[] passwordSalt);
                    account.PasswordHash = passwordHash;
                    account.PasswordSalt = passwordSalt;
                }
                await _unitOfWork.Repository<Account>().Update(account, request.UserId);
                await _unitOfWork.CommitAsync();
                var rs = _mapper.Map<Account, AccountResponse>(account);
                rs.AccessToken = _tokenHandler.GenerateJwtToken(account,date);
                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Update account error!!!!!", ex.Message);
            }
        }
    }
}
