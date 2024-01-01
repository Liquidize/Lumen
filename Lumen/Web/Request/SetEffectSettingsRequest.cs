using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Lumen.Api.Graphics;
using Newtonsoft.Json;

namespace Lumen.Web.Request
{
    /// <summary>
    /// Request data for setting effect settings via the API
    /// </summary>
    /// <param name="Location">Name of the location to make request too</param>
    /// <param name="Id">Unique ID of the effect to set the settings for</param>
    /// <param name="MergeDefaults">Whether to merge the default settings in for any missed keys, defaults to false. If false then merge in the current settings for any missed keys. If true merge in effect defaults.</param>
    /// <param name="Settings">JSON key-value object as a dictionary containing the new settings</param>
    public record SetEffectSettingsRequest(string Location, string Id, bool MergeDefaults, Dictionary<string, object> Settings)
    {
        public SetEffectSettingsRequest() : this(string.Empty, string.Empty, false, new Dictionary<string, object>())
        {

        }
    }

}
