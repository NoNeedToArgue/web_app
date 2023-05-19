using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Lib
{
    public class UserService
    {
        IDistributedCache cache;
       
        public UserService(IDistributedCache distributedCache)
        {
            cache = distributedCache;
        }

        public async Task<User?> GetUser(int id)
        {
            User? user = null;
           
            var userString = await cache.GetStringAsync(id.ToString());
            if (userString != null) user = JsonSerializer.Deserialize<User>(userString);

            return user;
        }

        public async void SetUser(User user)
        {
            var userString = JsonSerializer.Serialize(user);
            
            await cache.SetStringAsync(user.Id.ToString(), userString, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });
        }
    }
}
