﻿using Mediatr.OData.Api.Abstractions.Interfaces;
using Mediatr.OData.Example.DomainModel.Company;
using Mediatr.OData.Example.DomainRepository.Interfaces;
using Mediatr.OData.Example.DomainRepository.Queries;
using System.Data;


namespace Mediatr.OData.Example.DomainRepository;

public class Repository(IDbConnection connection) : IRepository, IDisposable
{
    #region Connection
    public ConnectionState State => connection.State;

    private bool TryOpenConnection()
    {
        try
        {
            if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                connection.Open();

            return connection.State == ConnectionState.Open;
        }
        catch
        {
            return false;
        }
    }

    private bool TryCloseConnection()
    {
        try
        {
            if (connection.State != ConnectionState.Closed)
                connection.Close();
            return connection.State == ConnectionState.Closed;
        }
        catch
        {
            return false;
        }
    }
    #endregion

    public async Task<Department> DepartmentAsync(Guid key = default!, bool departmentOnly = false)
    {
        if (!TryOpenConnection())
            return default!;

        var departments = await DepartmentsAsync(key, departmentOnly);

        TryCloseConnection();
        return departments.FirstOrDefault() ?? default!;
    }

    public async Task<IQueryable<Department>> DepartmentsAsync(Guid key = default!, bool departmentOnly = false)
    {
        if (!TryOpenConnection())
            return Enumerable.Empty<Department>().AsQueryable();

        var departments = await DepartmentQueries.PopulateDepartments(connection, key, departmentOnly);

        TryCloseConnection();
        return departments;
    }

    public async Task<IQueryable<Employee>> DepartmentEmployeesAsync(Guid key = default!)
    {
        if (!TryOpenConnection())
            return Enumerable.Empty<Employee>().AsQueryable();
        var departments = await DepartmentQueries.PopulateDepartmentEmployees(connection, key);

        TryCloseConnection();
        return departments;
    }

    public async Task<Company> DepartmentCompanyAsync(Guid key = default!)
    {
        if (!TryOpenConnection())
            return default!;
        var company = await DepartmentQueries.PopulateDepartmentCompany(connection, key);

        TryCloseConnection();
        return company;
    }

    public async Task<IQueryable<IDomainObject>> DepartmentMembersAsync(Guid key = default!)
    {
        if (!TryOpenConnection())
            return Enumerable.Empty<IDomainObject>().AsQueryable();
        var members = await DepartmentQueries.PopulateDepartmentMembers(connection, key);

        TryCloseConnection();
        return members;
    }
    //public async Task<IQueryable<Employee>> EmployeesAsync(int Id = default!, bool employeeOnly = false)
    //{
    //    await Task.CompletedTask;
    //    if (!TryOpenConnection())
    //        return Enumerable.Empty<Employee>().AsQueryable();
    //    TryCloseConnection();
    //    return default!;
    //}

    //public async Task<IQueryable<Company>> CompaniesAsync(int Id = default!, bool companyOnly = false)
    //{
    //    await Task.CompletedTask;
    //    if (!TryOpenConnection())
    //        return Enumerable.Empty<Company>().AsQueryable();
    //    TryCloseConnection();
    //    return default!;
    //}

    #region Dispose pattern
    private bool disposedValue;
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                if (connection.State != ConnectionState.Closed)
                    connection.Close();
                connection.Dispose();
            }
            disposedValue = true;
        }
    }

    ~Repository()
    {
        Dispose(disposing: false);
    }

    void IDisposable.Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    #endregion
}
