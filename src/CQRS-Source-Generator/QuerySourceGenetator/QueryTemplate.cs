using CQRS_Source_Generator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRS_Source_Generator.QuerySourceGenetator
{
    internal static class QueryTemplate
    {
        public static string GetQueryTemplate(MethodInfo methodInfo)
        {

            var str = $$""" 
                using MediatR;
                using System;
                using System.Collections.Generic;
                using System.Linq;
                using System.Text;
                using System.Threading.Tasks;

                namespace {{methodInfo.Namespace}};
                public class {{methodInfo.Name}}QueryHandler : IRequestHandler<{{methodInfo.RequestType}}, {{GetNestedReturnType(methodInfo.ReturnType)}}>
                {
                    private readonly {{methodInfo.ParentInterface}} _repository;

                    public {{methodInfo.Name}}QueryHandler({{methodInfo.ParentInterface}} repository)
                    {
                        _repository = repository;
                    }

                    public async Task<{{GetNestedReturnType(methodInfo.ReturnType)}}> Handle({{methodInfo.RequestType}} request, CancellationToken cancellationToken)
                    {
                        return{{(IsTask(methodInfo.ReturnType) ? " await" : "")}} _repository.{{methodInfo.Name}}({{(methodInfo.HasParameters ? string.Join(", ", methodInfo.OrderedRequestParameters.Select(p => $"request.{p}")) : "")}});
                        
                    }
                }
                
                """;

            return str;
        }

        static string GetNestedReturnType(string typeName)
        {
            if (IsTask(typeName))
            {
                int startIndex = typeName.IndexOf('<') + 1;
                int endIndex = typeName.LastIndexOf('>');
                string nestedTypeName = typeName.Substring(startIndex, endIndex - startIndex);
                return nestedTypeName;
            }
            return typeName;
        }

        static bool IsTask(string typeName)
        {
            if (typeName.Contains("Task<") || typeName.Contains("ValueTask<"))
            {
                return true;
            }

            return false;
        }
    }
}
