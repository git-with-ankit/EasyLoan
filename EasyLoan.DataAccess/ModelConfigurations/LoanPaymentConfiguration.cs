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
    internal class LoanPaymentConfiguration
    : IEntityTypeConfiguration<LoanPayment>
    {
        public void Configure(EntityTypeBuilder<LoanPayment> builder)
        {
            builder.Property(p => p.Status)
                   .HasConversion<string>();
        }
    }
}
