using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Lumen.Api.Effects;
using Lumen.Api.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Lumen.Web.Request
{
    public record NewEffectRequest(string Location, string Id, string Effect, JObject Settings)
    {
        public NewEffectRequest() : this(string.Empty, Guid.NewGuid().ToString(), string.Empty, new JObject())
        {

        }
    }

}
