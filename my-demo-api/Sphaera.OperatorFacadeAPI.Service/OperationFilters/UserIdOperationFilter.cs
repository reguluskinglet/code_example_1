using System.Collections.Generic;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace demo.DemoApi.Service.OperationFilters
{
    /// <summary>
    /// Возможность указать пользователя в Swagger
    /// </summary>
    public class UserIdOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
                operation.Parameters = new List<OpenApiParameter>();

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = Authorization.HeaderNames.UserId,
                In = ParameterLocation.Header,
                Description = "User Id",
                Required = false,
                Schema = new OpenApiSchema
                {
                    Type = "String",
                    Default = new OpenApiString(string.Empty)
                }
            });
        }
    }
}
