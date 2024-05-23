
using System.ComponentModel.DataAnnotations;
using ThinkTank.Domain.Commons;

namespace ThinkTank.Application.DTO.Response
{
    public class AssetResponse
    {
        [Key]
        public int Id { get; set; }
        public string? Value { get; set; }
        [IntAttribute]
        public int? TopicId { get; set; }
        [IntAttribute]
        public int? GameId { get; set; }
        public string? GameName { get; set; }
        public string? TopicName { get; set; }
        public int? Version { get; set; }
        public string?  Answer { get; set; }
        public bool? Status { get; set; }
    }
}
