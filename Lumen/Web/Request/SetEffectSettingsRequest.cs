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
    public record SetEffectSettingsRequest(string Location, string Id, bool MergeDefaults, Dictionary<string, object> Settings)
    {
        public SetEffectSettingsRequest() : this(string.Empty, string.Empty, false, new Dictionary<string, object>())
        {

        }
    }

}
