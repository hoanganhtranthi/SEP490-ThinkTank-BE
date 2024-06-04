
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.CQRS.Accounts.DomainServices.IService;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.Accounts.Commands.CreateAccount
{
    public class CreateAccountCommandHandler : ICommandHandler<CreateAccountCommand, AccountResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;
        private readonly DateTime date;
        private readonly IHashPasswordService _hashPasswordHandler;
        public CreateAccountCommandHandler(IUnitOfWork unitOfWork, IMapper mapper,IConfiguration configuration,IHashPasswordService hashPasswordHandler)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _config = configuration;
            if (TimeZoneInfo.Local.BaseUtcOffset != TimeSpan.FromHours(7))
                date = DateTime.UtcNow.ToLocalTime().AddHours(7);
            else date = DateTime.Now;
            _hashPasswordHandler = hashPasswordHandler;
        }
        public async Task<AccountResponse> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.CreateAccountRequest.FullName == null || request.CreateAccountRequest.FullName == "" || request.CreateAccountRequest.Email == null || request.CreateAccountRequest.Email == ""
                || request.CreateAccountRequest.UserName == null || request.CreateAccountRequest.UserName == "" || request.CreateAccountRequest.Password == null || request.CreateAccountRequest.Password == "")
                    throw new CrudException(HttpStatusCode.BadRequest, "Information is required", "");


                var account = _mapper.Map<CreateAccountRequest, Account>(request.CreateAccountRequest);
                var s = _unitOfWork.Repository<Account>().Find(s => s.Email == request.CreateAccountRequest.Email);
                if (s != null)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Email has already !!!", "");
                }

                var cus = _unitOfWork.Repository<Account>().Find(s => s.UserName == request.CreateAccountRequest.UserName || s.UserName.Equals(_config["UsernameAdmin"]));
                if (cus != null)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Username has already !!!", "");
                }

                _hashPasswordHandler.CreatPasswordHash(request.CreateAccountRequest.Password, out byte[] passwordHash, out byte[] passwordSalt);
                account.PasswordHash = passwordHash;
                account.PasswordSalt = passwordSalt;

                account.VersionToken = 1;
                account.Status = true;
                account.Avatar = "https://firebasestorage.googleapis.com/v0/b/thinktank-79ead.appspot.com/o/System%2Favatar-trang-4.jpg?alt=media&token=2ab24327-c484-485a-938a-ed30dc3b1688";
                account.RegistrationDate = date;
                account.Fcm = (request.CreateAccountRequest.Fcm == null || request.CreateAccountRequest.Fcm == "") ? null : request.CreateAccountRequest.Fcm;
                account.Coin = 1000;

                Guid id = Guid.NewGuid();
                account.Code = id.ToString().Substring(0, 8).ToUpper();

                //Tạo badge Tycoon cho account mới
                Badge badge = new Badge();
                badge.AccountId = account.Id;
                badge.CompletedLevel = 1000;
                badge.ChallengeId = _unitOfWork.Repository<Challenge>().Find(x => x.Name == "The Tycoon").Id;
                badge.Status = false;
                account.Badges.Add(badge);

                // Tạo icon free cho account mới
                var icons = _unitOfWork.Repository<Icon>().GetAll().AsNoTracking().Where(x => x.Status == true && x.Price == 0).ToList();
                foreach (var icon in icons)
                {
                    IconOfAccount iconOfAccount = new IconOfAccount();
                    iconOfAccount.AccountId = account.Id;
                    iconOfAccount.IconId = icon.Id;
                    iconOfAccount.IsAvailable = true;
                    account.IconOfAccounts.Add(iconOfAccount);
                }

                await _unitOfWork.Repository<Account>().CreateAsync(account);
                await _unitOfWork.CommitAsync();
                return _mapper.Map<AccountResponse>(account);
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Create Account Error!!!", ex?.Message);
            }
        }
    }
}
