using System;
using System.Collections.Generic;

namespace ThinkTank.Data.Entities
{
    public partial class AnswerOfStoryTeller
    {
        public int Id { get; set; }
        public string LinkImg { get; set; } = null!;
        public int OrdinalNumber { get; set; }
        public int StoryTellerId { get; set; }

        public virtual StoryTeller StoryTeller { get; set; } = null!;
    }
}
