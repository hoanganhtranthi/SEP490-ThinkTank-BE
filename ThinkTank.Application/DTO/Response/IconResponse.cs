
using System.ComponentModel.DataAnnotations;
using ThinkTank.Domain.Commons;

namespace ThinkTank.Application.DTO.Response
{
    public class IconResponse
    {
        [Key]
        public int Id { get; set; }
        [StringAttribute]
        public string? Name { get; set; } 
        public string? Avatar { get; set; }
        public int? Price { get; set; }
        [BooleanAttribute]
        public bool? Status { get; set; }
    }
}
