using Contracts.Interface;
using Entities.LinkModels;
using Entities.Model;
using Microsoft.Net.Http.Headers;
using Shared.DataTransferObjects;

namespace CompanyEmplooyes.Utility
{
    public class EmployeeLinks : IEmployeeLinks
   {
        private readonly LinkGenerator _linkGenerator;
        private readonly IDataShaper<EmployeeDto> _dataShaper;

        public EmployeeLinks(LinkGenerator linkGenerator, IDataShaper<EmployeeDto> dataShaper) => (_linkGenerator, _dataShaper) = (linkGenerator, dataShaper);

        public LinkResponse TryGenerateLinks(IEnumerable<EmployeeDto> employees, string fields, Guid companyId, HttpContext httpContext)
        {
            var shapedEmployees = ShapeData(employees, fields);

            if (!ShouldGenerateLinks(httpContext)) 
                return ReturnLinkedEmployees(employees, fields, companyId, httpContext, shapedEmployees);

            return ReturnShapedEmployees(shapedEmployees);
        }

        private List<Entity> ShapeData(IEnumerable<EmployeeDto> employees, string fields)=>         
            _dataShaper.ShapeData(employees, fields)
                .Select(e => e.Entity)
                .ToList();
        private bool ShouldGenerateLinks(HttpContext httpContext)
        {
             var mediaType = (MediaTypeHeaderValue)httpContext.Items["AcceptHeaderMediaType"];
             return mediaType.SubTypeWithoutSuffix.EndsWith("hateoas", StringComparison.InvariantCultureIgnoreCase);
        }

        private LinkResponse ReturnShapedEmployees(List<Entity> shapedEmployees) => new LinkResponse {ShapedEntities = shapedEmployees};

        private LinkResponse ReturnLinkedEmployees(IEnumerable<EmployeeDto> employeeDto,string fields, Guid companyId, HttpContext httpContext, List<Entity> shapedEmployees)
        {
            var employeeDtoList = employeeDto.ToList();
            for(var index = 0; index < employeeDtoList.Count; index++)
            {
                var employeeLinks = CreateLinksForEmployee(httpContext, companyId, employeeDtoList[index].Id, fields);
                shapedEmployees[index].Add("Links", employeeLinks);
            }

            var employeeCollection = new LinkCollectionWrapper<Entity>(shapedEmployees);
            var linkedEmployees = CreateLinksForEmployees(httpContext,employeeCollection);

            return new LinkResponse {HasLinks = true, LinkedEntities = linkedEmployees};
        }

        private List<Link> CreateLinksForEmployee(HttpContext httpContext, Guid CompanyId, Guid id, string fields ="")
        {
            var links = new List<Link>
            {
                new Link(_linkGenerator.GetUriByAction(httpContext, "GetEmployeeForCompany", values: new {CompanyId, id, fields}), "self", "GET"),
                new Link(_linkGenerator.GetUriByAction(httpContext, "DeleteEmployeeForCompany", values: new {CompanyId, id}), "delete_employee", "DELETE"),
                new Link(_linkGenerator.GetUriByAction(httpContext, "CreateEmployeeForCompany", values: new {CompanyId}), "create_employee_for_company", "POST"),
                new Link(_linkGenerator.GetUriByAction(httpContext, "UpdateEmployeeForCompany", values: new {CompanyId, id}), "update_employee_for_company", "PUT"),
                new Link(_linkGenerator.GetUriByAction(httpContext, "PartiallyUpdateEmployeeForCompany", values: new {CompanyId, id}), "partially_update_employee_for_company", "PATCH")
            };
            return links;
        }
        
        private LinkCollectionWrapper<Entity> CreateLinksForEmployees(HttpContext httpContext, LinkCollectionWrapper<Entity> employeesWrapper)
        {
            employeesWrapper.Links.Add(new Link(_linkGenerator.GetUriByAction(httpContext, "GetEmployeesForCompany", values: new { }), "self", "GET"));
            return employeesWrapper;
        }
        
        
    }
}
