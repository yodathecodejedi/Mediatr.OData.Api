﻿using Mediatr.OData.Api.Abstractions.Interfaces;
using Mediatr.OData.Example.DomainModel.Company;
using System.Data;

namespace Mediatr.OData.Example.DomainRepository.Interfaces;

public interface IRepository
{
    public ConnectionState State { get; }

    public Task<Department> DepartmentAsync(Guid key = default!, bool departmentOnly = false);

    public Task<IQueryable<Department>> DepartmentsAsync(Guid Key = default!, bool departmentOnly = false);

    public Task<IQueryable<Employee>> DepartmentEmployeesAsync(Guid key = default!);

    public Task<Company> DepartmentCompanyAsync(Guid key = default!);

    public Task<IQueryable<IDomainObject>> DepartmentMembersAsync(Guid key = default!);

    //public Task<IQueryable<Employee>> EmployeesAsync(int Id = default!, bool employeeOnly = false);

    //public Task<IQueryable<Company>> CompaniesAsync(int Id = default!, bool companyOnly = false);
}
