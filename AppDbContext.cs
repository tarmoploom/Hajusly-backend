using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Entity.Infrastructure;
using Hajusly.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Validations.Rules;

namespace Hajusly
{    public class AppDbContext : DbContext
    {

        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Course>? Courses { get; set; }
        public DbSet<Module>? Modules { get; set; }
        public DbSet<Student>? Students { get; set; }
        public DbSet<User>? Users { get; set; }
        public DbSet<PrivateMessage>? PrivateMessages { get; set; }
        public DbSet<StudentInCourse>? StudentInCourse { get; set; }
        public DbSet<StudentPoints>? StudentPoints { get; set; }
        public DbSet<Tag>? Tags { get; set; }
        public DbSet<TagInStudent>? TagInStudentSet { get; set; }

        protected override void OnModelCreating(ModelBuilder mb)
        {
            base.OnModelCreating(mb);
            mb.Entity<User>().Property(x => x.Id).HasIdentityOptions(3);

            mb.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "Admin",
                    Password = "St9tpNN2zrinRGNUgKWCy4JjZRFEorSQ0Zg3a/8m7k4=", //test1
                    name = "Admini Straator",
                    emailAddress = "startup@hajusly.io",
                    isActive = true
                }, 
                new User
                {
                    Id = 2,
                    Username = "kasutaja.nimi",
                    Password = "St9tpNN2zrinRGNUgKWCy4JjZRFEorSQ0Zg3a/8m7k4=", //test1
                    name = "Hajusly Kasutaja",
                    emailAddress = "tuulehaug@kalamaja.com",
                    isActive = true
                   
                }
                );
            
            mb.Entity<Tag>().Property(x => x.Id).HasIdentityOptions(2);
            mb.Entity<Tag>().HasData(
                new Tag
                {
                    Id = 1,
                    Name = "blu tag",
                    Color = "#4169e1",
                    TeacherId = 1
                }
            );
            
            mb.Entity<Student>().Property(x => x.Id).HasIdentityOptions(5);
            mb.Entity<Student>().HasData(
                new Student
                {
                    Id = 1,
                    firstName = "Kati",
                    lastName = "Karu",
                    email = "kati.pati@gmail.com",
                    studentCode = "22451TAF",
                    isActive = true,
                    guid = Guid.NewGuid(),
                    teacherId = 1
                },
                new Student
                {
                    Id = 2,
                    firstName = "Jaan",
                    lastName = "Jalgratas",
                    email = "jalgratas@ttu.ee",
                    studentCode = "22111SAH",
                    isActive = true,
                    guid = Guid.NewGuid(),
                    teacherId = 1
                },
                new Student
                {
                    Id = 3,
                    firstName = "Zoja",
                    lastName = "Puhur",
                    email = "zojapuhur@mail.ee",
                    studentCode = "21217BAH",
                    isActive = true,
                    guid = Guid.NewGuid(),
                    teacherId = 2
                },
                new Student
                {
                    Id = 4,
                    firstName = "Aadu",
                    lastName = "Kuult",
                    email = "Aadu.Kuult@mail.ee",
                    studentCode = "113322SS",
                    isActive = true,
                    guid = Guid.NewGuid(),
                    teacherId = 1
                }
            );
            
            mb.Entity<TagInStudent>().Property(x => x.Id).HasIdentityOptions(2);
            mb.Entity<TagInStudent>().HasData(
                new TagInStudent
                {
                    Id = 1,
                    StudentId = 1,
                    TagId = 1,
                    CourseId = 1
                }
            );

            mb.Entity<StudentInCourse>().Property(x => x.Id).HasIdentityOptions(5);
            mb.Entity<StudentInCourse>().HasData(
                new StudentInCourse
                {
                    Id = 1,
                    StudentId = 1,
                    CourseId = 1
                },
                new StudentInCourse
                {
                    Id = 2,
                    StudentId = 2,
                    CourseId = 1
                },
                new StudentInCourse
                {
                    Id = 3,
                    StudentId = 3,
                    CourseId = 1
                },
                new StudentInCourse
                {
                    Id = 4,
                    StudentId = 4,
                    CourseId = 2
                }
            );
            
            mb.Entity<Course>().ToTable("Courses").HasKey(x => x.Id);

