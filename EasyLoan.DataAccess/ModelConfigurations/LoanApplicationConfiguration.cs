using EasyLoan.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.DataAccess.ModelConfigurations
{
    internal class LoanApplicationConfiguration
    : IEntityTypeConfiguration<LoanApplication>
    {
        public void Configure(EntityTypeBuilder<LoanApplication> builder)
        {
            builder.Property(l => l.Status)
                   .HasConversion<string>();
        }
    }

}
