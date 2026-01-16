using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyLoan.DataAccess.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EasyLoan.DataAccess.ModelConfigurations
{
    internal class EmployeeConfiguration
    {
        public void Configure(EntityTypeBuilder<Employee> builder)
        {
            builder.Property(e => e.Role)
                .HasConversion<string>();
        }
    }
}