
using System.ComponentModel.DataAnnotations;

namespace ThinkTank.Service.DTO.Request
{
    public class CreateIconRequest
    {
        public string Name { get; set; } = null!;
        public string Avatar { get; set; } = null!;
        [Range(10,int.MaxValue, ErrorMessage = "Only positive number allowed")]
        public int Price { get; set; }
    }
}
