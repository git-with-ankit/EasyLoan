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
    internal class LoanDetailsConfiguration
    : IEntityTypeConfiguration<LoanDetails>
    {
        public void Configure(EntityTypeBuilder<LoanDetails> builder)
        {
            builder.Property(l => l.Status)
                   .HasConversion<string>();
        }
    }
}
