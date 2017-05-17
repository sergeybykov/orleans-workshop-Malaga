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
    }
}
