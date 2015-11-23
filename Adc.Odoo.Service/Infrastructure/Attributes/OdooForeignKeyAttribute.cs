using System;

namespace Adc.Odoo.Service.Infrastructure.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class OdooForeignKeyAttribute : Attribute
    {
        public String PropertyName { get; set; }

        public OdooForeignKeyAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }
    }
}
