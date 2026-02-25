# Stage 1 — Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution + project files first (better caching)
COPY EasyLoan.sln .
COPY EasyLoan.Api/EasyLoan.Api.csproj EasyLoan.Api/
COPY EasyLoan.Business/EasyLoan.Business.csproj EasyLoan.Business/
COPY EasyLoan.DataAccess/EasyLoan.DataAccess.csproj EasyLoan.DataAccess/
COPY EasyLoan.Dtos/EasyLoan.Dtos.csproj EasyLoan.Dtos/
COPY EasyLoan.Models.Common/EasyLoan.Models.Common.csproj EasyLoan.Models.Common/

RUN dotnet restore EasyLoan.Api/EasyLoan.Api.csproj

# Copy full source
COPY . .

WORKDIR /src/EasyLoan.Api
RUN dotnet publish -c Release -o /app/publish

# Stage 2 — Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

ENTRYPOINT ["dotnet", "EasyLoan.Api.dll"]