using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Data.Entities;

namespace ThinkTank.Service.DTO.Request
{
    public class CreateTopicOfGameRequest
    {
        public string Name { get; set; } = null!;
        public virtual ICollection<int> TopicOfGamesId { get; set; }
    }
}
