using AutoMapper;
using AutoMapper.QueryableExtensions;
using Firebase.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using OpenAI_API.Completions;
using OpenAI_API.Moderation;
using Repository.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Dynamic.Core.Tokenizer;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ThinkTank.Data.Entities;
using ThinkTank.Data.UnitOfWork;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;
using ThinkTank.Service.Exceptions;
using ThinkTank.Service.Helpers;
using ThinkTank.Service.Services.IService;
using ThinkTank.Service.Utilities;

namespace ThinkTank.Service.Services.ImpService
{
    public class AccountService : IAccountService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;
        private readonly IFirebaseMessagingService _firebaseMessagingService;
        private readonly DateTime date;
        private readonly IFirebaseRealtimeDatabaseService _firebaseRealtimeDatabaseService;
        public AccountService(IUnitOfWork unitOfWork, IMapper mapper, IConfiguration configuration, IFirebaseMessagingService firebaseMessagingService, IFirebaseRealtimeDatabaseService firebaseRealtimeDatabaseService)
        {
            _unitOfWork = unitOfWork;
            if (TimeZoneInfo.Local.BaseUtcOffset != TimeSpan.FromHours(7))
                date = DateTime.UtcNow.ToLocalTime().AddHours(7);
            else date = DateTime.Now;
            _mapper = mapper;
            _config = configuration;
            _firebaseMessagingService = firebaseMessagingService;
            _firebaseRealtimeDatabaseService = firebaseRealtimeDatabaseService;
        }

        public async Task<AccountResponse> CreateAccount(CreateAccountRequest createAccountRequest)
        {
            try
            {
                var customer = _mapper.Map<CreateAccountRequest, Account>(createAccountRequest);
                var s = _unitOfWork.Repository<Account>().Find(s => s.Email == createAccountRequest.Email);
                if (s != null)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Email has already !!!", "");
                }
                var cus = _unitOfWork.Repository<Account>().Find(s => s.UserName == createAccountRequest.UserName);
                if (cus != null)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Username has already !!!", "");
                }
                if(!Regex.IsMatch(createAccountRequest.Password, "^(?=.*\\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[a-zA-Z]).{8,12}$"))
                    throw new CrudException(HttpStatusCode.BadRequest, "Password is invalid", "");
                CreatPasswordHash(createAccountRequest.Password, out byte[] passwordHash, out byte[] passwordSalt);
                customer.PasswordHash = passwordHash;
                customer.PasswordSalt = passwordSalt;
                customer.VersionToken = 1;
                customer.Status = true;
                customer.Avatar = "https://firebasestorage.googleapis.com/v0/b/thinktank-79ead.appspot.com/o/System%2Favatar-trang-4.jpg?alt=media&token=2ab24327-c484-485a-938a-ed30dc3b1688";
                DateTime date = DateTime.Now;
                if (TimeZoneInfo.Local.BaseUtcOffset != TimeSpan.FromHours(7))
                    date = DateTime.UtcNow.ToLocalTime().AddHours(7);
                customer.RegistrationDate = date;
                if (createAccountRequest.Fcm == null || createAccountRequest.Fcm == "")
                    customer.Fcm = null;
                customer.Coin = 1000;
                Guid id = Guid.NewGuid();
                customer.Code = id.ToString().Substring(0, 8).ToUpper();
                CreateBadgeRequest createBadgeRequest = new CreateBadgeRequest();
                createBadgeRequest.AccountId = customer.Id;
                createBadgeRequest.CompletedLevel = 1000;
                createBadgeRequest.ChallengeId = _unitOfWork.Repository<Challenge>().Find(x => x.Name == "The Tycoon").Id;
                var b = _mapper.Map<CreateBadgeRequest, Badge>(createBadgeRequest);
                b.Status = false;
                customer.Badges.Add(b);
                var icons=_unitOfWork.Repository<Icon>().GetAll().AsNoTracking().Where(x=>x.Status==true && x.Price==0).ToList();
                foreach (var icon in icons)
                {
                    IconOfAccount iconOfAccount = new IconOfAccount();
                    iconOfAccount.AccountId=customer.Id;
                    iconOfAccount.IconId = icon.Id;
                    iconOfAccount.IsAvailable = true;
                    customer.IconOfAccounts.Add(iconOfAccount);
                 }
                await _unitOfWork.Repository<Account>().CreateAsync(customer);
                await _unitOfWork.CommitAsync();
                return _mapper.Map<AccountResponse>(customer);
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
        public string GenerateRandomNo()
        {
            int _min = 0000;
            int _max = 9999;
            Random _rdm = new Random();
            return _rdm.Next(_min, _max).ToString();
        }
        public async Task<dynamic> CreateMailMessage(string username)
        {
            var acc = _unitOfWork.Repository<Account>().Find(a => a.UserName.Equals(username));
            if (acc == null)
            {
                throw new CrudException(HttpStatusCode.NotFound, $"Not found account with username {username}", "");
            }
            if (acc.GoogleId != null && acc.GoogleId != "")
                throw new CrudException(HttpStatusCode.BadRequest, "Login Google cannot update password", "");
            bool success = false;
            string token = "";
            var randomToken = GenerateRandomNo();
            string to = acc.Email;
            string from = _config["EmailUserName"];         
            if (acc.Status == false) throw new CrudException(HttpStatusCode.BadRequest, "Your account is block", "");
            MailMessage message = new MailMessage(from, to);
            message.Subject = "Account Verification Code";
            message.Body = $"<p> Hi {acc.FullName}, </p> \n <span> <p> We received a request to access your Account {acc.Email} through your email address. Your Account verification code is:</p></span>\n" +
                $"<div style=\"text-align:center\"<p dir=\"ltr\"><strong style= \"text-align:center;font-size:24px;font-weight:bold\">{randomToken}</strong></p></div>";
            AlternateView htmlView = AlternateView.CreateAlternateViewFromString(message.Body, null, "text/html");
            message.AlternateViews.Add(htmlView);

            message.IsBodyHtml = true;
            SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
            SmtpServer.UseDefaultCredentials = false;

            SmtpServer.Port = 587;
            SmtpServer.Credentials = new System.Net.NetworkCredential(from, _config["EmailPassword"]);
            SmtpServer.EnableSsl = true;

            try
            {
                SmtpServer.Send(message);
                success = true;
                token = randomToken;
            }
            catch (Exception ex)
            {
                success = false;
                token = null;
                throw new Exception(ex.Message);
            }
            return new
            {
                Success = success,
                Token = token,
                Email=acc.Email
            };
        }

