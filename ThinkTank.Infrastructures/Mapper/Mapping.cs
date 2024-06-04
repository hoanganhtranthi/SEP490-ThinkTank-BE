
using AutoMapper;
using ThinkTank.Domain.Entities;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Infrastructures.Mapper
{
    public class Mapping : Profile
    {
        public Mapping() {
            CreateMap<AccountRequest, Account>();
            CreateMap<AccountRequest, AccountResponse>();
            CreateMap<Account, AccountResponse>();
            CreateMap<CreateAccountRequest, Account>();
            CreateMap<UpdateAccountRequest, Account>();
            CreateMap<LoginGoogleRequest, Account>();

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

            CreateMap<Game, GameResponse>();

            CreateMap<AchievementRequest, Achievement>();
            CreateMap<AchievementRequest, AchievementResponse>();
            CreateMap<Achievement, AchievementResponse>();
            CreateMap<CreateAchievementRequest, Achievement>();

            CreateMap<CreateTopicRequest, Topic>();
            CreateMap<TopicRequest, Topic>();
            CreateMap<TopicRequest, TopicResponse>();
            CreateMap<Topic, TopicResponse>();

            CreateMap<IconRequest, Icon>();
            CreateMap<IconRequest, IconResponse>();
            CreateMap<Icon, IconResponse>();
            CreateMap<CreateIconRequest, Icon>();

            CreateMap<CreateIconOfAccountRequest, IconOfAccount>();
            CreateMap<IconOfAccount, IconOfAccountResponse>();
            CreateMap<IconOfAccountRequest, IconOfAccountResponse>();
            CreateMap<IconOfAccountRequest, IconOfAccount>();


            CreateMap<CreateAndUpdateContestRequest, Contest>();
            CreateMap<Contest, ContestResponse>();
            CreateMap<ContestRequest, ContestResponse>();
            CreateMap<ContestRequest, Contest>();

            CreateMap<CreateAssetRequest, Asset>();
            CreateMap<Asset, AssetResponse>().ReverseMap();
            CreateMap<AssetRequest,AssetResponse>();
            CreateMap<AssetRequest, Asset>();

            CreateMap<TypeOfAsset, TypeOfAssetResponse>().ReverseMap();

            CreateMap<ChallengeRequest, ChallengeResponse>();
            CreateMap<ChallengeRequest, Challenge>();
            CreateMap<Challenge, ChallengeResponse>();

            CreateMap<CreateBadgeRequest, Badge>();

            CreateMap<AccountInContestRequest, AccountInContest>();
            CreateMap<AccountInContest, AccountInContestResponse>();
            CreateMap<AccountInContestRequest, AccountInContestResponse>();
            CreateMap<UpdateAccountInContestRequest, AccountInContest>();


            CreateMap<AccountIn1vs1, AccountIn1vs1Response>();
            CreateMap<CreateAndUpdateAccountIn1vs1Request, AccountIn1vs1>();

            CreateMap<CreateAssetOfContestRequest, AssetOfContest>();
            CreateMap<AssetOfContest, AssetOfContestResponse>().ReverseMap();

            CreateMap<TypeOfAssetInContest, TypeOfAssetInContestResponse>().ReverseMap();

            CreateMap<CreateRoomRequest, Room>();
            CreateMap<Room, RoomResponse>().ReverseMap();
            CreateMap<RoomRequest, RoomResponse>();
            CreateMap<RoomRequest, Room>();

            CreateMap<AccountInRoomRequest, AccountInRoom>();
            CreateMap<AccountInRoom, AccountInRoomResponse>();
            CreateMap<AccountInRoomRequest, AccountInRoomResponse>();
            CreateMap<CreateAndUpdateAccountInRoomRequest, AccountInRoom>();
        }

    }
}