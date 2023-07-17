using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace CQRS_Source_Generator.Models
{
    internal struct MethodInfo
    {
        public string Name;
        public string Namespace;
        public string ParentInterface;
        public string ReturnType;
        public string RequestType;
        public bool IsAsync;
        public bool HasParameters;
        public ImmutableArray<string> OrderedRequestParameters;
    }
}
