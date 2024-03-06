using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThinkTank.Service.Helpers
{
    public class Enum
    {      
        public enum ResourceType
        {
            Anonymous=1,
            MusicPassword=2,
            FlipCard=3,
            ImagesWalkthrough=4,
            StoryTeller=5,
        }
        public enum FileType
        {
            System=1,
            Player=2
        }
        public enum StatusType
        {
            All=1,
            True=2,
            False=3,
            Null=4,
        }
    }
}
