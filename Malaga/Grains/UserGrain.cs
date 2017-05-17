using System.Threading.Tasks;
using Interface;
using Orleans;

namespace Grains
{
    public class UserGrain : Grain, IUser
    {
        private UserProperties _props = new UserProperties();

        public Task SetName(string name)
        {
            _props.Name = name;
            return Task.CompletedTask;
        }

        public Task SetStatus(string status)
        {
            _props.Status = status;
            return Task.CompletedTask;
        }

        public Task<UserProperties> GetProperties()
        {
            return Task.FromResult(_props);
        }

        public Task<bool> InviteFriend(IUser friend)
        {
            if (!_props.Friends.Contains(friend))
                _props.Friends.Add(friend);

            return Task.FromResult(true);
        }

        public async Task<bool> AddFriend(IUser friend)
        {
            var ok = await friend.InviteFriend(this);
            if (!ok)
                return false;
            if (!_props.Friends.Contains(friend))
                _props.Friends.Add(friend);
            return true;
        }
    }
}
