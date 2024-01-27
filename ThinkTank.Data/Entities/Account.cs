using System;
using System.Collections.Generic;

namespace ThinkTank.Data.Entities
{
    public partial class Account
    {
        public Account()
        {
            AccountIn1vs1AccountId1Navigations = new HashSet<AccountIn1vs1>();
            AccountIn1vs1AccountId2Navigations = new HashSet<AccountIn1vs1>();
            AccountInContests = new HashSet<AccountInContest>();
            AccountInRooms = new HashSet<AccountInRoom>();
            Achievements = new HashSet<Achievement>();
            Badges = new HashSet<Badge>();
            FriendAccountId1Navigations = new HashSet<Friend>();
            FriendAccountId2Navigations = new HashSet<Friend>();
            IconOfAccounts = new HashSet<IconOfAccount>();
        }

        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public byte[] PasswordHash { get; set; } = null!;
        public byte[] PasswordSalt { get; set; } = null!;
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Avatar { get; set; }
        public int? Coin { get; set; }
        public bool? IsOnline { get; set; }
        public string? RefreshToken { get; set; }
        public byte[] Version { get; set; } = null!;
        public string? Fcm { get; set; }
        public bool? Status { get; set; }
        public string? GoogleId { get; set; }

        public virtual ICollection<AccountIn1vs1> AccountIn1vs1AccountId1Navigations { get; set; }
        public virtual ICollection<AccountIn1vs1> AccountIn1vs1AccountId2Navigations { get; set; }
        public virtual ICollection<AccountInContest> AccountInContests { get; set; }
        public virtual ICollection<AccountInRoom> AccountInRooms { get; set; }
        public virtual ICollection<Achievement> Achievements { get; set; }
        public virtual ICollection<Badge> Badges { get; set; }
        public virtual ICollection<Friend> FriendAccountId1Navigations { get; set; }
        public virtual ICollection<Friend> FriendAccountId2Navigations { get; set; }
        public virtual ICollection<IconOfAccount> IconOfAccounts { get; set; }
    }
}
