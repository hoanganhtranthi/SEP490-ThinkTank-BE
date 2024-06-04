using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.Services.IService
{
    public interface IBadgesService
    {
        Task GetBadge(Account account, string name);
        Task GetBadge(List<Account> accounts, string name);
        Task GetPlowLordBadge(Account account, List<Achievement> list);
    }
}
