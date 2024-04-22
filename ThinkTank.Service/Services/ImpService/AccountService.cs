using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
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
                if (createAccountRequest.FullName == null || createAccountRequest.FullName == "" || createAccountRequest.Email == null || createAccountRequest.Email == ""
                    || createAccountRequest.UserName == null || createAccountRequest.UserName == "" || createAccountRequest.Password == null || createAccountRequest.Password == "")
                    throw new CrudException(HttpStatusCode.BadRequest, "Information is required", "");


                var customer = _mapper.Map<CreateAccountRequest, Account>(createAccountRequest);
                var s = _unitOfWork.Repository<Account>().Find(s => s.Email == createAccountRequest.Email);
                if (s != null)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Email has already !!!", "");
                }

                var cus = _unitOfWork.Repository<Account>().Find(s => s.UserName == createAccountRequest.UserName || s.UserName.Equals(_config["UsernameAdmin"]));
                if (cus != null)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Username has already !!!", "");
                }

                CreatPasswordHash(createAccountRequest.Password, out byte[] passwordHash, out byte[] passwordSalt);
                customer.PasswordHash = passwordHash;
                customer.PasswordSalt = passwordSalt;

                customer.VersionToken = 1;
                customer.Status = true;
                customer.Avatar = "https://firebasestorage.googleapis.com/v0/b/thinktank-79ead.appspot.com/o/System%2Favatar-trang-4.jpg?alt=media&token=2ab24327-c484-485a-938a-ed30dc3b1688";
                customer.RegistrationDate = date;

                customer.Fcm = (createAccountRequest.Fcm == null || createAccountRequest.Fcm == "") ? null : createAccountRequest.Fcm;

                customer.Coin = 1000;

                Guid id = Guid.NewGuid();
                customer.Code = id.ToString().Substring(0, 8).ToUpper();

                //Tạo badge Tycoon cho account mới
                Badge badge = new Badge();
                badge.AccountId = customer.Id;
                badge.CompletedLevel = 1000;
                badge.ChallengeId = _unitOfWork.Repository<Challenge>().Find(x => x.Name == "The Tycoon").Id;
                badge.Status = false;
                customer.Badges.Add(badge);

                // Tạo icon free cho account mới
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

        private async Task<List<Badge>> GetListBadgesCompleted(Account account)
        {
            var result = new List<Badge>();
            var badges = _unitOfWork.Repository<Badge>().GetAll().Include(x => x.Challenge).Where(x => x.AccountId == account.Id).ToList();
            if (badges.Any())
            {
                foreach (var badge in badges)
                {
                    if (badge.CompletedLevel == badge.Challenge.CompletedMilestone)
                        result.Add(badge);
                }
            }
            return result;
        }
        public async Task<AccountResponse> LoginPlayer(LoginRequest request)
        {
            try
            {
                if (request.UserName == null || request.UserName == "" || request.Password == null || request.Password == "")
                    throw new CrudException(HttpStatusCode.BadRequest, "Information is required", "");

                Account user = _unitOfWork.Repository<Account>().Find(u => u.UserName.Equals(request.UserName));
                    if (user == null) throw new CrudException(HttpStatusCode.BadRequest, "Account Not Found", "");
                
                if (user.GoogleId != null && user.GoogleId != "")
                    throw new CrudException(HttpStatusCode.BadRequest, "Login Google cannot login by username and password", "");

                if (!Regex.IsMatch(request.Password, "^(?=.*\\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[a-zA-Z]).{8,12}$"))
                    throw new CrudException(HttpStatusCode.BadRequest, "Password is invalid", "");

                if (!VerifyPasswordHash(request.Password.Trim(), user.PasswordHash, user.PasswordSalt))
                        throw new CrudException(HttpStatusCode.BadRequest, "Password is incorrect", "");


                if (user.Status == false) throw new CrudException(HttpStatusCode.BadRequest, "Your account is block", "");
                    

                    user.VersionToken = user.VersionToken+1;
                    user.RefreshToken = GenerateRefreshToken(user);
                    user.Fcm = (request.Fcm == null || request.Fcm == "") ? null : request.Fcm;

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
                if (request.UserName == null || request.UserName == "" || request.Password == null || request.Password == "")
                    throw new CrudException(HttpStatusCode.BadRequest, "Information is required", "");

                Account user = new Account();
                AccountResponse rs = new AccountResponse();

                if (request.UserName.Equals(_config["UsernameAdmin"]))
                {
                    if (!request.Password.Equals(_config["PasswordAdmin"]))
                        throw new CrudException(HttpStatusCode.BadRequest, "Password is incorrect", "");

                    rs = _mapper.Map<Account, AccountResponse>(user);
                    rs.UserName = "Admin";
                    user.FullName = "Admin";

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
                        user.VersionToken= 1;

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
                if (request.Email == null || request.Email == "" || request.FullName == null || request.FullName == "" ||
                    request.GoogleId == null || request.GoogleId=="")
                    throw new CrudException(HttpStatusCode.BadRequest, "Information is required", "");

                Account user = _unitOfWork.Repository<Account>().GetAll().SingleOrDefault(u => u.GoogleId.Equals(request.GoogleId));

                if (user == null)
                {
                    if(_unitOfWork.Repository<Account>().Find(x=>x.Email == request.Email) != null)
                        throw new CrudException(HttpStatusCode.BadRequest, "Email has already been registered", "");

                    user = _mapper.Map<Account>(request);
                    user.VersionToken = 1;
                    user.Status = true;
                    user.RegistrationDate = date;
                    user.Fcm = (request.FCM == null || request.FCM == "") ? null : request.FCM;
                    user.Coin = 1000;
                    user.Avatar = (request.Avatar == "" || request.Avatar == null) ?
                     "https://firebasestorage.googleapis.com/v0/b/thinktank-79ead.appspot.com/o/System%2Favatar-trang-4.jpg?alt=media&token=2ab24327-c484-485a-938a-ed30dc3b1688" : request.Avatar;
                    
                    Guid id = Guid.NewGuid();
                    user.Code = id.ToString().Substring(0, 8).ToUpper();
                    user.UserName = $"player_{user.Code}";
                    user.RefreshToken = GenerateRefreshToken(user);
                   
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
                    user.RefreshToken = GenerateRefreshToken(user);
                    user.Fcm = (request.FCM == null || request.FCM == "") ? null : request.FCM;
                    await _unitOfWork.Repository<Account>().Update(user, user.Id);

                    DateTime newDateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 21, 0, 0);
                    if (date > newDateTime)
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
            var result = await GetListBadgesCompleted(account);

            if (result.SingleOrDefault(x => x.Challenge.Name.Equals(name)) == null)
            {
                var badge = _unitOfWork.Repository<Badge>().GetAll().Include(x => x.Challenge).SingleOrDefault(x => x.AccountId == account.Id && x.Challenge.Name.Equals(name));
                var challage = _unitOfWork.Repository<Challenge>().Find(x => x.Name.Equals(name));
                if (badge != null)
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
                            Status = false,
                            Title = "ThinkTank"
                        };
                        await _unitOfWork.Repository<Notification>().CreateAsync(notification);
                }
                badge = new Badge();
                badge.AccountId = account.Id;
                badge.CompletedLevel = 1;
                badge.ChallengeId = challage.Id;
                badge.Status = false;
                await _unitOfWork.Repository<Badge>().CreateAsync(badge);

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
                if (request.FullName == "" || request.FullName == null || request.Email == "" || request.Email==null)
                    throw new CrudException(HttpStatusCode.BadRequest, "Information is required", "");


                Account account = _unitOfWork.Repository<Account>()
                     .Find(c => c.Id == accountId);

                if (account.Status == false) throw new CrudException(HttpStatusCode.BadRequest, "Your account is block", "");
                if (account == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found account with id{accountId}", "");

                if (request.DateOfBirth !=null && (date.Year - request.DateOfBirth.Value.Year) < 5 )
                    throw new CrudException(HttpStatusCode.BadRequest, "Date Of Birth is invalid", "");

                var existingEmailAccount = _unitOfWork.Repository<Account>().Find(c => c.Email.Equals(request.Email) && c.Id != accountId);
                if (existingEmailAccount != null)
                    throw new CrudException(HttpStatusCode.BadRequest, "Email has already been registered", "");

                request.Email = (account.GoogleId != null && account.GoogleId != "") ? account.Email : request.Email;

                _mapper.Map<UpdateAccountRequest, Account>(request, account);

                account.Avatar = request.Avatar ?? account.Avatar;
                account.Id = accountId;
                account.VersionToken = account.VersionToken + 1;
                account.RefreshToken = GenerateRefreshToken(account);
                if (request.OldPassword != null && request.NewPassword != null && request.OldPassword != "" && request.NewPassword != "")
                {
                    if (account.GoogleId != null && account.GoogleId != "")
                        throw new CrudException(HttpStatusCode.BadRequest, "Login Google cannot update password", "");
                                     
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
        private string GeneratePassword()
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random();
            StringBuilder passwordBuilder = new StringBuilder();

            // Thêm một ký tự chữ thường, một ký tự chữ in hoa và một chữ số vào mật khẩu
            passwordBuilder.Append(validChars[random.Next(validChars.Length)]);
            passwordBuilder.Append(validChars[random.Next(26, 52)]); // Chữ in hoa
            passwordBuilder.Append(validChars[random.Next(52, 62)]); // Số

            int requiredLength = random.Next(8, 13); // Random độ dài từ 8 đến 12 ký tự
            while (passwordBuilder.Length < requiredLength)
            {
                passwordBuilder.Append(validChars[random.Next(validChars.Length)]);
            }

            return passwordBuilder.ToString();
        }




        public async Task<AccountResponse> UpdatePass(ResetPasswordRequest request)
        {
            try
            {
                if (request.Email == null || request.Email == "" || request.Username == null || request.Username == "")
                    throw new CrudException(HttpStatusCode.BadRequest, "Information is required", "");

                Account customer = _unitOfWork.Repository<Account>()
                     .Find(c => c.Email.Equals(request.Email) && c.UserName.Equals(request.Username));
                if (customer == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found account with email {request.Email} and username {request.Username}", "");
                }
                if (customer.Status == false) throw new CrudException(HttpStatusCode.BadRequest, "Your account is block", "");
                if (customer.GoogleId != null && customer.GoogleId != "")
                    throw new CrudException(HttpStatusCode.BadRequest, "Login Google cannot update password", "");
                var newPass = GeneratePassword();

                MailMessage message = new MailMessage(_config["EmailUserName"], request.Email);
                message.Subject = "Your New Password for Account Verification";
                message.Body = $"<p> Hi {customer.FullName}, </p> \n <span> <p> We are pleased to inform you that your account has been successfully verified with the email address associated with it. As a result, a new password has been generated for your account to enhance security.</p></span>\n" +
                 $"<p> Your new password is :<strong style= \"font-weight:bold\">{newPass}</strong></p>";
                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(message.Body, null, "text/html");
                message.AlternateViews.Add(htmlView);
                message.IsBodyHtml = true;
               
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
                SmtpServer.UseDefaultCredentials = false;
                SmtpServer.Port = 587;
                SmtpServer.Credentials = new System.Net.NetworkCredential(_config["EmailUserName"], _config["EmailPassword"]);
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
                if (request.AccessToken == null || request.AccessToken == "" || request.RefreshToken == null || request.RefreshToken == "")
                    throw new CrudException(HttpStatusCode.BadRequest, "Information is required", "");
                
                AccountResponse cus = new AccountResponse();
                Account acc = new Account();
                var jwtTokenHandler = new JwtSecurityTokenHandler();

                JwtSecurityToken tokenPrincipal;
                

                //Check xem access token đúng là một JWTToken không
                try
                {
                    tokenPrincipal = jwtTokenHandler.ReadJwtToken(request.AccessToken);
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
                        if (!request.RefreshToken.Equals(adminAccountResponse.RefreshTokenAdmin))
                            throw new CrudException(HttpStatusCode.BadRequest, "Invalid Refresh Token", "");
                    }

                    //Check xem acess token đã hết hạn hay chưa
                    if (expiredDate > DateTime.UtcNow) throw new CrudException(HttpStatusCode.BadRequest, "Access Token is not expried", "");
                    
                    acc.FullName = "Admin";
                    acc.VersionToken = adminAccountResponse.VersionTokenAdmin + 1;
                    cus = _mapper.Map<Account, AccountResponse>(acc);
                    cus.AccessToken = GenerateJwtToken(acc);
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
                    acc.RefreshToken = GenerateRefreshToken(acc);
                    
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
                throw new CrudException(HttpStatusCode.InternalServerError, "Verify And Generate Token Error!!!", ex.InnerException?.Message);
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
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found account with id{id}", "");
                }

                //Check xem account đó có đang online hay không
                if (CheckIsLogin(id).Result==true)
                    throw new CrudException(HttpStatusCode.BadRequest, $"Account Id {id} is online so it cannot be banned ", "");
                
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
        private async Task<bool> CheckIsLogin(int id)
        {
            try
            {
                Account user = new Account();
                var result = false;
                user = _unitOfWork.Repository<Account>().Find(u => u.Id==id);
                    if (user == null) throw new CrudException(HttpStatusCode.BadRequest, "Account Not Found", "");
               
                //Get trạng thái của account hiện tại là online hay offline
                var isOnline= await _firebaseRealtimeDatabaseService.GetAsyncOfFlutterRealtimeDatabase<bool>($"/islogin/{id}");
                if(isOnline != null)
                {
                    //Set trạng thái lại là offline
                    await _firebaseRealtimeDatabaseService.SetAsyncOfFlutterRealtimeDatabase<bool>($"/islogin/{id}", false);
                    Thread.Sleep(2000);
                    //Get ra trạng thái của account hiện tại  sau khi deplay 2s
                    result= await _firebaseRealtimeDatabaseService.GetAsyncOfFlutterRealtimeDatabase<bool>($"/islogin/{id}");
                }
                return result;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Check login error!!!!!", ex.Message);
            }
        }
        public async Task<AccountResponse> GetIdToLogin(LoginRequest request, string? googleId)
        {
            try
            {
                             
                Account user = new Account();
                if (request.UserName != null && request.Password != null && request.UserName != "" && request.Password != "")
                {
                    user= _unitOfWork.Repository<Account>().Find(u => u.UserName.Equals(request.UserName)); 
                    
                    if (user == null) throw new CrudException(HttpStatusCode.BadRequest, "Account Not Found", "");
                    
                    if (user.GoogleId != null && user.GoogleId != "")
                        throw new CrudException(HttpStatusCode.BadRequest, "Login Google cannot login by username and password", "");
                    
                    if (user.Status == false) throw new CrudException(HttpStatusCode.BadRequest, "Your account is block", "");
                    
                    if (!VerifyPasswordHash(request.Password.Trim(), user.PasswordHash, user.PasswordSalt))
                        throw new CrudException(HttpStatusCode.BadRequest, "Password is incorrect", "");
                    
                    if (!Regex.IsMatch(request.Password, "^(?=.*\\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[a-zA-Z]).{8,12}$"))
                        throw new CrudException(HttpStatusCode.BadRequest, "Password is invalid", "");
                }
                if(googleId != null)
                {
                    user= _unitOfWork.Repository<Account>().GetAll().AsNoTracking().SingleOrDefault(u => u.GoogleId.Equals(googleId));
                    
                    if (user == null) throw new CrudException(HttpStatusCode.BadRequest, "Account Not Found", "");
                    
                    if (user.Status == false) throw new CrudException(HttpStatusCode.BadRequest, "Your account is block", "");
                }
                return _mapper.Map<Account, AccountResponse>(user);
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Check login error!!!!!", ex.Message);
            }
        }
        public async Task<List<GameLevelOfAccountResponse>> GetGameLevelByAccountId(int accountId)
        {
            try
            {
                var account = _unitOfWork.Repository<Account>().Find(x => x.Id == accountId);
                
                if (account == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"Account Id {accountId} not found ", "");
               
                if (account.Status == false) throw new CrudException(HttpStatusCode.BadRequest, "Your account is block", "");
               
                var achievements = _unitOfWork.Repository<Achievement>().GetAll().AsNoTracking().Include(x => x.Game)
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
                        gameLevelOfAccountResponse.Level = achievements.Where(a => a.GameId == achievement.GameId).OrderByDescending(a=>a.Level).Distinct().First().Level;
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
