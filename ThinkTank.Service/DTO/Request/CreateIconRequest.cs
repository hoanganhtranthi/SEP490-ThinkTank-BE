using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
