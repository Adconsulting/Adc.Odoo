using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Adc.Odoo.Service.Infrastructure.Attributes;
using Adc.Odoo.Service.Infrastructure.Interfaces;

namespace Adc.Odoo.Tests.Models
{
    [OdooMap("crm.lead")]
    class OdooLead : IOdooObject
    {
        [OdooMap("id")]
        public int Id { get; set; }

        [OdooMap("name")]
        public string Name { get; set; }
    }
}
