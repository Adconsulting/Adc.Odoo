namespace Adc.Odoo.Service.Models
{
    public class OdooConnection
    {
        /// <summary>
        /// Gets or Sets the Url
        /// </summary>
        public virtual string Url { get; set; }

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
    }
}
