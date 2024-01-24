using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThinkTank.Service.Services.IService
{
    public interface IQueueService
    {
        Task EnqueuePlayer(string playerId);
        Task<string> GetRandomPlayersFromQueue(string newPlayerId);

    }
}
