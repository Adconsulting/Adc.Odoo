using System;

using Adc.Odoo.Service.Infrastructure.Enums;

namespace Adc.Odoo.Service.Infrastructure.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class OdooMapAttribute : System.Attribute
    {
        public string OdooName { get; private set; }

        public OdooType OdooType { get; private set; }

        public bool ReadOnly { get; private set; }


        public OdooMapAttribute(string name)
        {
            OdooName = name;
            OdooType = OdooType.Undefined;
            ReadOnly = false;
        }

        public OdooMapAttribute(string name, OdooType type)
            : this(name)
        {
            OdooType = type;
        }

        public OdooMapAttribute(string name, OdooType type, bool readOnly)
            : this(name, type)
        {
            ReadOnly = readOnly;
        }

        public OdooMapAttribute(OdooType type)
        {
            OdooName = string.Empty;
            OdooType = type;
            ReadOnly = false;
        }

    }
}
