using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThinkTank.Service.DTO.Response
{
    public class TopicOfGameResponse
    {
        public int Id { get; set; }
        public int GameId { get; set; }
        public string GameName { get; set; } = null!;
    }
}
