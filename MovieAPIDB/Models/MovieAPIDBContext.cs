using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MovieAPIDB.Models
{
    public class MovieAPIDBContext : DbContext
    {
        public MovieAPIDBContext() 
        { 
        }

        public MovieAPIDBContext(DbContextOptions<MovieAPIDBContext> options) : base(options)
        {
        }

        public virtual DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                //optionsBuilder.UseSqlServer("Server=Chocobo\\SQLEXPRESS;Database=MovieAPIDB;Trusted_Connection=True;");
                optionsBuilder.UseSqlServer("Server=ANDREW-DESKTOP\\SQLEXPRESS;Database=MovieAPIDB;Trusted_Connection=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
           {
               entity.HasKey(e => e.ID);

               entity.Property(e => e.Username).IsRequired().HasColumnName("Username");

               entity.Property(e => e.Password).IsRequired().HasColumnName("Password");
           });
        }
    }
}
