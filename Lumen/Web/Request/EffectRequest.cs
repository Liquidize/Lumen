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
    public record EffectRequest(string Location, string Id, string Effect, Dictionary<string, object> Settings)
    {
        public EffectRequest() : this(string.Empty, Guid.NewGuid().ToString(), string.Empty, new Dictionary<string, object>())
        {

        }
    }

}
