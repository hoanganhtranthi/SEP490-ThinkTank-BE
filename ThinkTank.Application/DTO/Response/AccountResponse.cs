
using System.ComponentModel.DataAnnotations;
using ThinkTank.Domain.Commons;

namespace ThinkTank.Application.DTO.Response
{
    public class AccountResponse
    {
        [Key]
        public int Id { get; set; }
        [StringAttribute]
        public string? Code { get; set; }
        [StringAttribute]
        public string? FullName { get; set; }
        [StringAttribute]
        public string? UserName { get; set; }
        [StringAttribute]
        public string? Email { get; set; } 
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Avatar { get; set; }
        public int? Coin { get; set; }
        public bool? IsOnline { get; set; }
        public string? RefreshToken { get; set; }
        public string? Fcm { get; set; }
        public bool? Status { get; set; }
        public string? GoogleId { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public string? AccessToken { get; set; }
        public int? AmountReport { get; set; }
        public byte[] Version { get; set; } 
        public int? VersionToken { get; set; }
    }
}
