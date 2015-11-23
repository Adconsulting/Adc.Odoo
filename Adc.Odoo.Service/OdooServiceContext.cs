using Adc.Odoo.Service.Infrastructure.Interfaces;

namespace Adc.Odoo.Service
{
    class OdooServiceContext
    {
        /// <summary>
        /// Gets or Sets the Database
        /// </summary>
        public virtual IOdooDatabase OdooDatabase { get; set; }

        /// <summary>
        /// Gets or Sets the Authentication
        /// </summary>
        public virtual IOdooLogin OdooAuthentication { get; set; }

        /// <summary>
        /// Gets or Sets the Data
        /// </summary>
        public virtual IOdoo OdooData { get; set; }

        /// <summary>
        /// Gets or Sets the Username
        /// </summary>
        public virtual string Username { get; set; }

        /// <summary>
        /// Gets or Sets the Password
        /// </summary>
        public virtual string Password { get; set; }

        /// <summary>
        /// Gets or Sets the Database
        /// </summary>
        public virtual string Database { get; set; }

        /// <summary>
        /// Gets or Sets the UserId
        /// </summary>
        public virtual int UserId { get; set; }

    }
}
