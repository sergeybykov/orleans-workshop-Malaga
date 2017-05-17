using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Interface;
using Orleans;
using Orleans.Providers;
using Orleans.Runtime;

namespace Grains
{
    [StorageProvider(ProviderName = "Storage")]
    public class UserGrain : Grain<UserProperties>, IUser, IRemindable
    {
        Random rand;

        public override Task OnActivateAsync()
        {
            rand = new Random(this.GetHashCode());
            //RegisterTimer(OnTimer, null, TimeSpan.FromSeconds(rand.Next(5)), TimeSpan.FromSeconds(5));
            RegisterOrUpdateReminder("poke", TimeSpan.FromSeconds(rand.Next(60)), TimeSpan.FromSeconds(60));
            return base.OnActivateAsync();
        }

        private async Task OnTimer(object state)
        {
            if (State.Friends.Count > 0)
            {
                var friend = State.Friends.ToList()[rand.Next(State.Friends.Count)];
                await friend.Poke(this, "I'm bored!");
            }
        }

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

        public Task Poke(IUser user, string message)
        {
            Console.WriteLine($"[{this.GetPrimaryKeyString()}] User {user.GetPrimaryKeyString()} poked me with '{message}'");
            return Task.CompletedTask;
        }

        public Task ReceiveReminder(string reminderName, TickStatus status)
        {
            return OnTimer(null);
        }
    }
}
