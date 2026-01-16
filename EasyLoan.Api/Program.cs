
using EasyLoan.Business.Interfaces;
using EasyLoan.Business.Services;
using EasyLoan.DataAccess;
using EasyLoan.DataAccess.Interfaces;
using EasyLoan.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EasyLoan.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDbContextPool<EasyLoanDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("EasyLoanDBConnection"))
            );
            // Add services to the container.

            builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
            builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            builder.Services.AddScoped<ILoanApplicationRepository, LoanApplicationRepository>();
            builder.Services.AddScoped<ILoanDetailsRepository, LoanDetailsRepository>();
            builder.Services.AddScoped<ILoanPaymentRepository, LoanPaymentRepository>();
            builder.Services.AddScoped<ILoanTypeRepository, LoanTypeRepository>();

            builder.Services.AddScoped<ICustomerService, CustomerService>();
            builder.Services.AddScoped<IEmployeeService, EmployeeService>();
            builder.Services.AddScoped<ILoanApplicationService, LoanApplicationService>();
            builder.Services.AddScoped<ILoanPaymentService, LoanPaymentService>();
            builder.Services.AddScoped<ILoanService, LoanService>();
            builder.Services.AddScoped<ILoanTypeService, LoanTypeService>();

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
