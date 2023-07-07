using System;
using System.Collections.Generic;
using System.Text;

namespace CQRS_Source_Generator.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class GenerateQuery : Attribute
    {
    }
}
