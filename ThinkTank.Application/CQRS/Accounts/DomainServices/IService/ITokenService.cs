using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.CQRS.Accounts.DomainServices.IService
{
    public interface ITokenService
    {
        string GenerateRefreshToken(Account? account, DateTime date);
        string GenerateJwtToken(Account? account, DateTime date);
    }
}
