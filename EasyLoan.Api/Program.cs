
using EasyLoan.Api.Middleware;
using EasyLoan.Business.Interfaces;
using EasyLoan.Business.Services;
using EasyLoan.DataAccess;
using EasyLoan.DataAccess.Interfaces;
using EasyLoan.DataAccess.Repositories;
using EasyLoan.Dtos.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

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
            builder.Services.AddScoped<IJwtTokenGeneratorService, JwtTokenGeneratorService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddSingleton<IPublicIdService, PublicIdService>();

            builder.Services.PostConfigure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var problemDetails = new ValidationProblemDetails(context.ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = "Validation failed",
                        Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1",
                        Instance = context.HttpContext.Request.Path
                    };

                    return new BadRequestObjectResult(problemDetails)
                    {
                        ContentTypes = { "application/problem+json" }
                    };
                };
            });

            //builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            //.AddJwtBearer(options =>
            //{
            //    options.TokenValidationParameters = new TokenValidationParameters
            //    {
            //        ValidateIssuer = true,
            //        ValidateAudience = true,
            //        ValidateLifetime = true,
            //        ValidateIssuerSigningKey = true,

            //        ValidIssuer = builder.Configuration["Jwt:Issuer"],
            //        ValidAudience = builder.Configuration["Jwt:Audience"],
            //        IssuerSigningKey = new SymmetricSecurityKey(
            //            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            //        ),

            //        ClockSkew = TimeSpan.Zero
            //    };
            //});

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opt =>
            {
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
                    ClockSkew = TimeSpan.Zero
                };
                opt.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        // stop the default behaviour
                        context.HandleResponse();

                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/problem+json";

                        var problem = new ProblemDetails
                        {
                            Status = StatusCodes.Status401Unauthorized,
                            Title = "Authentication failed",
                            Type = "https://datatracker.ietf.org/doc/html/rfc7235#section-3.1",
                            Detail = "The access token is missing, invalid, or expired.",
                            Instance = context.Request.Path
                        };

                        await context.Response.WriteAsJsonAsync(problem);
                    },

                    OnForbidden = async context =>
                    {
                        // This will trigger when user is logged in but doesn't have access (role etc.)
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/problem+json";

                        var problem = new ProblemDetails
                        {
                            Status = StatusCodes.Status403Forbidden,
                            Title = "Access denied",
                            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.3",
                            Detail = "You do not have permission to perform this action.",
                            Instance = context.Request.Path
                        };


                        await context.Response.WriteAsJsonAsync(problem);
                    }
                };
            });

            builder.Services.AddAuthorization();

            builder.Services.AddControllers()
             .AddJsonOptions(options =>
             {
                 options.JsonSerializerOptions.Converters.Add(
                     new JsonStringEnumConverter()
                 );
             });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(opt =>
            {
                // JWT Swagger Support
                opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using Bearer scheme. Example: \"{token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer"
                });

                opt.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                    Array.Empty<string>()
                }
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseMiddleware<GlobalExceptionMiddleware>();


            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
