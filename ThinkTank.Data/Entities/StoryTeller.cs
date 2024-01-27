using System;
using System.Collections.Generic;

namespace ThinkTank.Data.Entities
{
    public partial class StoryTeller
    {
        public StoryTeller()
        {
            AnswerOfStoryTellers = new HashSet<AnswerOfStoryTeller>();
        }

        public int Id { get; set; }
        public string Description { get; set; } = null!;
        public int TopicOfGameId { get; set; }

        public virtual TopicOfGame TopicOfGame { get; set; } = null!;
        public virtual ICollection<AnswerOfStoryTeller> AnswerOfStoryTellers { get; set; }
    }
}
