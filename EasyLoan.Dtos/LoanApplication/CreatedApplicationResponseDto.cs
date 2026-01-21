using EasyLoan.Models.Common.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.Dtos.LoanApplication
{
    public class CreatedApplicationResponseDto
    {
        public string ApplicationNumber { get; set; }

        public LoanApplicationStatus Status { get; set; }

        public DateTime CreatedDate { get; set; } 
    }
}
