using EasyLoan.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
           modelBuilder.ApplyConfigurationsFromAssembly(typeof(EasyLoanDbContext).Assembly);
        }

    }
}
