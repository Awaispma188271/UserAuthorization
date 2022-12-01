using Microsoft.EntityFrameworkCore;

namespace userAuth.Model
{
    public class UserContext : DbContext 
    {
        public UserContext(DbContextOptions options) : base(options)
        {

        }
        
       public DbSet<RegisterUser> Registers { get; set; }
       
        public DbSet<Role> Roles { get; set; }

        public DbSet<UserRole> UserRoles { get; set; }

        public DbSet<TblRefreshToken> tblRefreshTokens { get; set; }
    }
}
