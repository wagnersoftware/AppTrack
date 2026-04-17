using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace AppTrack.Api.Swagger;

public class NullableEnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.MemberInfo is not PropertyInfo propertyInfo)
            return;

        var type = propertyInfo.PropertyType;
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)
            && Nullable.GetUnderlyingType(type)!.IsEnum)
        {
            schema.Nullable = true;
        }
    }
}
