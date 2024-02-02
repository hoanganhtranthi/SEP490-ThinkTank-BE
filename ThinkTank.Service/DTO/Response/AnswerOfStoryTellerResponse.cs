using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThinkTank.Service.DTO.Response
{
    public class AnswerOfStoryTellerResponse
    {
        public string LinkImg { get; set; } = null!;
        public int OrdinalNumber { get; set; }
        public int StoryTellerId { get; set; }
    }
}
