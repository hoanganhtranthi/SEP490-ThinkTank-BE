using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Service.Commons;
using ThinkTank.Service.DTO.Request;

namespace ThinkTank.Service.DTO.Response
{
    public class StoryTellerResponse
    {
        public int Id { get; set; }
        public string? Description { get; set; } 
        [IntAttribute]
        public int? TopicOfGameId { get; set; }
        public string? TopicName { get; set; }
        public virtual ICollection<AnswerOfStoryTellerResponse> AnswerOfStoryTellers { get; set; }
    }
}
