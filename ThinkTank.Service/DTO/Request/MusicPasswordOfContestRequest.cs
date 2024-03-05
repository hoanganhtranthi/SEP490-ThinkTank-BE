﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThinkTank.Service.DTO.Request
{
    public class MusicPasswordOfContestRequest
    {
        public string Password { get; set; } = null!;
        public string SoundLink { get; set; }=null!;

        public int ContestId { get; set; }
    }
}
