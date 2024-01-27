
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Data.Entities;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;

namespace MuTote.API.Mapper
{
    public class Mapping : Profile
    {
        public Mapping() {
            CreateMap<AccountRequest, Account>();
            CreateMap<AccountRequest, AccountResponse>();
            CreateMap<Account, AccountResponse>();
            CreateMap<CreateAccountRequest, Account>();
            CreateMap<UpdateAccountRequest, Account>();

            CreateMap<FriendRequest, Friend>();
            CreateMap<FriendRequest, FriendResponse>();
            CreateMap<Friend, FriendResponse>();
            CreateMap<CreateFriendRequest, Friend>();
        }

    }
}
