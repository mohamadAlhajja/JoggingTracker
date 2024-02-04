using Microsoft.AspNetCore.Identity;

namespace JoggingTrackerAPI.Data.Entities
{
    public class UserEntity : IdentityUser
    {
        public IList<JoggingTrackerEntity>? JoggingTrackerEntities { get; set; }

    }
}
