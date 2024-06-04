using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Application.Accounts.Commands.Login;
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.CQRS.Accounts.DomainServices.IService;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.Services.IService;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.Accounts.Commands.LoginGoogle
{
    public class LoginGoogleCommandHandler : ICommandHandler<LoginGoogleCommand, AccountResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenHandler;
        private readonly DateTime date;
        private readonly IBadgesService _badgesService;
        public LoginGoogleCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ITokenService tokenHandler, IBadgesService badgesService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _tokenHandler = tokenHandler;
            if (TimeZoneInfo.Local.BaseUtcOffset != TimeSpan.FromHours(7))
                date = DateTime.UtcNow.ToLocalTime().AddHours(7);
            else date = DateTime.Now;
            _badgesService = badgesService;
        }

        public async Task<AccountResponse> Handle(LoginGoogleCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.LoginGoogleRequest.Email == null || request.LoginGoogleRequest.Email == "" || request.LoginGoogleRequest.FullName == null || request.LoginGoogleRequest.FullName == "" ||
                    request.LoginGoogleRequest.GoogleId == null || request.LoginGoogleRequest.GoogleId == "")
                    throw new CrudException(HttpStatusCode.BadRequest, "Information is required", "");

                Account user = _unitOfWork.Repository<Account>().GetAll().SingleOrDefault(u => u.GoogleId.Equals(request.LoginGoogleRequest.GoogleId));

                if (user == null)
                {
                    if (_unitOfWork.Repository<Account>().Find(x => x.Email == request.LoginGoogleRequest.Email) != null)
                        throw new CrudException(HttpStatusCode.BadRequest, "Email has already been registered", "");

                    user = _mapper.Map<Account>(request.LoginGoogleRequest);
                    user.VersionToken = 1;
                    user.Status = true;
                    user.RegistrationDate = date;
                    user.Fcm = request.LoginGoogleRequest.FCM == null || request.LoginGoogleRequest.FCM == "" ? null : request.LoginGoogleRequest.FCM;
                    user.Coin = 1000;
                    user.Avatar = request.LoginGoogleRequest.Avatar == "" || request.LoginGoogleRequest.Avatar == null ?
                     "https://firebasestorage.googleapis.com/v0/b/thinktank-79ead.appspot.com/o/System%2Favatar-trang-4.jpg?alt=media&token=2ab24327-c484-485a-938a-ed30dc3b1688" : request.LoginGoogleRequest.Avatar;

                    Guid id = Guid.NewGuid();
                    user.Code = id.ToString().Substring(0, 8).ToUpper();
                    user.UserName = $"player_{user.Code}";
                    user.RefreshToken =_tokenHandler.GenerateRefreshToken(user, date);

                    Badge badge = new Badge();
                    badge.AccountId = user.Id;
                    badge.CompletedLevel = 1000;
                    badge.ChallengeId = _unitOfWork.Repository<Challenge>().Find(x => x.Name == "The Tycoon").Id;
                    badge.Status = false;
                    user.Badges.Add(badge);

                    var icons = _unitOfWork.Repository<Icon>().GetAll().AsNoTracking().Where(x => x.Status == true && x.Price == 0).ToList();
                    foreach (var icon in icons)
                    {
                        IconOfAccount iconOfAccount = new IconOfAccount();
                        iconOfAccount.AccountId = user.Id;
                        iconOfAccount.IconId = icon.Id;
                        iconOfAccount.IsAvailable = true;
                        user.IconOfAccounts.Add(iconOfAccount);
                    }
                    await _unitOfWork.Repository<Account>().CreateAsync(user);
                }
                else
                {
                    if (user.Status == false) throw new CrudException(HttpStatusCode.BadRequest, "Your account is block", "");

                    user.VersionToken = user.VersionToken + 1;
                    user.RefreshToken = _tokenHandler.GenerateRefreshToken(user, date);
                    user.Fcm = request.LoginGoogleRequest.FCM == null || request.LoginGoogleRequest.FCM == "" ? null : request.LoginGoogleRequest.FCM;
                    await _unitOfWork.Repository<Account>().Update(user, user.Id);

                    DateTime newDateTime = new DateTime(date.Year, date.Month, date.Day, 21, 0, 0);
                    if (date > newDateTime)
                        await _badgesService.GetBadge(user, "Nocturnal");
                }

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
                throw new CrudException(HttpStatusCode.InternalServerError, "Login Google Fail!!!", ex.InnerException?.Message);
            }
        }
    }
}
