using Microsoft.AspNetCore.Http.Connections;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Service.Services.IService;

namespace ThinkTank.Service.Services.ImpService
{
    public class QueueService :IQueueService
    {
        private readonly ConnectionMultiplexer _redisConnection;
        private readonly IDatabase _redisDatabase;
        private readonly string _queueKey = "MatchmakingQueue";

        public QueueService()
        {
            _redisConnection = ConnectionMultiplexer.Connect(new ConfigurationOptions
            {
                EndPoints = { "redis-16974.c295.ap-southeast-1-1.ec2.cloud.redislabs.com:16974" },
                Password = "a7uAOmGIE4BCbiA9BcuchWliIsJXOcH0",
                DefaultDatabase = 0,
                AbortOnConnectFail = false,
                AllowAdmin = true,
                SyncTimeout = 100000
            });
            _redisDatabase = _redisConnection.GetDatabase();
        }

        public async Task EnqueuePlayer(string playerId)
        {
            var queuedPlayers = await _redisDatabase.ListRangeAsync(_queueKey);
            if (!queuedPlayers.Any(x => x == playerId))
            {
                await _redisDatabase.ListRightPushAsync(_queueKey, playerId);
            }
           
        }

        private static SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        public async Task<string> GetRandomPlayersFromQueue(string newPlayerId)
        {
            await semaphore.WaitAsync();

            try
            {
                // Lấy tất cả các phần tử từ hàng đợi
                var queuedPlayers = await _redisDatabase.ListRangeAsync(_queueKey);

                // Tạo danh sách mới để lưu trữ các người chơi không giống với newPlayerId
                var filteredPlayers = new List<string>();

                foreach (var queuedPlayer in queuedPlayers)
                {
                    // So sánh với newPlayerId và chỉ thêm vào danh sách nếu không giống
                    if (queuedPlayer != newPlayerId)
                    {
                        filteredPlayers.Add(queuedPlayer.ToString());
                    }
                }

                // Kiểm tra xem có đủ người chơi không
                if (filteredPlayers.Count >= 2)
                {
                    // Lấy hai người chơi đầu tiên từ danh sách mới
                    var selectedPlayers = filteredPlayers.Take(2);

                    // Loại bỏ hai người chơi mới khỏi hàng đợi
                    foreach (var selectedPlayer in selectedPlayers)
                    {
                        await _redisDatabase.ListRemoveAsync(_queueKey, selectedPlayer);
                    }

                    // Trả về danh sách hai người chơi được chọn
                    return string.Join(",", selectedPlayers);
                }

                // Trường hợp không đủ người chơi
                return null;
            }
            finally
            {
                semaphore.Release();
            }
        }
    }

}
