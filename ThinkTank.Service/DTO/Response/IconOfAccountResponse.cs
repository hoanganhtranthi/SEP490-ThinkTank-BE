
using System.ComponentModel.DataAnnotations;
using ThinkTank.Domain.Commons;

namespace ThinkTank.Application.DTO.Response
{
    public class IconOfAccountResponse
    {
        [Key]
        public int Id { get; set; }
        public bool? IsAvailable { get; set; }
        [IntAttribute]
        public int? AccountId { get; set; }
        [IntAttribute]
        public int? IconId { get; set; }
        public string? UserName { get; set; }
        public string? IconAvatar { get; set; }
        public string? IconName { get; set; }
    }
}
