using System;
using System.Collections.Generic;
using System.Linq;

namespace Adc.Odoo.Service.Infrastructure.Extensions
{
    public static class ExtensionsForType
    {
        public static bool IsGenericCollection(this Type referenceType)
        {
            if (!referenceType.IsGenericType)
            {
                return false;
            }

            var enumerableType = typeof(IEnumerable<>).MakeGenericType(referenceType.GetGenericArguments());
            var interfaces = referenceType.GetInterfaces();
            return interfaces.Contains(enumerableType);
        }

        public static bool IsNullable(this Type referenceType)
        {
            return referenceType.IsGenericType && referenceType.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
    }
}
