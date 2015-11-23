using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Adc.Odoo.Service.Infrastructure.Attributes;
using Adc.Odoo.Service.Infrastructure.Interfaces;

namespace Adc.Odoo.Tests.Models
{
    [OdooMap("project.project")]
    class OdooProject : IOdooObject
    {
        [OdooMap("id")]
        public int Id { get; set; }

        [OdooMap("name")]
        public string Name { get; set; }

        [OdooMap("ks_partner_invoicing_ids")]
        public ICollection<int> PartnerInvoices { get; set; }

        [OdooMap("tasks")]
        public ICollection<int> Tasks { get; set; } 
    }
}
