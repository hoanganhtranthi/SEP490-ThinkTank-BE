using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Data.Entities;
using ThinkTank.Service.Commons;

namespace ThinkTank.Service.DTO.Response
{
    public class IconResponse
    {
        public int Id { get; set; }
        [StringAttribute]
        public string? Name { get; set; } 
        public string? Avatar { get; set; }
        public int? Price { get; set; }
        [BooleanAttribute]
        public bool? Status { get; set; }
    }
}
