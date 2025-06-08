using Microsoft.EntityFrameworkCore;

namespace becore.api.Scheme;

public class ApplicationContext : DbContext
{
    public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options) { }
}