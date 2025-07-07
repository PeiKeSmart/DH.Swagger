using System.ComponentModel;
using System.Reflection;

using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace DH.Swagger;

/// <summary>
/// swagger默认值，只对swagger生效，避免直接使用[DefaultValue]影响正常业务
/// </summary>
public class SwaggerDefaultValueFilter : IOperationFilter
{
    public SwaggerDefaultValueFilter()
    {
        // 移除了 Newtonsoft.Json 依赖，不再需要 mvcJsonOptions
    }

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Parameters == null || !operation.Parameters.Any())
        {
            return;
        }

        var parameterValuePairs = context.ApiDescription.ParameterDescriptions
            .Where(parameter => GetDefaultValueAttribute(parameter) != null || (GetParameterInfo(parameter)?.HasDefaultValue ?? false))
            .ToDictionary(parameter => parameter.Name, GetDefaultValue);
        if (parameterValuePairs.Count == 0) return;
        foreach (var parameter in operation.Parameters)
        {
            if (parameterValuePairs.TryGetValue(parameter.Name, out var defaultValue))
            {
                parameter.Extensions.Add("default", new OpenApiString(defaultValue?.ToString()));
            }
        }
    }

    private SwaggerDefaultValueAttribute GetDefaultValueAttribute(ApiParameterDescription parameter)
    {
        if (!(parameter.ModelMetadata is DefaultModelMetadata metadata) || metadata.Attributes.PropertyAttributes == null)
        {
            return null;
        }

        return metadata.Attributes.PropertyAttributes
            .OfType<SwaggerDefaultValueAttribute>()
            .FirstOrDefault();
    }

    public ParameterInfo GetParameterInfo(ApiParameterDescription parameter)
    {
        if (parameter.ParameterDescriptor != null)
            return ((ControllerParameterDescriptor)parameter.ParameterDescriptor).ParameterInfo;
        return null;
    }

    private object GetDefaultValue(ApiParameterDescription parameter)
    {
        var parameterInfo = GetParameterInfo(parameter);

        if (parameterInfo?.HasDefaultValue == true)
        {
            if (parameter.Type.IsEnum)
            {
                // 对于枚举类型，直接返回默认值，不再依赖 Newtonsoft.Json 的 StringEnumConverter
                return parameterInfo.DefaultValue?.ToString();
            }

            return parameterInfo.DefaultValue;
        }

        var defaultValueAttribute = GetDefaultValueAttribute(parameter);
        return defaultValueAttribute?.Value;
    }
}

public class SwaggerJsonDefaultValueFilter : IOperationFilter
{
    public SwaggerJsonDefaultValueFilter()
    {
        // 移除了 Newtonsoft.Json 依赖
    }

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Parameters == null) return;
        var parameterValuePairs = context.ApiDescription.ParameterDescriptions
            .Where(parameter => GetDefaultValueAttribute(parameter) != null || GetParameterInfo(parameter).HasDefaultValue)
            .ToDictionary(parameter => parameter.Name, GetDefaultValue);

        foreach (var parameter in operation.Parameters)
        {
            if (parameterValuePairs.TryGetValue(parameter.Name, out var defaultValue))
            {
                parameter.Required = false;
                if (defaultValue != null)
                {
                    parameter.Extensions.Add("default", new OpenApiString(defaultValue.ToString()));
                }
            }
        }
    }

    private DefaultValueAttribute GetDefaultValueAttribute(ApiParameterDescription parameter)
    {
        return (parameter.ModelMetadata as DefaultModelMetadata)?
            .Attributes.PropertyAttributes?
            .OfType<DefaultValueAttribute>()
            .FirstOrDefault();
    }

    public ParameterInfo GetParameterInfo(ApiParameterDescription parameter)
    {
        return ((ControllerParameterDescriptor)parameter.ParameterDescriptor).ParameterInfo;
    }

    private object GetDefaultValue(ApiParameterDescription parameter)
    {
        var parameterInfo = GetParameterInfo(parameter);
        if (parameterInfo.HasDefaultValue)
        {
            return parameterInfo.DefaultValue;
        }
        else
        {
            return GetDefaultValueAttribute(parameter)?.Value;
        }
    }
}
