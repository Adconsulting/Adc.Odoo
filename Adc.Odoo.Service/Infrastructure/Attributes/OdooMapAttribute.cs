using System;

using Adc.Odoo.Service.Infrastructure.Enums;

namespace Adc.Odoo.Service.Infrastructure.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class OdooMapAttribute : System.Attribute
    {
        public string OdooName { get; set; }

        public OdooType OdooType { get; set; }

        public OdooMapAttribute(string name)
        {
            OdooName = name;
            OdooType = OdooType.Undefined;
        }

        public OdooMapAttribute(string name, OdooType type)
            : this(name)
        {
            OdooType = type;
        }

        public OdooMapAttribute(OdooType type)
        {
            OdooName = string.Empty;
            OdooType = type;
        }

    }
}
