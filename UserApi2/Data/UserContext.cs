using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserApi2.Models;

namespace UserApi2.Data
{
    public class UserContext : DbContext
    {
        public UserContext(DbContextOptions<UserContext> options) : base(options)
        {

        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("User").HasKey(i => i.Id);

            modelBuilder.Entity<UserProperty>().ToTable("UserProperties").HasKey(i => new { i.Key, i.Value, i.UserId });

            modelBuilder.Entity<UserTag>().ToTable("UserTags").HasKey(i => new { i.UserId, i.Tag });

            modelBuilder.Entity<BPFile>().ToTable("UserBPFiles").HasKey(i => i.Id);

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<User> Users { get; set; }

        public DbSet<UserProperty> UserProperties { get; set; }

        public DbSet<UserTag> UserTags { get; set; }

        public DbSet<BPFile> BPFiles { get; set; }
    }
}
