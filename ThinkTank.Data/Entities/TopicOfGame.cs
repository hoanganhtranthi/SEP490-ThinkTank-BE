using System;
using System.Collections.Generic;

namespace ThinkTank.Data.Entities
{
    public partial class TopicOfGame
    {
        public TopicOfGame()
        {
            Anonymous = new HashSet<Anonymous>();
            FlipCardAndImagesWalkthroughs = new HashSet<FlipCardAndImagesWalkthrough>();
            MusicPasswords = new HashSet<MusicPassword>();
            StoryTellers = new HashSet<StoryTeller>();
        }

        public int Id { get; set; }
        public int GameId { get; set; }
        public int TopicId { get; set; }

        public virtual Game Game { get; set; } = null!;
        public virtual Topic Topic { get; set; } = null!;
        public virtual ICollection<Anonymous> Anonymous { get; set; }
        public virtual ICollection<FlipCardAndImagesWalkthrough> FlipCardAndImagesWalkthroughs { get; set; }
        public virtual ICollection<MusicPassword> MusicPasswords { get; set; }
        public virtual ICollection<StoryTeller> StoryTellers { get; set; }
    }
}
