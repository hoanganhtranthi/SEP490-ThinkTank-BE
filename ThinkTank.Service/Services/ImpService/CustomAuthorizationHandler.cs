using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Server.IIS.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Repository.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Service.DTO.Response;
using ThinkTank.Service.Exceptions;
using ThinkTank.Service.Services.IService;

namespace ThinkTank.Service.Services.ImpService
{
    public class CustomAuthorizationHandler : AuthorizationHandler<CustomRequirement>
    {
        private readonly IAccountService _accountRepository;
        private readonly IFirebaseRealtimeDatabaseService _firebaseRealtimeDatabaseService;

        public CustomAuthorizationHandler(IAccountService accountRepository,IFirebaseRealtimeDatabaseService firebaseRealtimeDatabaseService)
        {
            _accountRepository = accountRepository;
            _firebaseRealtimeDatabaseService = firebaseRealtimeDatabaseService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CustomRequirement requirement)
        {
           
            var idClaimValue = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(idClaimValue))
            {
                context.Fail();
                return;
            }

            try
            {

                var utcExpiredDate = long.Parse(context.User.FindFirst(x => x.Type == JwtRegisteredClaimNames.Exp).Value);

                var expiredDate = DateTimeOffset.FromUnixTimeSeconds(utcExpiredDate).DateTime;
                if (expiredDate < DateTime.UtcNow)
                {

                    throw new CrudException(HttpStatusCode.Unauthorized, $"{HttpStatusCode.Unauthorized}",$"{HttpStatusCode.Unauthorized}");
                }
                var role = context.User.FindFirst(ClaimTypes.Role).Value;
                var versionClaimValue = context.User.FindFirst("version")?.Value;
                if (string.IsNullOrEmpty(versionClaimValue))
                {
                    context.Fail();
                    return;
                }
                if (role.Equals("Admin"))
                {
                    var adminAccountResponse = _firebaseRealtimeDatabaseService.GetAsync<AdminAccountResponse>("AdminAccount").Result;
                    if(adminAccountResponse != null)
                    {
                        if(Int32.Parse(versionClaimValue)==adminAccountResponse.VersionTokenAdmin)
                            context.Succeed(requirement);
                        else context.Fail();
                    }                      
                }
                else
                {
                    var accountId = int.Parse(idClaimValue);
                    var account = await _accountRepository.GetAccountById(accountId);
                    var versionCheck = account.VersionToken;
                    if (versionClaimValue.SequenceEqual(versionCheck.ToString()))
                    {
                        context.Succeed(requirement);
                    }
                    else
                    {
                        context.Fail();
                    }
                }
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.BadRequest, "Progress Error!!!", ex.InnerException?.Message);
            }

        }
    }

    public class CustomRequirement : IAuthorizationRequirement
    {
    }
}
