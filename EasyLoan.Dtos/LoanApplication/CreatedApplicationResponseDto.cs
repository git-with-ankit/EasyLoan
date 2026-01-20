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
    internal class CreatedApplicationResponseDto
    {
        public string ApplicationNumber { get; set; }

        [Required]
        public Guid CustomerId { get; set; }

        [Required]
        public Guid LoanTypeId { get; set; }

        [Required]
        public Guid AssignedEmployeeId { get; set; }

        [Required]
        [Precision(18, 2)]
        public decimal RequestedAmount { get; set; }

        [Required]
        [Precision(18, 2)]
        public decimal ApprovedAmount { get; set; }

        [Required]
        public int RequestedTenureInMonths { get; set; }

        [Required]
        public LoanApplicationStatus Status { get; set; }

        [MaxLength(1000)]
        public string? ManagerComments { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
