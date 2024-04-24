

namespace ThinkTank.Service.DTO.Request
{
    public class CreateAssetRequest
    {
        public string Value { get; set; } = null!;
        public int TopicId { get; set; }
        public int TypeOfAssetId { get; set; }
    }
}
