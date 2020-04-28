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
        public virtual DbSet<UserMovie> Favorites { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=Chocobo\\SQLEXPRESS;Database=MovieAPIDB;Trusted_Connection=True;");
                //optionsBuilder.UseSqlServer("Server=ANDREW-DESKTOP\\SQLEXPRESS;Database=MovieAPIDB;Trusted_Connection=True;");
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

            //modelBuilder.Entity<UserMovie>(entity =>
            //{
            //    entity.HasKey(e => e.ID);

            //    modelBuilder.Entity<UserMovie>()
            //     .HasOne<User>(e => e.User)
            //     .WithMany(x => x.UserMovies)
            //     .HasForeignKey(e => e.UserID);

            //    modelBuilder.Entity<UserMovie>()
            //     .HasOne<Movie>(e => e.Movie)
            //     .WithMany(x => x.UserMovies)
            //     .HasForeignKey(e => e.ID);
            //});
        }
    }
}
