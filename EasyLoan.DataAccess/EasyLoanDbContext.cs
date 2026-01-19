using EasyLoan.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using EasyLoan.Models.Common.Enums;

namespace EasyLoan.DataAccess
{
    public class EasyLoanDbContext : DbContext
    {
        public EasyLoanDbContext(DbContextOptions<EasyLoanDbContext> options)
        : base(options) { }

        public DbSet<Customer> Customers {get; set;}
        public DbSet<Employee> Employees {get; set;}
        public DbSet<LoanType> LoanTypes { get; set; }
        public DbSet<LoanApplication> LoanApplications { get; set; }
        public DbSet<LoanDetails> Loans { get; set; }
        public DbSet<LoanPayment> LoanPayments { get; set; }
        public DbSet<LoanEmi> LoanEmis { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
           modelBuilder.ApplyConfigurationsFromAssembly(typeof(EasyLoanDbContext).Assembly);

           modelBuilder.Entity<Employee>().HasData(new Employee() 
           {    Id = Guid.NewGuid(),
                Email = "ankitkumarsingh018@gmail.com",
                Password = BCrypt.Net.BCrypt.HashPassword("ankit@Ankit@1"),
                PhoneNumber = "1234567890",
                Name = "Ankit",
                Role = EmployeeRole.Admin
           });
        }

    }
}
