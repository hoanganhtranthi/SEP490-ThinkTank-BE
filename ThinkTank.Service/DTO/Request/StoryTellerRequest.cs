using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Data.Entities;

namespace ThinkTank.Service.DTO.Request
{
    public class StoryTellerRequest
    {
        public string Description { get; set; } = null!;
        public int TopicOfGameId { get; set; }
        public virtual ICollection<AnswerOfStoryTellerRequest> AnswerOfStoryTellers { get; set; }
    }
}
