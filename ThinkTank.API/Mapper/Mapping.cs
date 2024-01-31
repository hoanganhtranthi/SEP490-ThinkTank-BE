
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

            CreateMap<GameRequest, Game>();
            CreateMap<GameRequest, GameResponse>();
            CreateMap<Game, GameResponse>();

            CreateMap<ResourceRequest, Anonymous>();
            CreateMap<ResourceRequest, AnonymousResponse>();
            CreateMap<Anonymous, AnonymousResponse>();
            CreateMap<AnonymousRequest, Anonymous>();

            CreateMap<ResourceRequest, MusicPassword>();
            CreateMap<ResourceRequest, MusicPasswordResponse>();
            CreateMap<MusicPassword, MusicPasswordResponse>();
            CreateMap<MusicPasswordRequest, MusicPassword>();

            CreateMap<TopicRequest, Topic>();
            CreateMap<TopicRequest, TopicResponse>();
            CreateMap<Topic, TopicResponse>();
            CreateMap<CreateTopicOfGameRequest, Topic>();

            CreateMap<TopicOfGame, TopicOfGameResponse>();
        }

    }
}
