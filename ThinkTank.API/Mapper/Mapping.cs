
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

            CreateMap<ReportRequest, Report>();
            CreateMap<ReportRequest, ReportResponse>();
            CreateMap<Report, ReportResponse>();
            CreateMap<CreateReportRequest, Report>();

            CreateMap<NotificationRequest, Notification>();
            CreateMap<NotificationRequest, NotificationResponse>();
            CreateMap<Notification, NotificationResponse>();

            CreateMap<GameRequest, Game>();
            CreateMap<GameRequest, GameResponse>();
            CreateMap<Game, GameResponse>();

            CreateMap<AchievementRequest, Achievement>();
            CreateMap<AchievementRequest, AchievementResponse>();
            CreateMap<Achievement, AchievementResponse>();
            CreateMap<CreateAchievementRequest, Achievement>();

            CreateMap<TopicRequest, Topic>();
            CreateMap<TopicRequest, TopicResponse>();
            CreateMap<Topic, TopicResponse>();

            CreateMap<IconRequest, Icon>();
            CreateMap<IconRequest, IconResponse>();
            CreateMap<Icon, IconResponse>();
            CreateMap<CreateIconRequest, Icon>();

            CreateMap<IconOfAccountRequest, IconOfAccount>();
            CreateMap<IconOfAccount,IconOfAccountResponse>();


            CreateMap<CreateContestRequest, Contest>();
            CreateMap<Contest, ContestResponse>();
            CreateMap<ContestRequest, ContestResponse>();
            CreateMap<ContestRequest, Contest>();
            CreateMap<UpdateContestRequest, Contest>();

            CreateMap<CreateAssetRequest, Asset>();
            CreateMap<Asset, AssetResponse>().ReverseMap();
            CreateMap<AssetRequest, AssetResponse>();
            CreateMap<AssetRequest, Asset>();

            CreateMap<CreateTypeOfAssetRequest, TypeOfAsset>();
            CreateMap<TypeOfAsset, TypeOfAssetResponse>().ReverseMap();
            CreateMap<TypeOfAssetRequest, TypeOfAssetResponse>();
            CreateMap<TypeOfAssetRequest, TypeOfAsset>();

            CreateMap<ChallengeRequest, ChallengeResponse>();
            CreateMap<ChallengeRequest, Challenge>();
            CreateMap<Challenge, ChallengeResponse>();

            CreateMap<BadgeRequest, Badge>();
            CreateMap<BadgeRequest, BadgeResponse>();
            CreateMap<Badge, BadgeResponse>();
            CreateMap<CreateBadgeRequest, Badge>();

            CreateMap<AccountInContestRequest, AccountInContest>();
            CreateMap<AccountInContest, AccountInContestResponse>();
            CreateMap<AccountInContestRequest, AccountInContestResponse>();
            CreateMap<CreateAccountInContestRequest, AccountInContest>();
            CreateMap<UpdateAccountInContestRequest, AccountInContest>();

            CreateMap<CreateAssetOfContestRequest, AssetOfContest>();
            CreateMap<AssetOfContest, AssetOfContestResponse>();
        }

    }
}