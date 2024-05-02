using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HSProject.Authentication;
public class IdentityDbContext(DbContextOptions<IdentityDbContext> options) :
    IdentityDbContext<ApplicationUser>(options) {
    protected override void OnModelCreating(ModelBuilder builder) {
        base.OnModelCreating(builder);
    }
}