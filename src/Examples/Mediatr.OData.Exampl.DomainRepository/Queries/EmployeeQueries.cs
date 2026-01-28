using Dapper;
using Mediatr.OData.Api.Abstractions.Interfaces;
using Mediatr.OData.Example.DomainModel.Company;
using Mediatr.OData.Example.DomainRepository.Helpers;
using System.Data;

namespace Mediatr.OData.Example.DomainRepository.Queries;

internal static class EmployeeQueries
{
    internal static async Task<IQueryable<Employee>> PopulateEmployees(IDbConnection connection, Guid key = default!, bool employeeOnly = false)
    {
        QueryBuilderHelper queryBuilderHelper = new();
        //Internal script processor
        if (key != default!)
            queryBuilderHelper.AddQuery(@"DECLARE @Id int = (SELECT [Id] FROM [ods].[Employee] WHERE [Key] =  @Key);");

        //Employees
        queryBuilderHelper.AddQuery(@"
            SELECT 
            e.*
            FROM [ods].[Employee] e
        ");
        if (key != default!) queryBuilderHelper.TryAddCondition(@"WHERE e.Id = @Id");
        if (!employeeOnly)
        {
            //Departments
            queryBuilderHelper.AddQuery(@"
                SELECT 
                d.*
                FROM [ods].[Department] d
                INNER JOIN [ods].[Employee] e
                ON d.Id = e.DepartmentId
            ");
            if (key != default!) queryBuilderHelper.TryAddCondition(@"WHERE e.Id = @Id");
            //Companies
            queryBuilderHelper.AddQuery(@"
                SELECT 
                c.*
                FROM [ods].[Company] c
                INNER JOIN [ods].[Department] d
                ON d.CompanyId = c.Id
                INNER JOIN [ods].[Employee] e
                ON d.Id = e.DepartmentId
            ");
            if (key != default!) queryBuilderHelper.TryAddCondition(@"WHERE e.Id = @Id");
        }
        var query = queryBuilderHelper.Build();

        try
        {
            IQueryable<Employee> employees;
            var result = await connection.QueryMultipleAsync(
                query,
                key == default! ? null : new { Key = key });


            employees = result.Read<Employee>().AsQueryable();
            if (!employeeOnly)
            {
                var departments = result.Read<Department>().AsQueryable();
                var companies = result.Read<Company>().AsQueryable();
                foreach (var employee in employees)
                {
                    employee.Department = departments.FirstOrDefault(d => d.Id == employee.DepartmentId);
                    employee.Department!.Company = companies.FirstOrDefault(c => c.Id == employee.Department!.CompanyId);
                }
            }
            return employees;
        }
        catch
        {
            return default!;
        }
    }
}
