﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Service.Commons;

namespace ThinkTank.Service.DTO.Response
{
    public class FriendResponse
    {
        public int Id { get; set; }
        [BooleanAttribute]
        public bool? Status { get; set; }
        [IntAttribute]
        public int? AccountId1 { get; set; }
        public int AccountId2 { get; set; }
        public string UserName1 { get; set; }
        public string UserName2 { get; set; }
    }
}