        public async Task<AccountResponse> GetAccountById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Id Account Invalid", "");
                }
                var response = await _unitOfWork.Repository<Account>().GetAsync(u => u.Id == id);

                if (response == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found account with id {id.ToString()}", "");
                }

                return _mapper.Map<AccountResponse>(response);
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get Account By ID Error!!!", ex.InnerException?.Message);
            }
        }

        public async Task<PagedResults<AccountResponse>> GetAccounts(AccountRequest request, PagingRequest paging)
        {
            try
            {

                var filter = _mapper.Map<AccountResponse>(request);
                var customers = _unitOfWork.Repository<Account>().GetAll().AsNoTracking()
                                           .ProjectTo<AccountResponse>(_mapper.ConfigurationProvider)
                                           .DynamicFilter(filter)
                                           .ToList();
                foreach (var customer in customers)
                {
                    customer.AmountReport = _unitOfWork.Repository<Report>().GetAll().Where(x => x.AccountId2 == customer.Id).ToList().Count();
                }
                var sort = PageHelper<AccountResponse>.Sorting(paging.SortType, customers, paging.ColName);
                var result = PageHelper<AccountResponse>.Paging(sort, paging.Page, paging.PageSize);
                return result;
            }
            catch (CrudException ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get account list error!!!!!", ex.Message);
            }
        }
        public async Task<AccountResponse> LoginPlayer(LoginRequest request)
        {
            try
            {
                Account user = _unitOfWork.Repository<Account>().Find(u => u.UserName.Equals(request.UserName));
                    if (user == null) throw new CrudException(HttpStatusCode.BadRequest, "Account Not Found", "");
                if (user.GoogleId != null && user.GoogleId != "")
                    throw new CrudException(HttpStatusCode.BadRequest, "Login Google cannot login by username and password", "");
                if (!VerifyPasswordHash(request.Password.Trim(), user.PasswordHash, user.PasswordSalt))
                        throw new CrudException(HttpStatusCode.BadRequest, "Password is incorrect", "");
                    if (user.Status == false) throw new CrudException(HttpStatusCode.BadRequest, "Your account is block", "");
                    user.VersionToken = user.VersionToken+1;
                    var token = GenerateRefreshToken(user);
                    user.RefreshToken = token;
                    if(request.Fcm != null && request.Fcm != "")
                    user.Fcm = request.Fcm;
                    await _unitOfWork.Repository<Account>().Update(user, user.Id);
                    DateTime newDateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 21, 0, 0);
                    if (date > newDateTime)
                    await GetBadge(user, "Nocturnal");
                await _unitOfWork.CommitAsync();
                var rs = _mapper.Map<Account, AccountResponse>(user);
                    rs.AccessToken = GenerateJwtToken(user);
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
        public async Task<AccountResponse> LoginAdmin(LoginRequest request)
        {
            try
            {
                Account user = new Account();
                AccountResponse rs = new AccountResponse();
                if (request.UserName.Equals(_config["UsernameAdmin"]))
                {
                    if (!request.Password.Equals(_config["PasswordAdmin"]))
                        throw new CrudException(HttpStatusCode.BadRequest, "Password is incorrect", "");
                    user.FullName = "Admin";
                   rs = _mapper.Map<Account, AccountResponse>(user);
                    rs.UserName = "Admin";
                    var adminAccount = _firebaseRealtimeDatabaseService.GetAsync<AdminAccountResponse>("AdminAccount").Result;
                    if (adminAccount != null)
                    {
                        adminAccount.VersionTokenAdmin += 1;
                        user.VersionToken=adminAccount.VersionTokenAdmin;
                        rs.AccessToken = GenerateJwtToken(user);
                        var token = GenerateRefreshToken(user);
                        rs.RefreshToken = token;
                        adminAccount.RefreshTokenAdmin = token;
                       await _firebaseRealtimeDatabaseService.SetAsync<AdminAccountResponse>("AdminAccount", adminAccount);
                    }
                    else
                    {
                        adminAccount = new AdminAccountResponse();
                        adminAccount.VersionTokenAdmin = 1;
                        user.VersionToken= adminAccount.VersionTokenAdmin;
                        rs.AccessToken = GenerateJwtToken(user);
                        var token = GenerateRefreshToken(user);
                        rs.RefreshToken = token;
                        adminAccount.RefreshTokenAdmin = token;
                       await _firebaseRealtimeDatabaseService.SetAsync<AdminAccountResponse>("AdminAccount", adminAccount);
                    }
                }
                else throw new CrudException(HttpStatusCode.BadRequest, "Account Not Found", "");
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
        private void CreatPasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }
        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }
        private string GenerateJwtToken(Account? customer)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config["ApiSetting:Secret"]);
            var tokenDescriptor = new SecurityTokenDescriptor();
            if (customer.FullName != "Admin")
            {
                tokenDescriptor.Subject = new ClaimsIdentity(new Claim[]
                 {
                new Claim(ClaimTypes.NameIdentifier, customer.Id.ToString()),
                new Claim(ClaimTypes.Role, "Player"),
                new Claim("version", customer.VersionToken.ToString()),
                new Claim(ClaimTypes.Email , customer.Email),
                 });              
            }
            else
            {
                tokenDescriptor.Subject = new ClaimsIdentity(new Claim[]
                {
                new Claim(ClaimTypes.NameIdentifier, customer.Id.ToString()),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim("version",customer.VersionToken.ToString()),
                });             
            }
            tokenDescriptor.Expires = date.AddMinutes(20);
            tokenDescriptor.SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature);
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        private string GenerateRefreshToken(Account? customer)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config["ApiSetting:Secret"]);
            var tokenDescriptor = new SecurityTokenDescriptor();
            if (customer.FullName != "Admin")
            {
                tokenDescriptor.Subject = new ClaimsIdentity(new Claim[]
                 {
                new Claim(ClaimTypes.NameIdentifier, customer.Id.ToString()),
                new Claim(ClaimTypes.Role, "Player"),
                 new Claim("version", customer.VersionToken.ToString()),
                new Claim(ClaimTypes.Email , customer.Email),
                 });
            }
            else
            {
                tokenDescriptor.Subject = new ClaimsIdentity(new Claim[]
                {
                new Claim(ClaimTypes.NameIdentifier, customer.Id.ToString()),
                new Claim(ClaimTypes.Role, "Admin"),
                 new Claim("version", customer.VersionToken.ToString()),
                });
            }
            tokenDescriptor.Expires = date.AddMonths(6);
            tokenDescriptor.SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature);
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        public async Task<AccountResponse> LoginGoogle(LoginGoogleRequest request)
        {
            try
            {
                Account user = _unitOfWork.Repository<Account>().GetAll().SingleOrDefault(u => u.GoogleId.Equals(request.GoogleId));
                if (user == null)
                {
                    if(_unitOfWork.Repository<Account>().Find(x=>x.Email == request.Email) != null)
                        throw new CrudException(HttpStatusCode.BadRequest, "Email has already been registered", "");
                    user = _mapper.Map<Account>(request);
                    user.VersionToken = 1;
                    user.Status = true;
                    user.RegistrationDate = date;
                    if (request.FCM == null || request.FCM == "")
                        user.Fcm = null;
                    user.Coin = 1000;
                    if (request.Avatar == "" || request.Avatar == null)
                        user.Avatar = "https://firebasestorage.googleapis.com/v0/b/thinktank-79ead.appspot.com/o/System%2Favatar-trang-4.jpg?alt=media&token=2ab24327-c484-485a-938a-ed30dc3b1688";
                    Guid id = Guid.NewGuid();
                    user.Code = id.ToString().Substring(0, 8).ToUpper();
                    user.UserName = $"player_{user.Code}";
                    var token = GenerateRefreshToken(user);
                    user.RefreshToken = token;
                    user.Fcm = request.FCM;
                    CreateBadgeRequest createBadgeRequest = new CreateBadgeRequest();
                    createBadgeRequest.AccountId = user.Id;
                    createBadgeRequest.CompletedLevel = 1000;
                    createBadgeRequest.ChallengeId = _unitOfWork.Repository<Challenge>().Find(x => x.Name == "The Tycoon").Id;
                    var b = _mapper.Map<CreateBadgeRequest, Badge>(createBadgeRequest);
                    b.Status = false;
                    user.Badges.Add(b);
                    await _unitOfWork.Repository<Account>().CreateAsync(user);
                }
                else
                {
                    if (user.Status == false) throw new CrudException(HttpStatusCode.BadRequest, "Your account is block", "");
                    user.VersionToken = user.VersionToken + 1;
                    var token = GenerateRefreshToken(user);
                    user.RefreshToken = token;
                    if (request.FCM == null || request.FCM == "")
                        user.Fcm = null;
                    else user.Fcm = request.FCM;
                    await _unitOfWork.Repository<Account>().Update(user, user.Id);
                    DateTime newDateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 21, 0, 0);
                    if (DateTime.Now > newDateTime)
                       await GetBadge(user, "Nocturnal");
                }
                
                await _unitOfWork.CommitAsync();
                var rs = _mapper.Map<Account, AccountResponse>(user);
                rs.AccessToken = GenerateJwtToken(user);
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
        private async Task GetBadge(Account account, string name)
        {
            var badge = _unitOfWork.Repository<Badge>().GetAll().Include(x => x.Challenge).SingleOrDefault(x => x.AccountId == account.Id && x.Challenge.Name.Equals(name));
            var challage = _unitOfWork.Repository<Challenge>().Find(x => x.Name.Equals(name));
            var noti = _unitOfWork.Repository<Notification>().Find(x => x.Description == $"You have received {challage.Name} badge." && x.AccountId == account.Id);
            if (badge != null)
            {
                if (badge.CompletedLevel < challage.CompletedMilestone)
                 badge.CompletedLevel += 1;
                if (badge.CompletedLevel == challage.CompletedMilestone && noti == null )
                {
                    badge.CompletedDate = date;
                    #region send noti for account
                    List<string> fcmTokens = new List<string>();
                    if (account.Fcm != null)
                        fcmTokens.Add(account.Fcm);
                    var data = new Dictionary<string, string>()
                    {
                        ["click_action"] = "FLUTTER_NOTIFICATION_CLICK",
                        ["Action"] = "home",
                        ["Argument"] = JsonConvert.SerializeObject(new JsonSerializerSettings
                        {
                            ContractResolver = new DefaultContractResolver
                            {
                                NamingStrategy = new SnakeCaseNamingStrategy()
                            }
                        }),
                    };
                    if (fcmTokens.Any())
                        _firebaseMessagingService.SendToDevices(fcmTokens,
                                                               new FirebaseAdmin.Messaging.Notification() { Title = "ThinkTank", Body = $"You have received {challage.Name} badge.", ImageUrl = challage.Avatar }, data);
                    #endregion
                    Notification notification = new Notification
                    {
                        AccountId = account.Id,
                        Avatar = challage.Avatar,
                        DateNotification = DateTime.Now,
                        Description = $"You have received {challage.Name} badge.",
                        Status=false, 
                        Title = "ThinkTank"
                    };
                    await _unitOfWork.Repository<Notification>().CreateAsync(notification);
                }
                await _unitOfWork.Repository<Badge>().Update(badge, badge.Id);
            }
            else
            {
                CreateBadgeRequest createBadgeRequest = new CreateBadgeRequest();
                createBadgeRequest.AccountId = account.Id;
                createBadgeRequest.CompletedLevel = 1;
                createBadgeRequest.ChallengeId = challage.Id;
                var b = _mapper.Map<CreateBadgeRequest, Badge>(createBadgeRequest);
                b.Status = false;
                await _unitOfWork.Repository<Badge>().CreateAsync(b);
            }
        }

        public async Task<AccountResponse> RevokeRefreshToken(int userId)
        {
            try
            {
                AccountResponse rs = new AccountResponse();
                if (userId==0)
                {
                    var expiryTime = DateTime.MaxValue;
                    var adminAccountResponse =  _firebaseRealtimeDatabaseService.GetAsync<AdminAccountResponse>("AdminAccount").Result;
                    if (adminAccountResponse != null)
                    {
                        adminAccountResponse.VersionTokenAdmin += 1;
                        adminAccountResponse.RefreshTokenAdmin = null;
                       await _firebaseRealtimeDatabaseService.SetAsync<AdminAccountResponse>("AdminAccount", adminAccountResponse);
                    }
                    rs.UserName = "Admin";
                }
                else
                {
                    Account customer = _unitOfWork.Repository<Account>().GetAsync(a => a.Id==userId).Result;
                    if (customer == null)
                    {
                        throw new CrudException(HttpStatusCode.NotFound, $"Not found account with id {userId}", "");
                    }
                    if (customer.Status == false) throw new CrudException(HttpStatusCode.BadRequest, "Your account is block", "");
                    customer.RefreshToken = null;
                    customer.Fcm = null;
                    customer.VersionToken = customer.VersionToken + 1;
                    await _unitOfWork.Repository<Account>().Update(customer, customer.Id);
                    await _unitOfWork.CommitAsync();
                    rs = _mapper.Map<Account, AccountResponse>(customer);
                }
                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Revoke RefreshToken Fail", ex.InnerException?.Message);
            }
        }

        public async Task<AccountResponse> UpdateAccount(int accountId, UpdateAccountRequest request)
        {
            try
            {
                Account account = _unitOfWork.Repository<Account>()
                     .Find(c => c.Id == accountId);
                if (account.Status == false) throw new CrudException(HttpStatusCode.BadRequest, "Your account is block", "");
                if (account == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found account with id{accountId.ToString()}", "");
                if (date.Year - request.DateOfBirth.Value.Year < 5 )
                    throw new CrudException(HttpStatusCode.BadRequest, "Date Of Birth is invalid", "");

                var existingEmailAccount = _unitOfWork.Repository<Account>().Find(c => c.Email.Equals(request.Email) && c.Id != accountId);
                if (existingEmailAccount != null)
                    throw new CrudException(HttpStatusCode.BadRequest, "Email has already been registered", "");                                 
                _mapper.Map<UpdateAccountRequest, Account>(request, account);

                account.Avatar = request.Avatar ?? account.Avatar;
                account.Id = accountId;
                account.VersionToken = account.VersionToken + 1;
                if (request.OldPassword != null && request.NewPassword != null && request.OldPassword != "" && request.NewPassword != "")
                {
                    if (account.GoogleId != null && account.GoogleId != "")
                        throw new CrudException(HttpStatusCode.BadRequest, "Login Google cannot update password", "");
                        if (!Regex.IsMatch(request.NewPassword, "^(?=.*\\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[a-zA-Z]).{8,12}$"))
                        throw new CrudException(HttpStatusCode.BadRequest, "New Password is invalid", "");
                    if (!VerifyPasswordHash(request.OldPassword.Trim(), account.PasswordHash, account.PasswordSalt))
                        throw new CrudException(HttpStatusCode.BadRequest, "Old Password is not match", "");
                    CreatPasswordHash(request.NewPassword, out byte[] passwordHash, out byte[] passwordSalt);
                    account.PasswordHash = passwordHash;
                    account.PasswordSalt = passwordSalt;
                }
                await _unitOfWork.Repository<Account>().Update(account, accountId);
                await _unitOfWork.CommitAsync();
                var rs= _mapper.Map<Account, AccountResponse>(account);
                rs.AccessToken = GenerateJwtToken(account);
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
      private  string GeneratePassword()
        {
            string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random();
            StringBuilder passwordBuilder = new StringBuilder();
            passwordBuilder.Append(validChars[random.Next(10, validChars.Length)]);
            passwordBuilder.Append(validChars[random.Next(0, 26)]);
            passwordBuilder.Append(char.ToUpper(validChars[random.Next(0, 26)]));
            while (passwordBuilder.Length < 8)
            {
                passwordBuilder.Append(validChars[random.Next(validChars.Length)]);
            }

            return passwordBuilder.ToString();
        }
    

    public async Task<AccountResponse> UpdatePass(ResetPasswordRequest request)
        {
            try
            {
                Account customer = _unitOfWork.Repository<Account>()
                     .Find(c => c.Email.Equals(request.Email) && c.UserName.Equals(request.Username));
                if (customer == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found account with email {request.Email} and username {request.Username}", "");
                }
                if (customer.Status == false) throw new CrudException(HttpStatusCode.BadRequest, "Your account is block", "");
                if (customer.GoogleId != null && customer.GoogleId != "")
                    throw new CrudException(HttpStatusCode.BadRequest, "Login Google cannot update password", "");
                string to = request.Email;
                string from = _config["EmailUserName"];
                var newPass = GeneratePassword();
                MailMessage message = new MailMessage(from, to);
                message.Subject = "Your New Password for Account Verification";
                message.Body = $"<p> Hi {customer.FullName}, </p> \n <span> <p> We are pleased to inform you that your account has been successfully verified with the email address associated with it. As a result, a new password has been generated for your account to enhance security.</p></span>\n" +
                 $"<p> Your new password is :<strong style= \"font-weight:bold\">{newPass}</strong></p>";
                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(message.Body, null, "text/html");
                message.AlternateViews.Add(htmlView);

                message.IsBodyHtml = true;
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
                SmtpServer.UseDefaultCredentials = false;
                SmtpServer.Port = 587;
                SmtpServer.Credentials = new System.Net.NetworkCredential(from, _config["EmailPassword"]);
                SmtpServer.EnableSsl = true;
                SmtpServer.Send(message);                  
                CreatPasswordHash(newPass, out byte[] passwordHash, out byte[] passwordSalt);
                customer.PasswordHash = passwordHash;
                customer.PasswordSalt = passwordSalt;
                await _unitOfWork.Repository<Account>().Update(customer, customer.Id);
                await _unitOfWork.CommitAsync();
                return _mapper.Map<Account, AccountResponse>(customer);
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Update Password account error!!!!!", ex.Message);
            }
        }

        public async Task<AccountResponse> VerifyAndGenerateToken(TokenRequest request)
        {
            try
            {
                AccountResponse cus = new AccountResponse();
                Account acc = new Account();
                var jwtTokenHandler = new JwtSecurityTokenHandler();
                JwtSecurityToken tokenPrincipal;
                try
                {
                    tokenPrincipal = jwtTokenHandler.ReadJwtToken(request.AccessToken);
                }
                catch (ArgumentException)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Invalid Access Token", "");
                }
                var utcExpiredDate = long.Parse(tokenPrincipal.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);
                var expiredDate = DateTimeOffset.FromUnixTimeSeconds(utcExpiredDate).DateTime;             
                if (tokenPrincipal.Claims.FirstOrDefault(x => x.Type == "role").Value.Equals("Admin"))
                {
                    var adminAccountResponse = _firebaseRealtimeDatabaseService.GetAsync<AdminAccountResponse>("AdminAccount").Result;
                    if (adminAccountResponse != null)
                    {
                        if (!request.RefreshToken.Equals(adminAccountResponse.RefreshTokenAdmin))
                            throw new CrudException(HttpStatusCode.BadRequest, "Invalid Refresh Token", "");
                    }
                    if (expiredDate > DateTime.UtcNow) throw new CrudException(HttpStatusCode.BadRequest, "Access Token is not expried", "");
                    acc.FullName = "Admin";
                    acc.VersionToken = adminAccountResponse.VersionTokenAdmin + 1;
                    var token = GenerateJwtToken(acc);
                    cus = _mapper.Map<Account, AccountResponse>(acc);
                    cus.AccessToken = token;
                    cus.RefreshToken = GenerateRefreshToken(acc);
                    adminAccountResponse.RefreshTokenAdmin = cus.RefreshToken;
                    adminAccountResponse.VersionTokenAdmin = (int)acc.VersionToken;
                    await _firebaseRealtimeDatabaseService.SetAsync<AdminAccountResponse>("AdminAccount", adminAccountResponse);
                }
                else
                {
                    acc = _unitOfWork.Repository<Account>().Find(a => a.RefreshToken != null && a.RefreshToken.Equals(request.RefreshToken));
                    if (acc == null) throw new CrudException(HttpStatusCode.BadRequest, "Invalid Refresh Token", "");
                    if (expiredDate > DateTime.UtcNow) throw new CrudException(HttpStatusCode.BadRequest, "Access Token is not expried", "");
                    if (acc.Status == false) throw new CrudException(HttpStatusCode.BadRequest, "Your account is block", "");
                    acc.VersionToken = acc.VersionToken + 1;
                    var token = GenerateRefreshToken(acc);
                    acc.RefreshToken = token;
                    await _unitOfWork.Repository<Account>().Update(acc, acc.Id);
                    await _unitOfWork.CommitAsync();
                    cus = _mapper.Map<Account, AccountResponse>(acc);
                    cus.AccessToken = GenerateJwtToken(acc);
                }
                return cus;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Progress Error!!!", ex.InnerException?.Message);
            }
        }
        public async Task<AccountResponse> GetToBanAccount(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Id Account Invalid", "");
                }
                Account account = _unitOfWork.Repository<Account>()
                    .Find(c => c.Id == id);
                if (account == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found account with id{id.ToString()}", "");
                }
                account.Status = !account.Status;
                account.VersionToken += 1;
                account.RefreshToken = null;
                await _unitOfWork.Repository<Account>().Update(account, id);
                await _unitOfWork.CommitAsync();
                return _mapper.Map<Account, AccountResponse>(account);
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Ban account error!!!!!", ex.Message);
            }
        }
        public async Task<AccountResponse> GetToUpdateStatus(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Id Account Invalid", "");
                }
                Account account = _unitOfWork.Repository<Account>()
                      .Find(c => c.Id == id);

                if (account == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found account with id{id.ToString()}", "");
                }
                if (account.Status == false) throw new CrudException(HttpStatusCode.BadRequest, "Your account is block", "");
                account.IsOnline = !account.IsOnline;
                await _unitOfWork.Repository<Account>().Update(account, id);
                await _unitOfWork.CommitAsync();
                return _mapper.Map<Account, AccountResponse>(account);
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Update status account error!!!!!", ex.Message);
            }
        }
        public async Task<List<GameLevelOfAccountResponse>> GetGameLevelByAccountId(int accountId)
        {
            try
            {
                var account = _unitOfWork.Repository<Account>().Find(x => x.Id == accountId);
                if (account == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"Account Id {accountId} not found ", "");
                var achievements = _unitOfWork.Repository<Achievement>().GetAll().Include(x => x.Account).Include(x => x.Game)
                    .Where(x => x.AccountId == accountId).ToList();
                var result = new List<GameLevelOfAccountResponse>();
                foreach (var achievement in achievements)
                {
                    GameLevelOfAccountResponse gameLevelOfAccountResponse = new GameLevelOfAccountResponse();
                    var game = result.SingleOrDefault(a => a.GameId == achievement.GameId);
                    if (game == null)
                    {
                        gameLevelOfAccountResponse.GameId = (int)achievement.GameId;
                        gameLevelOfAccountResponse.GameName = achievement.Game.Name;
                        gameLevelOfAccountResponse.Level = achievements.LastOrDefault(a => a.GameId == achievement.GameId).Level;
                        result.Add(gameLevelOfAccountResponse);
                    }
                }
                return result;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get game level of account error!!!!!", ex.Message);
            }
        }
    }
}
