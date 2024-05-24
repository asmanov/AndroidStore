using Microsoft.EntityFrameworkCore;

namespace AndroidStore.Data
{
    public class UsersContext : DbContext
    {
        public UsersContext(DbContextOptions<UsersContext> options)
            : base(options)
        {
        }

        public DbSet<AndroidStore.Models.User> User { get; set; } = default!;
    }
}
