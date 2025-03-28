using Dapper;
using Mediatr.OData.Exampl.DomainRepository.Helpers;
using Mediatr.OData.Example.DomainModel.Company;
using System.Data;

namespace Mediatr.OData.Exampl.DomainRepository.Queries;

internal static class DepartmentQueries
{
    internal static async Task<IQueryable<Department>> PopulateDepartments(IDbConnection connection, int id = default!, bool departmentOnly = false)
    {

        QueryBuilderHelper queryBuilderHelper = new();
        //Departments
        queryBuilderHelper.AddQuery(@"
            SELECT 
            d.*
            FROM [ods].[Department] d
        ");
        if (id != default!) queryBuilderHelper.AddQuery(@"WHERE d.Id = @Id");
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
            if (id != default!) queryBuilderHelper.AddQuery(@"WHERE d.Id = @Id");
            //Companies
            queryBuilderHelper.AddQuery(@"
                SELECT 
                c.*
                FROM [ods].[Company] c
                INNER JOIN [ods].[Department] d
                ON d.CompanyId = c.Id
            ");
            if (id != default!) queryBuilderHelper.AddQuery(@"WHERE d.Id = @Id");
        }
        var query = queryBuilderHelper.Build();

        try
        {
            IQueryable<Department> departments;
            var result = await connection.QueryMultipleAsync(
                query,
                id == default! ? null : new { Id = id });

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
}
