namespace Boxed.AspNetCore.Swagger.Test.OperationFilters
{
    using System.Collections.Generic;
    using System.Linq;
    using Boxed.AspNetCore.Swagger.OperationFilters;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Authorization.Infrastructure;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
    using Microsoft.AspNetCore.Mvc.Authorization;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Moq;
    using Swashbuckle.AspNetCore.Swagger;
    using Swashbuckle.AspNetCore.SwaggerGen;
    using Xunit;

    public class ClaimsOperationFilterTest
    {
        private readonly ApiDescription apiDescription;
        private readonly Operation operation;
        private readonly ClaimsOperationFilter operationFilter;
        private readonly OperationFilterContext operationFilterContext;

        public ClaimsOperationFilterTest()
        {
            this.apiDescription = new ApiDescription()
            {
                ActionDescriptor = new ActionDescriptor()
                {
                    FilterDescriptors = new List<FilterDescriptor>()
                }
            };
            this.operation = new Operation()
            {
                Responses = new Dictionary<string, Response>()
            };
            this.operationFilter = new ClaimsOperationFilter();
            this.operationFilterContext = new OperationFilterContext(
                this.apiDescription,
                new Mock<ISchemaRegistry>().Object,
                this.GetType().GetMethods().First());
        }

        [Fact]
        public void Apply_HasClaimsAuthorizationRequirements_AddsClaimsToOperation()
        {
            var requirement = new ClaimsAuthorizationRequirement("Type", new string[0]);
            var requirements = new List<IAuthorizationRequirement>() { requirement };
            var policy = new AuthorizationPolicy(requirements, new List<string>());
            var filterDescriptor = new FilterDescriptor(new AuthorizeFilter(policy), 30);
            this.operationFilterContext.ApiDescription.ActionDescriptor.FilterDescriptors.Add(filterDescriptor);

            this.operationFilter.Apply(this.operation, this.operationFilterContext);

            Assert.NotNull(this.operation.Security);
            Assert.Equal(1, this.operation.Security.Count);
            Assert.Equal(1, this.operation.Security.First().Count);
            Assert.Equal("oauth2", this.operation.Security.First().First().Key);
            Assert.Equal(new string[] { "Type" }, this.operation.Security.First().First().Value);
        }

        [Fact]
        public void Apply_HasPolicyWithNoClaimsAuthorizationRequirements_DoesNothing()
        {
            var requirement = new DenyAnonymousAuthorizationRequirement();
            var requirements = new List<IAuthorizationRequirement>() { requirement };
            var policy = new AuthorizationPolicy(requirements, new List<string>());
            var filterDescriptor = new FilterDescriptor(new AuthorizeFilter(policy), 30);
            this.operationFilterContext.ApiDescription.ActionDescriptor.FilterDescriptors.Add(filterDescriptor);

            this.operationFilter.Apply(this.operation, this.operationFilterContext);

            Assert.Null(this.operation.Security);
        }

        [Fact]
        public void Apply_DoesNotHaveClaimsAuthorizationRequirement_DoesNothing()
        {
            this.operationFilter.Apply(this.operation, this.operationFilterContext);

            Assert.Null(this.operation.Security);
        }
    }
}
