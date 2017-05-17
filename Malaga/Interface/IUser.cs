using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans;

namespace Interface
{
    public class UserProperties
    {
        public string Name { get; set; }
        public string Status { get; set; }
        public HashSet<IUser> Friends { get; set; }

        public UserProperties()
        {
            Friends = new HashSet<IUser>();
        }

        public override string ToString()
        {
            string friends = "";
            foreach (var friend in Friends)
                friends += $"{friend.GetPrimaryKeyString()}, ";

            return $"Name='{Name}' Status='{Status}' Friends: {friends}";
        }
    }
    public interface IUser : IGrainWithStringKey
    {
        Task SetName(string name);
        Task SetStatus(string status);

        Task<UserProperties> GetProperties();

        Task<bool> InviteFriend(IUser friend);
        Task<bool> AddFriend(IUser friend);
    }
}
