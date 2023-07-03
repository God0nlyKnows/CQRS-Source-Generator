using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace CQRS_Source_Generator.Models
{
    internal struct InterfaceInfo
    {
        public ImmutableArray<ISymbol> Members;
        public string Name;
        public string Namespace;
    }
}
