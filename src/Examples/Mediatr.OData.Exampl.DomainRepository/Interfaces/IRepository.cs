using Mediatr.OData.Example.DomainModel.Company;
using System.Data;

namespace Mediatr.OData.Exampl.DomainRepository.Interfaces;

public interface IRepository
{
    public ConnectionState State { get; }

    public Task<IQueryable<Department>> DepartmentsAsync(int Id = default!, bool departmentOnly = false);

    public Task<IQueryable<Employee>> EmployeesAsync(int Id = default!, bool employeeOnly = false);

    public Task<IQueryable<Company>> CompaniesAsync(int Id = default!, bool companyOnly = false);
}
