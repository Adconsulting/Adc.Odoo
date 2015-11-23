# Adc.Odoo
A small C# API for Odoo 8.0 based on the work of https://openerpnet.codeplex.com/

# Usage
## Objects
The lib works with Odoo Object (Objects implementing the IOdooObject interface)
    
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

Linking a class / property to an Odoo table / field is done by using the OdooMap attribute

## Connecting 
the connection setup is handled by the OdooConnection object. This is the only required parameter of the OdooService constructor. 

# OdooService
this is the base class for all communication with odoo. This class has generic methods for add / update / delete / list 





