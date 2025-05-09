using Mediatr.OData.Api.Abstractions.Attributes;
using Mediatr.OData.Api.Abstractions.Enumerations;
using Mediatr.OData.Api.Abstractions.Interfaces;
using Mediatr.OData.Api.Abstractions.Models;
using Mediatr.OData.Api.Extensions;
using Mediatr.OData.Api.Factories;
using Mediatr.OData.Example.DomainModel.Company;
using Mediatr.OData.Example.DomainRepository.Interfaces;
using Microsoft.AspNetCore.OData.Deltas;
using System.Net;

namespace Mediatr.OData.Example.Api.EndpointHandlers
{
    [EndpointGroup("departments")]
    public class DepartmentHandler()
    {

        [Endpoint<Department, Guid>(EndpointMethod.Delete)]
        public class DeleteDepartment : IEndpointDeleteHandler<Department, Guid>
        {
            public async Task<IMediatrResult<dynamic>> Handle(Guid key, CancellationToken cancellationToken)
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
        public class GetDepartments(IRepository repository) : IEndpointGetHandler<Department>
        {
            public async Task<IMediatrResult<dynamic>> Handle(IODataQueryOptionsWithPageSize<Department> options, CancellationToken cancellationToken)
            {
                var result = await repository.DepartmentsAsync();
                return options.ApplyODataOptions(result);
            }
        }

        public class GetDepartments2(IRepository repository) : IEndpointGetHandler<Department>
        {
            public async Task<IMediatrResult<dynamic>> Handle(IODataQueryOptionsWithPageSize<Department> options, CancellationToken cancellationToken)
            {
                var result = await repository.DepartmentsAsync();
                return options.ApplyODataOptions(result);
            }
        }

        [Endpoint<Department, Guid>(EndpointMethod.Get)]
        public class GetDepartment(IRepository repository) : IEndpointGetByKeyHandler<Department, Guid>
        {
            public async Task<IMediatrResult<dynamic>> Handle(Guid key, IODataQueryOptionsWithPageSize<Department> options, CancellationToken cancellationToken)
            {
                var result = await repository.DepartmentAsync(key);
                return options.ApplyODataOptions(result);
            }
        }

        [Endpoint<Department, Guid>(EndpointMethod.Patch)]
        public class PatchDepartment : IEndpointPatchHandler<Department, Guid>
        {
            public async Task<IMediatrResult<dynamic>> Handle(Guid key, Delta<Department> domainObjectDelta, IODataQueryOptionsWithPageSize<Department> options, CancellationToken cancellationToken)
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
                //var x = domainObjectDelta.GetChangedPropertyNames();
                //var y = domainObjectDelta.GetInstance();
                //var Z = domainObjectDelta.GetDeltaNestedNavigationProperties();

                await Task.CompletedTask;
                var result = new Department();
                return options.ApplyODataOptions(result);
            }
        }

        [Endpoint<Department, Guid>(EndpointMethod.Put)]
        public class PutDepartment : IEndpointPutHandler<Department, Guid>
        {
            public async Task<IMediatrResult<dynamic>> Handle(Guid key, Delta<Department> domainObjectDelta, IODataQueryOptionsWithPageSize<Department> options, CancellationToken cancellationToken)
            {
                await Task.CompletedTask;
                var result = new Department();
                return options.ApplyODataOptions(result);
            }
        }

        [Endpoint<Department, Guid, Employee>(EndpointMethod.Get)]
        public class GetDepartmentEmployees(IRepository repository) : IEndpointGetByNavigationHandler<Department, Guid, Employee>
        {
            public async Task<IMediatrResult<dynamic>> Handle(Guid key, Type TDomainObject, IODataQueryOptionsWithPageSize<Employee> options, CancellationToken cancellationToken)
            {
                var result = await repository.DepartmentEmployeesAsync(key);
                return options.ApplyODataOptions(result);
            }
        }

        [Endpoint<Department, Guid, Company>(EndpointMethod.Get)]
        public class GetDepartmentCompany(IRepository repository) : IEndpointGetByNavigationHandler<Department, Guid, Company>
        {
            public async Task<IMediatrResult<dynamic>> Handle(Guid key, Type TDomainObject, IODataQueryOptionsWithPageSize<Company> options, CancellationToken cancellationToken)
            {
                var result = await repository.DepartmentCompanyAsync(key);
                return options.ApplyODataOptions(result);
            }
        }

        [Endpoint<Department, Guid, DomainObject>(EndpointMethod.Get)]
        public class GetDepartmentMembers(IRepository repository) : IEndpointGetByNavigationHandler<Department, Guid, DomainObject>
        {
            public async Task<IMediatrResult<dynamic>> Handle(Guid key, Type TDomainObject, IODataQueryOptionsWithPageSize<DomainObject> options, CancellationToken cancellationToken)
            {
                var result = await repository.DepartmentMembersAsync(key);
                return options.ApplyODataOptions(result.ToDomainObjects());
            }
        }
    }
}
