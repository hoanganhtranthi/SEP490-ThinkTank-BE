﻿
using System.ComponentModel.DataAnnotations;
using static ThinkTank.Service.Helpers.Enum;

namespace ThinkTank.Service.DTO.Request
{
    public class PagingRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "Only positive number allowed")]
        public int Page { get; set; } = 1;
        [Range(1, int.MaxValue, ErrorMessage = "Only positive number allowed")]
        public int PageSize { get; set; } = 10;
        public SortOrder SortType { get; set; }
        public string ColName { get; set; } = "Id";
    }
}