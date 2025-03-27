﻿using Mediatr.OData.Exampl.DomainRepository.Interfaces;
using Mediatr.OData.Example.DomainModel.Company;
using System.Data;


namespace Mediatr.OData.Exampl.DomainRepository;

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

    public async Task<IQueryable<Department>> DepartmentsAsync(int Id = default!, bool departmentOnly = false)
    {
        await Task.CompletedTask;
        if (!TryOpenConnection())
            return Enumerable.Empty<Department>().AsQueryable();


        TryCloseConnection();
        return default!;
    }

    public async Task<IQueryable<Employee>> EmployeesAsync(int Id = default!, bool employeeOnly = false)
    {
        await Task.CompletedTask;
        if (!TryOpenConnection())
            return Enumerable.Empty<Employee>().AsQueryable();
        TryCloseConnection();
        return default!;
    }

    public async Task<IQueryable<Company>> CompaniesAsync(int Id = default!, bool companyOnly = false)
    {
        await Task.CompletedTask;
        if (!TryOpenConnection())
            return Enumerable.Empty<Company>().AsQueryable();
        TryCloseConnection();
        return default!;
    }

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
