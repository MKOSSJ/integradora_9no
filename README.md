** cargar el proyecto
dotnet restore
dotnet build

** cargar las herramientas
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Pomelo.EntityFrameworkCore.MySql
dotnet tool install --global dotnet-ef

** cargar las migraciones
dotnet ef database update
