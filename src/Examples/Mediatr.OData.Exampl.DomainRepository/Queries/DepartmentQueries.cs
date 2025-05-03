using Dapper;
using Mediatr.OData.Example.DomainModel.Company;
using Mediatr.OData.Example.DomainRepository.Helpers;
using System.Data;

namespace Mediatr.OData.Example.DomainRepository.Queries;

internal static class DepartmentQueries
{
    internal static async Task<IQueryable<Department>> PopulateDepartments(IDbConnection connection, Guid key = default!, bool departmentOnly = false)
    {
        QueryBuilderHelper queryBuilderHelper = new();
        //Internal script processor
        if (key != default!)
            queryBuilderHelper.AddQuery(@"DECLARE @Id int = (SELECT [Id] FROM [ods].[Department] WHERE [Key] =  @Key);");

        //Departments
        queryBuilderHelper.AddQuery(@"
            SELECT 
            d.*
            FROM [ods].[Department] d
        ");
        if (key != default!) queryBuilderHelper.TryAddCondition(@"WHERE d.Id = @Id");
        if (!departmentOnly)
        {
            //Employees
            queryBuilderHelper.AddQuery(@"
                SELECT 
                e.*
                FROM [ods].[Employee] e
                INNER JOIN [ods].[Department] d
                ON d.Id = e.DepartmentId
            ");
            if (key != default!) queryBuilderHelper.TryAddCondition(@"WHERE d.Id = @Id");
            //Companies
            queryBuilderHelper.AddQuery(@"
                SELECT 
                c.*
                FROM [ods].[Company] c
                INNER JOIN [ods].[Department] d
                ON d.CompanyId = c.Id
            ");
            if (key != default!) queryBuilderHelper.TryAddCondition(@"WHERE d.Id = @Id");
        }
        var query = queryBuilderHelper.Build();

        try
        {
            IQueryable<Department> departments;
            var result = await connection.QueryMultipleAsync(
                query,
                key == default! ? null : new { Key = key });

            departments = result.Read<Department>().AsQueryable();
            if (!departmentOnly)
            {
                var employees = result.Read<Employee>().AsQueryable();
                var companies = result.Read<Company>().AsQueryable();
                foreach (var department in departments)
                {
                    department.Employees = [.. employees.Where(e => e.DepartmentId == department.Id)];
                    department.Company = companies.FirstOrDefault(c => c.Id == department.CompanyId);
                }
            }
            return departments;
        }
        catch
        {
            return default!;
        }
    }

    internal static async Task<Company> PopulateDepartmentCompany(IDbConnection connection, Guid key = default!)
    {
        QueryBuilderHelper queryBuilderHelper = new();
        //Internal script processor
        if (key != default!)
            queryBuilderHelper.AddQuery(@"DECLARE @Id int = (SELECT [Id] FROM [ods].[Department] WHERE [Key] =  @Key);");

        //Companies
        queryBuilderHelper.AddQuery(@"
                SELECT 
                c.*
                FROM [ods].[Company] c
                INNER JOIN [ods].[Department] d
                ON d.CompanyId = c.Id
            ");
        queryBuilderHelper.TryAddCondition(@"WHERE d.Id = @Id");

        var query = queryBuilderHelper.Build();

        try
        {
            Company company;
            var result = await connection.QueryMultipleAsync(
                query,
                key == default! ? null : new { Key = key });

            company = result.ReadFirst<Company>();

            return company;
        }
        catch
        {
            return default!;
        }
    }

    internal static async Task<IQueryable<Employee>> PopulateDepartmentEmployees(IDbConnection connection, Guid key = default!)
    {
        QueryBuilderHelper queryBuilderHelper = new();
        //Internal script processor
        if (key != default!)
            queryBuilderHelper.AddQuery(@"DECLARE @Id int = (SELECT [Id] FROM [ods].[Department] WHERE [Key] =  @Key);");

        //Employees
        queryBuilderHelper.AddQuery(@"
            SELECT 
            e.*
            FROM [ods].[Employee] e
            INNER JOIN [ods].[Department] d
            ON d.Id = e.DepartmentId
        ");
        queryBuilderHelper.TryAddCondition(@"WHERE d.Id = @Id");

        var query = queryBuilderHelper.Build();

        try
        {
            IQueryable<Employee> employees;
            var result = await connection.QueryMultipleAsync(
                query,
                key == default! ? null : new { Key = key });

            employees = result.Read<Employee>().AsQueryable();

            return employees;
        }
        catch
        {
            return default!;
        }
    }
}
