

namespace ThinkTank.Service.Helpers
{
    public class Enum
    {      
        public enum ResourceType
        {
            Anonymous=1,
            MusicPassword=2,
            FlipCard=3,
            ImagesWalkthrough=4
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
        public enum StatusIconType
        {
            All = 1,
            True = 2,
            False = 3
        }
        public enum StatusTopicType
        {
            All = 1,
            True = 2,
            False = 3
        }
        public enum SortOrder
        {
            Ascending = 0,
            Descending = 1,
            None = 2
        }
    }
}
