
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

            CreateMap<ResourceRequest, FlipCardAndImagesWalkthrough>();
            CreateMap<ResourceRequest, FlipCardAndImagesWalkthroughResponse>();
            CreateMap<FlipCardAndImagesWalkthrough, FlipCardAndImagesWalkthroughResponse>();
            CreateMap<FlipCardAndImagesWalkthroughRequest, FlipCardAndImagesWalkthrough>();

            CreateMap<ResourceRequest, StoryTeller>();
            CreateMap<ResourceRequest, StoryTellerResponse>();
            CreateMap<StoryTeller, StoryTellerResponse>();
            CreateMap<StoryTellerRequest, StoryTeller>();

            CreateMap<AnswerOfStoryTeller, AnswerOfStoryTellerRequest>().ReverseMap();
            CreateMap<AnswerOfStoryTellerRequest, AnswerOfStoryTellerResponse>();
            CreateMap<AnswerOfStoryTeller, AnswerOfStoryTellerResponse>();

            CreateMap<AchievementRequest, Achievement>();
            CreateMap<AchievementRequest, AchievementResponse>();
            CreateMap<Achievement, AchievementResponse>();
            CreateMap<CreateAchievementRequest, Achievement>();

            CreateMap<TopicRequest, Topic>();
            CreateMap<TopicRequest, TopicResponse>();
            CreateMap<Topic, TopicResponse>();
            CreateMap<CreateTopicOfGameRequest, Topic>();

            CreateMap<IconRequest, Icon>();
            CreateMap<IconRequest, IconResponse>();
            CreateMap<Icon, IconResponse>();
            CreateMap<CreateIconRequest, Icon>();

            CreateMap<IconOfAccountRequest, IconOfAccount>();
            CreateMap<IconOfAccount,IconOfAccountResponse>();

            CreateMap<TopicOfGame, TopicOfGameResponse>();

            //
            CreateMap<CreateContestRequest, Contest>();
            CreateMap<Contest, ContestResponse>();
            CreateMap<CreateContestRequest, ContestResponse>();

            CreateMap<CreatePrizeOfContestRequest, PrizeOfContest>();
            CreateMap<PrizeOfContest, PrizeOfContestResponse>();
            CreateMap<CreatePrizeOfContestRequest, PrizeOfContestResponse>();

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

            CreateMap<FlipCardAndImagesWalkthroughOfContestRequest, FlipCardAndImagesWalkthroughOfContest>();
            CreateMap<FlipCardAndImagesWalkthroughOfContest, FlipCardAndImagesWalkthroughOfContestResponse>();

            CreateMap<AnonymityOfContestRequest, AnonymityOfContest>();
            CreateMap<AnonymityOfContest, AnonymityOfContestResponse>();

            CreateMap<MusicPasswordOfContestRequest, MusicPasswordOfContest>();
            CreateMap<MusicPasswordOfContest,  MusicPasswordOfContestResponse>();

            CreateMap<ResourceOfContestRequest, AnonymityOfContestResponse>();
            CreateMap<ResourceOfContestRequest, FlipCardAndImagesWalkthroughOfContestResponse>();
            CreateMap<ResourceOfContestRequest, MusicPasswordOfContestResponse>();
            CreateMap<ResourceOfContestRequest, PrizeOfContestResponse>();
        }

    }
}