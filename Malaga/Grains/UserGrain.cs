using System;
using System.Threading;
using System.Threading.Tasks;
using Interface;
using Orleans;
using Orleans.Providers;

namespace Grains
{
    [StorageProvider(ProviderName = "Storage")]
    public class UserGrain : Grain<UserProperties>, IUser
    {
        public Task SetName(string name)
        {
            State.Name = name;
            return WriteStateAsync();
        }

        public Task SetStatus(string status)
        {
            State.Status = status;
            return WriteStateAsync();
        }

        public Task<UserProperties> GetProperties()
        {
            return Task.FromResult(State);
        }

        public async Task<bool> InviteFriend(IUser friend)
        {
            if (!State.Friends.Contains(friend))
                State.Friends.Add(friend);

            await WriteStateAsync();

            return true;
        }

        public async Task<bool> AddFriend(IUser friend)
        {
            var ok = await friend.InviteFriend(this);
             if (!ok)
                return false;
            if (!State.Friends.Contains(friend))
                State.Friends.Add(friend);

            await WriteStateAsync();

            return true;
        }
    }
}
