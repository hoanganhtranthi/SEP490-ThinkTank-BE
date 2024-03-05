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
        private readonly ICacheService _cacheService;
        public AccountService(IUnitOfWork unitOfWork, IMapper mapper, IConfiguration configuration, ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
            _config = configuration;
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
                if (createAccountRequest.GoogleId != null && createAccountRequest.GoogleId != "")
                {
                    var acc = _unitOfWork.Repository<Account>().Find(x => x.GoogleId == createAccountRequest.GoogleId);
                    if (acc != null)
                        throw new CrudException(HttpStatusCode.BadRequest, "GoogleId has already !!!", "");
                }
                CreatPasswordHash(createAccountRequest.Password, out byte[] passwordHash, out byte[] passwordSalt);
                customer.PasswordHash = passwordHash;
                customer.PasswordSalt = passwordSalt;
                customer.Status = true;
                customer.Avatar = "https://firebasestorage.googleapis.com/v0/b/thinktank-79ead.appspot.com/o/System%2Favatar-trang-4.jpg?alt=media&token=2ab24327-c484-485a-938a-ed30dc3b1688";
                customer.RegistrationDate = DateTime.Now;
                if (createAccountRequest.Fcm == null || createAccountRequest.Fcm == "")
                    customer.Fcm = null;
                Guid id = Guid.NewGuid();
                customer.Code = id.ToString().Substring(0, 8).ToUpper();
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
        public async Task<dynamic> CreateMailMessage(string email)
        {
            bool success = false;
            string token = "";
            var randomToken = GenerateRandomNo();
            string to = email;
            string from = _config["EmailUserName"];

            var acc = _unitOfWork.Repository<Account>().Find(a => a.Email.Equals(email));
            if (acc == null)
            {
                throw new CrudException(HttpStatusCode.NotFound, $"Not found account with gmail {email}", "");
            }
            if (acc.Status == false) throw new CrudException(HttpStatusCode.BadRequest, "Your account is block", "");
            MailMessage message = new MailMessage(from, to);
            message.Subject = "Account Verification Code";
            message.Body = $"<p> Hi {acc.FullName}, </p> \n <span> <p> We received a request to access your Account {email} through your email address. Your Account verification code is:</p></span>\n" +
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
                Token = token
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
                var customers = _unitOfWork.Repository<Account>().GetAll()
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
                Account user = _unitOfWork.Repository<Account>().GetAll()
                  .FirstOrDefault(u => u.UserName.Equals(request.UserName));
                    if (user == null) throw new CrudException(HttpStatusCode.BadRequest, "Account Not Found", "");
                    if (!VerifyPasswordHash(request.Password.Trim(), user.PasswordHash, user.PasswordSalt))
                        throw new CrudException(HttpStatusCode.BadRequest, "Password is incorrect", "");
                    if (user.Status == false) throw new CrudException(HttpStatusCode.BadRequest, "Your account is block", "");
                    var token = GenerateRefreshToken(user);
                    user.RefreshToken = token;
                    if(request.Fcm != null && request.Fcm != "")
                    user.Fcm = request.Fcm;
                    await _unitOfWork.Repository<Account>().Update(user, user.Id);
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
                    var token = GenerateRefreshToken(user);
                   rs = _mapper.Map<Account, AccountResponse>(user);
                    rs.UserName = "Admin";
                    rs.AccessToken = GenerateJwtToken(user);
                    rs.RefreshToken = token;
                    var expiryTime = DateTime.MaxValue;
                    var adminAccount = _cacheService.GetData<AdminAccountResponse>("AdminAccount");
                    if (adminAccount != null)
                    {
                        adminAccount.VersionTokenAdmin += 1;
                        adminAccount.RefreshTokenAdmin = token;
                        _cacheService.SetData<AdminAccountResponse>("AdminAccount", adminAccount, expiryTime);
                    }
                    else
                    {
                        adminAccount = new AdminAccountResponse();
                        adminAccount.VersionTokenAdmin = 1;
                        adminAccount.RefreshTokenAdmin = token;
                        _cacheService.SetData<AdminAccountResponse>("AdminAccount", adminAccount, expiryTime);
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
                new Claim("version", BitConverter.ToString(customer.Version).Replace("-", "")),
                new Claim(ClaimTypes.Email , customer.Email),
                 });
                tokenDescriptor.Expires = DateTime.Now.AddMinutes(20);
                tokenDescriptor.SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature);
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var rs = tokenHandler.WriteToken(token);
                return rs;
            }
            else
            {
                var adminAccountResponse = _cacheService.GetData<AdminAccountResponse>("AdminAccount");
                var t = 0;
                if (adminAccountResponse != null)
                    t = adminAccountResponse.VersionTokenAdmin + 1;
                else t = 1;
                tokenDescriptor.Subject = new ClaimsIdentity(new Claim[]
                {
                new Claim(ClaimTypes.NameIdentifier, customer.Id.ToString()),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim("version",t.ToString()),
                });
                tokenDescriptor.Expires = DateTime.Now.AddMinutes(20);
                tokenDescriptor.SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature);
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var rs = tokenHandler.WriteToken(token);
                return rs;
            }


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
                 new Claim("version", BitConverter.ToString(customer.Version).Replace("-", "")),
                new Claim(ClaimTypes.Email , customer.Email),
                 });
            }
            else
            {
                var adminAccountResponse = _cacheService.GetData<AdminAccountResponse>("AdminAccount");
                var t = 0;
                if (adminAccountResponse != null)
                    t = adminAccountResponse.VersionTokenAdmin + 1;
                else t = 1;
                tokenDescriptor.Subject = new ClaimsIdentity(new Claim[]
                {
                new Claim(ClaimTypes.NameIdentifier, customer.Id.ToString()),
                new Claim(ClaimTypes.Role, "Admin"),
                 new Claim("version", t.ToString()),
                });
            }
            tokenDescriptor.Expires = DateTime.Now.AddMonths(6);
            tokenDescriptor.SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature);
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        public async Task<AccountResponse> LoginGoogle(string data, string fcm)
        {
            try
            {
                Account user = _unitOfWork.Repository<Account>().GetAll()
               .FirstOrDefault(u => u.GoogleId.Equals(data));

                if (user == null) throw new CrudException(HttpStatusCode.BadRequest, "Account Not Found", "");
                if (user.Status == false) throw new CrudException(HttpStatusCode.BadRequest, "Your account is block", "");
                var token = GenerateRefreshToken(user);
                user.RefreshToken = token;
                user.Fcm = fcm;
                await _unitOfWork.Repository<Account>().Update(user, user.Id);
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

        public async Task<AccountResponse> RevokeRefreshToken(int userId)
        {
            try
            {
                AccountResponse rs = new AccountResponse();
                if (userId==0)
                {
                    var expiryTime = DateTime.MaxValue;
                    var adminAccountResponse = _cacheService.GetData<AdminAccountResponse>("AdminAccount");
                    if (adminAccountResponse != null)
                    {
                        adminAccountResponse.VersionTokenAdmin += 1;
                        adminAccountResponse.RefreshTokenAdmin = null;
                        _cacheService.SetData<AdminAccountResponse>("AdminAccount", adminAccountResponse, expiryTime);
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

                if (request.DateOfBirth >= DateTime.Now.AddDays(-1))
                    throw new CrudException(HttpStatusCode.BadRequest, "Date Of Birth is invalid", "");

                var existingUsernameAccount = _unitOfWork.Repository<Account>().GetAll().FirstOrDefault(c => c.UserName.Equals(request.UserName) && c.Id != accountId);
                if (existingUsernameAccount != null)
                    throw new CrudException(HttpStatusCode.BadRequest, "Username has already been taken", "");

                var existingEmailAccount = _unitOfWork.Repository<Account>().GetAll().FirstOrDefault(c => c.Email.Equals(request.Email) && c.Id != accountId);
                if (existingEmailAccount != null)
                    throw new CrudException(HttpStatusCode.BadRequest, "Email has already been registered", "");

                _mapper.Map<UpdateAccountRequest, Account>(request, account);

                if (request.Avatar == null) account.Avatar = account.Avatar;
                else account.Avatar = request.Avatar;
                account.Id = accountId;
                if (request.OldPassword != null && request.NewPassword != null)
                {
                    if (!VerifyPasswordHash(request.OldPassword.Trim(), account.PasswordHash, account.PasswordSalt))
                        throw new CrudException(HttpStatusCode.BadRequest, "Old Password is not match", "");
                    CreatPasswordHash(request.NewPassword, out byte[] passwordHash, out byte[] passwordSalt);
                    account.PasswordHash = passwordHash;
                    account.PasswordSalt = passwordSalt;
                }
                await _unitOfWork.Repository<Account>().Update(account, accountId);
                await _unitOfWork.CommitAsync();
                return _mapper.Map<Account, AccountResponse>(account);
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

        public async Task<AccountResponse> UpdatePass(ResetPasswordRequest request)
        {
            try
            {
                Account customer = _unitOfWork.Repository<Account>()
                     .Find(c => c.Email.Equals(request.Email));
                if (customer == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found account with email{request.Email}", "");
                }
                if (customer.Status == false) throw new CrudException(HttpStatusCode.BadRequest, "Your account is block", "");
                CreatPasswordHash(request.NewPassword, out byte[] passwordHash, out byte[] passwordSalt);
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
                var key = Encoding.ASCII.GetBytes(_config["ApiSetting:Secret"]);
                TokenValidationParameters tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                };
                var tokenInVerification = jwtTokenHandler.ValidateToken(request.AccessToken, tokenValidationParameters, out var tokenValidation);
                if (tokenValidation is not JwtSecurityToken securityToken)
                    throw new CrudException(HttpStatusCode.BadRequest, "Invalid Access Token", "");
                var utcExpiredDate = long.Parse(tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);

                var expiredDate = DateTimeOffset.FromUnixTimeSeconds(utcExpiredDate).DateTime;
                if (expiredDate.AddMinutes(-5) > DateTime.UtcNow) throw new CrudException(HttpStatusCode.BadRequest, "Access Token is not expried", "");
                if (tokenInVerification.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role).Value.Equals("Admin"))
                {
                    var adminAccountResponse = _cacheService.GetData<AdminAccountResponse>("AdminAccount");
                    if (adminAccountResponse != null)
                    {
                        if (!request.RefreshToken.Equals(adminAccountResponse.RefreshTokenAdmin))
                            throw new CrudException(HttpStatusCode.BadRequest, "Invalid Refresh Token", "");
                    }

                    acc.FullName = "Admin";
                    var token = GenerateJwtToken(acc);
                    cus = _mapper.Map<Account, AccountResponse>(acc);
                    cus.AccessToken = token;
                    cus.RefreshToken = GenerateRefreshToken(acc);

                }
                else
                {

                    var list = _unitOfWork.Repository<Account>().GetAll();
                    if (list != null) acc = list.Where(a => a.RefreshToken != null && a.RefreshToken.Equals(request.RefreshToken)).SingleOrDefault();
                    if (acc == null) throw new CrudException(HttpStatusCode.BadRequest, "Invalid Refresh Token", "");
                    if (acc.Status == false) throw new CrudException(HttpStatusCode.BadRequest, "Your account is block", "");
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
                if(_unitOfWork.Repository<Report>().GetAll().Where(x=>x.AccountId2==id).ToList().Count() <=0)
                    throw new CrudException(HttpStatusCode.BadRequest, $"Id Account {id} has not been reported by anyone", "");
                account.Status = !account.Status;
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
                        gameLevelOfAccountResponse.GameId = achievement.GameId;
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
