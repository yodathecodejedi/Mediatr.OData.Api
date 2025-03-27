using Mediatr.OData.Exampl.DomainRepository.Helpers;
using Mediatr.OData.Example.DomainModel.Company;
using System.Data;

namespace Mediatr.OData.Exampl.DomainRepository.Queries;

internal static class DepartmentQueries
{
    static IQueryable<Department> PopulateDepartments(IDbConnection connection, int id = default!, bool departmentOnly = false)
    {

        QueryBuilderHelper queryBuilderHelper = new QueryBuilderHelper();
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
        //Now do our multiple query stuff with Dapper


        return default!;
    }
}