            mb.Entity<Course>().Property(x => x.Id).HasIdentityOptions(1);
            mb.Entity<Course>().HasData(
                new Course
                {
                    Id = 1,
                    Name = "Distributed systems",
                    Code = "ABC123",
                    CourseStart = DateTime.Parse("2022-10-01T00:00:00Z").ToUniversalTime(),
                    CourseEnd = DateTime.Parse("2022-12-31T00:00:00Z").ToUniversalTime(),
                    TeacherId = 1,
                },
                new Course
                {
                    Id = 2,
                    Name = "Linear Algebra",
                    Code = "CBA123",
                    CourseStart = DateTime.Parse("2022-11-01T00:00:00Z").ToUniversalTime(),
                    CourseEnd = DateTime.Parse("2023-01-31T00:00:00Z").ToUniversalTime(),
                    TeacherId = 1
                }
            );
            
            mb.Entity<Module>().ToTable("Modules").HasKey(x => x.Id);
            mb.Entity<Module>().Property(x => x.Id).HasIdentityOptions(7);
                
            mb.Entity<Module>().HasData(
                new Module
                {
                    Id = 1,
                    Name = "Back-end",
                    Abbreviation = "BE",
                    CourseId = 1,
                    MaxPoints = 10,
                    PassingPercent = 51,
                    Deadline = DateTime.Parse("2022-10-31T00:00:00Z").ToUniversalTime(),
                    ParentModuleId = null,
                },
                new Module
                {
                    Id = 2,
                    Name = "Installing BE tools",
                    Abbreviation = "1.1",
                    CourseId = 1,
                    ParentModuleId = 1,
                    MaxPoints = 2,
                    Deadline = DateTime.Parse("2022-10-10T00:00:00Z").ToUniversalTime()
                },
                new Module
                {
                    Id = 3,
                    Name = "Hello world",
                    Abbreviation = "1.2",
                    CourseId = 1,
                    ParentModuleId = 1,
                    MaxPoints = 1,
                    Deadline = DateTime.Parse("2022-10-31T00:00:00Z").ToUniversalTime()
                },
                new Module
                {
                    Id = 4,
                    Name = "Front-end",
                    Abbreviation = "FE",
                    CourseId = 1,
                    MaxPoints = 10,
                    PassingPercent = 51,
                    Deadline = DateTime.Parse("2022-11-30T00:00:00Z").ToUniversalTime(),
                    ParentModuleId = null,
                },
                new Module
                {
                    Id = 5,
                    Name = "Installing FE tools",
                    Abbreviation = "2.1",
                    CourseId = 1,
                    MaxPoints = 2,                    
                    ParentModuleId = 4,
                    Deadline = DateTime.Parse("2022-11-30T00:00:00Z").ToUniversalTime()
                },
                new Module
                {
                    Id = 6,
                    Name = "course 2 module 1",
                    Abbreviation = "c1.m1",
                    CourseId = 2,
                    MaxPoints = 22,                    
                    ParentModuleId = null,
                    PassingPercent = 51,
                    Deadline = DateTime.Parse("2022-11-30T00:00:00Z").ToUniversalTime()
                }
            );

            mb.Entity<StudentPoints>().ToTable("StudentPoints").HasKey(x => x.Id);
            mb.Entity<StudentPoints>().Property(x => x.Id).HasIdentityOptions(6);
            mb.Entity<StudentPoints>().HasData(
                new StudentPoints
                {
                    Id = 1, StudentId = 1, ModuleId = 2, Points = 2,
                    Credited = DateTime.Parse("2022-10-31T00:00:00Z").ToUniversalTime(), ActiveCredit = true },
                new StudentPoints
                {
                    Id = 2, StudentId = 1, ModuleId = 3, Points = 1,
                    Credited = DateTime.Parse("2022-10-29T00:00:00Z").ToUniversalTime(), ActiveCredit = true },
                new StudentPoints
                {
                    Id = 3, StudentId = 1, ModuleId = 5, Points = 1.5m,
                    Credited = DateTime.Parse("2022-10-29T00:00:00Z").ToUniversalTime(), ActiveCredit = true },
                new StudentPoints
                {
                    Id = 4, StudentId = 1, ModuleId = 2, Points = 0.5m,
                    Credited = DateTime.Parse("2022-10-29T00:00:00Z").ToUniversalTime(), ActiveCredit = false },
                new StudentPoints
                {
                    Id = 5, StudentId = 2, ModuleId = 2, Points = 1,
                    Credited = DateTime.Parse("2022-10-29T00:00:00Z").ToUniversalTime(), ActiveCredit = true }
            );

            mb.UseIdentityColumns();
        }
    }
}