using System.Runtime.InteropServices.ComTypes;

using Adc.Odoo.Service.Infrastructure.Attributes;
using Adc.Odoo.Service.Infrastructure.Enums;
using Adc.Odoo.Service.Infrastructure.Interfaces;

namespace Adc.Odoo.Tests.Models
{

    [OdooMap("res.partner")]
    class OdooPartner : IOdooObject
    {
        [OdooMap("id")]
        public int Id { get; set; }

        [OdooMap("name")]
        public string Name { get; set; }

        [OdooMap("is_company", OdooType.Boolean)]
        public bool IsCompany { get; set; }

        [OdooMap("parent_id", OdooType.Many2One)]
        public int? ParentId { get; set; }

        [OdooMap("email")]
        public string Email { get; set; }

        [OdooForeignKey("parent_id")]
        public OdooPartner Parent { get; set; }
    }
}
