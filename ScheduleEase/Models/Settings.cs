using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleEase.Models
{
    public class Settings
    {
        /// <summary>
        /// The client ID (aka application ID) from the app registration in the Azure portal
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// An array of permission scopes required by the application (ex. "User.Read")
        /// </summary>
        public string[] GraphScopes { get; set; }
    }
}
