﻿using System;


namespace ThinkTank.Application.DTO.Request
{
    public class CreateReportRequest
    {
        public string? Description { get; set; } = null!;
        public int? AccountId1 { get; set; }
        public int? AccountId2 { get; set; }
        public string? Title { get; set; } = null!;
    }
}
