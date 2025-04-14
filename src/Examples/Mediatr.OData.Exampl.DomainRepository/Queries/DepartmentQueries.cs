using Dapper;
using Mediatr.OData.Example.DomainModel.Company;
using Mediatr.OData.Example.DomainRepository.Helpers;
using System.Data;

namespace Mediatr.OData.Example.DomainRepository.Queries;

internal static class DepartmentQueries
{
    //internal static async Task<IQueryable<Department>> PopulateDepartments(IDbConnection connection, int id = default!, bool departmentOnly = false)
    //{
    //    QueryBuilderHelper queryBuilderHelper = new();
    //    //Internal script processor
    //    if (id != default!)
    //        queryBuilderHelper.AddQuery(@"DECLARE @Key int = @Id;");

    //    //Departments
    //    queryBuilderHelper.AddQuery(@"
    //        SELECT 
    //        d.*
    //        FROM [ods].[Department] d
    //    ");
    //    //if (id != default!) queryBuilderHelper.TryAddCondition(@"WHERE d.Id = @Id");
    //    if (id != default!) queryBuilderHelper.TryAddCondition(@"WHERE d.Id = @Key");
    //    if (!departmentOnly)
    //    {
    //        //Employees
    //        queryBuilderHelper.AddQuery(@"
    //            SELECT 
    //            e.*
    //            FROM [ods].[Employee] e
    //            INNER JOIN [ods].[Department] d
    //            ON d.Id = e.DepartmentId
    //        ");
    //        //if (id != default!) queryBuilderHelper.TryAddCondition(@"WHERE d.Id = @Id");
    //        if (id != default!) queryBuilderHelper.TryAddCondition(@"WHERE d.Id = @Key");
    //        //Companies
    //        queryBuilderHelper.AddQuery(@"
    //            SELECT 
    //            c.*
    //            FROM [ods].[Company] c
    //            INNER JOIN [ods].[Department] d
    //            ON d.CompanyId = c.Id
    //        ");
    //        //if (id != default!) queryBuilderHelper.TryAddCondition(@"WHERE d.Id = @Id");
    //        if (id != default!) queryBuilderHelper.TryAddCondition(@"WHERE d.Id = @Key");
    //    }
    //    var query = queryBuilderHelper.Build();

    //    try
    //    {
    //        IQueryable<Department> departments;
    //        var result = await connection.QueryMultipleAsync(
    //            query,
    //            id == default! ? null : new { Id = id });

    //        departments = result.Read<Department>().AsQueryable();
    //        if (!departmentOnly)
    //        {
    //            var employees = result.Read<Employee>().AsQueryable();
    //            var companies = result.Read<Company>().AsQueryable();
    //            foreach (var department in departments)
    //            {
    //                department.Employees = [.. employees.Where(e => e.DepartmentId == department.Id)];
    //                department.Company = companies.FirstOrDefault(c => c.Id == department.CompanyId);
    //            }
    //        }
    //        return departments;
    //    }
    //    catch
    //    {
    //        return default!;
    //    }
    //}

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
}
