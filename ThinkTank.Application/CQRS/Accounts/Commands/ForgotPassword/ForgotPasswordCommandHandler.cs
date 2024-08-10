

using AutoMapper;
using Microsoft.Extensions.Configuration;
using System.Net.Mail;
using System.Net;
using System.Windows.Input;
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;
using ThinkTank.Application.CQRS.Accounts.DomainServices.IService;
using ThinkTank.Application.Services.IService;

namespace ThinkTank.Application.Accounts.Commands.ForgotPassword
{
    public class ForgotPasswordCommandHandler : ICommandHandler<ForgotPasswordCommand, AccountResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;   
        private readonly IConfiguration _configuration;
        private readonly IHashPasswordService _hashPasswordHandler;
        private readonly ISlackService _slackService;

        public ForgotPasswordCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IHashPasswordService hashPasswordHandler, IConfiguration configuration, ISlackService slackService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _hashPasswordHandler = hashPasswordHandler;
            _configuration = configuration;
            _slackService = slackService;
        }

        public async Task<AccountResponse> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.ResetPasswordRequest.Email == null || request.ResetPasswordRequest.Email == "" || request.ResetPasswordRequest.Username == null || request.ResetPasswordRequest.Username == "")
                    throw new CrudException(HttpStatusCode.BadRequest, "Information is required", "");

                Account account = _unitOfWork.Repository<Account>()
                     .Find(c => c.Email.Equals(request.ResetPasswordRequest.Email) && c.UserName.Equals(request.ResetPasswordRequest.Username));


                if (account == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found account with email {request.ResetPasswordRequest.Email} and username {request.ResetPasswordRequest.Username}", "");
                }

                if (account.Status == false) throw new CrudException(HttpStatusCode.BadRequest, "Your account is block", "");

                if (account.GoogleId != null && account.GoogleId != "")
                    throw new CrudException(HttpStatusCode.BadRequest, "Login Google cannot update password", "");

                var newPass = _hashPasswordHandler.GenerateRandomPassword();

                #region send email new password
                MailMessage message = new MailMessage(_configuration["EmailUserName"], request.ResetPasswordRequest.Email);
                message.Subject = "Your New Password for Account Verification";
                message.Body = $"<p> Hi {account.FullName}, </p> \n <span> <p> We are pleased to inform you that your account has been successfully verified with the email address associated with it. As a result, a new password has been generated for your account to enhance security.</p></span>\n" +
                 $"<p> Your new password is :<strong style= \"font-weight:bold\">{newPass}</strong></p>";
                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(message.Body, null, "text/html");
                message.AlternateViews.Add(htmlView);
                message.IsBodyHtml = true;

                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
                SmtpServer.UseDefaultCredentials = false;
                SmtpServer.Port = 587;
                SmtpServer.Credentials = new System.Net.NetworkCredential(_configuration["EmailUserName"], _configuration["EmailPassword"]);
                SmtpServer.EnableSsl = true;
                SmtpServer.Send(message);
                #endregion

                _hashPasswordHandler.CreatPasswordHash(newPass, out byte[] passwordHash, out byte[] passwordSalt);
                account.PasswordHash = passwordHash;
                account.PasswordSalt = passwordSalt;

                await _unitOfWork.Repository<Account>().Update(account, account.Id);
                await _unitOfWork.CommitAsync();
                return _mapper.Map<Account, AccountResponse>(account);
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                await _slackService.SendMessage(_slackService.CreateMessage(ex, "Update Password account error!!!!!"));
                throw new CrudException(HttpStatusCode.InternalServerError, "Update Password account error!!!!!", ex.Message);
            }
        }
    }
}
