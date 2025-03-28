using Mediatr.OData.Api.Abstractions.Attributes;
using Mediatr.OData.Api.Abstractions.Enumerations;
using Mediatr.OData.Api.Abstractions.Interfaces;
using Mediatr.OData.Api.Factories;
using Mediatr.OData.Exampl.DomainRepository.Interfaces;
using Mediatr.OData.Example.DomainModel.Company;
using Microsoft.AspNetCore.OData.Deltas;
using System.Net;

namespace Mediatr.OData.Example.Api.EndpointHandlers
{
    [EndpointGroup("departments")]
    public class DepartmentHandler(IRepository repository)
    {
        [Endpoint<Department, int>(EndpointMethod.Delete)]
        public class DeleteDepartment : IEndpointDeleteHandler<Department, int>
        {
            public async Task<IMediatrResult<dynamic>> Handle(int key, CancellationToken cancellationToken)
            {
                try
                {
                    return await MediatrResultFactory.CreateSuccess(default!, HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    return await MediatrResultFactory.CreateProblem(HttpStatusCode.BadRequest, ex.Message, ex);
                }
            }
        }

        [Endpoint<Department>(EndpointMethod.Get)]
        public class GetDepartments : IEndpointGetHandler<Department>
        {
            public async Task<IMediatrResult<dynamic>> Handle(IODataQueryOptionsWithPageSize<Department> options, CancellationToken cancellationToken)
            {
                await Task.CompletedTask;
                var result = Enumerable.Empty<Department>().AsQueryable();
                return options.ApplyODataOptions(result);
            }
        }

        [Endpoint<Department, int>(EndpointMethod.Get)]
        public class GetDepartment : IEndpointGetByKeyHandler<Department, int>
        {
            public async Task<IMediatrResult<dynamic>> Handle(int key, IODataQueryOptionsWithPageSize<Department> options, CancellationToken cancellationToken)
            {
                await Task.CompletedTask;
                var result = Enumerable.Empty<Department>().AsQueryable();
                return options.ApplyODataOptions(result);
            }
        }

        [Endpoint<Department, int>(EndpointMethod.Patch)]
        public class PatchDepartment : IEndpointPatchHandler<Department, int>
        {
            public async Task<IMediatrResult<dynamic>> Handle(int key, Delta<Department> domainObjectDelta, IODataQueryOptionsWithPageSize<Department> options, CancellationToken cancellationToken)
            {
                await Task.CompletedTask;
                var result = new Department();
                return options.ApplyODataOptions(result);
            }
        }

        [Endpoint<Department>(EndpointMethod.Post)]
        public class PostDepartment : IEndpointPostHandler<Department>
        {
            public async Task<IMediatrResult<dynamic>> Handle(Delta<Department> domainObjectDelta, IODataQueryOptionsWithPageSize<Department> options, CancellationToken cancellationToken)
            {
                await Task.CompletedTask;
                var result = new Department();
                return options.ApplyODataOptions(result);
            }
        }

        [Endpoint<Department, int>(EndpointMethod.Put)]
        public class PutDepartment : IEndpointPutHandler<Department, int>
        {
            public async Task<IMediatrResult<dynamic>> Handle(int key, Delta<Department> domainObjectDelta, IODataQueryOptionsWithPageSize<Department> options, CancellationToken cancellationToken)
            {
                await Task.CompletedTask;
                var result = new Department();
                return options.ApplyODataOptions(result);
            }
        }

        [Endpoint<Department, int, Employee>(EndpointMethod.Get, "employees")]
        public class GetDepartmentEmployees : IEndpointGetByNavigationHandler<Department, int, Employee>
        {
            public async Task<IMediatrResult<dynamic>> Handle(int key, Type TDomainObject, IODataQueryOptionsWithPageSize<Employee> options, CancellationToken cancellationToken)
            {
                await Task.CompletedTask;
                var result = Enumerable.Empty<Employee>().AsQueryable();
                return options.ApplyODataOptions(result);
            }
        }

        [Endpoint<Department, int, Company>(EndpointMethod.Get, "company")]
        public class GetDepartmentCompany : IEndpointGetByNavigationHandler<Department, int, Company>
        {
            public async Task<IMediatrResult<dynamic>> Handle(int key, Type TDomainObject, IODataQueryOptionsWithPageSize<Company> options, CancellationToken cancellationToken)
            {
                await Task.CompletedTask;
                var result = Enumerable.Empty<Employee>().AsQueryable();
                return options.ApplyODataOptions(result);
            }
        }
    }

}
