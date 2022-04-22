using DataLayer.Models;
using System.Data.Entity;
namespace DataLayer
{
    public class TwitterDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Information> Informations { get; set; }
    }
}
