using System.Net;
using Lumen.Api.Effects;
using Lumen.Registries;
using Lumen.Server;
using Lumen.Utils;
using Lumen.Web.Request;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Serilog;

namespace Lumen.Web;

[ApiController]
[Route("api")]
public class EffectsApi : ControllerBase
{
    private readonly IEffectRegistry _effectRegistry;
    private readonly ICanvasRegistry _canvasRegistry;
    private readonly ILocationRegistry _locationRegistry;


    public EffectsApi(IEffectRegistry effectRegistry, ICanvasRegistry canvasRegistry, ILocationRegistry locationRegistry)
    {
        _effectRegistry = effectRegistry;
        _canvasRegistry = canvasRegistry;
        _locationRegistry = locationRegistry;
    }




    /// <summary>
    ///     Force sets an effect skipping the queue and overriding the active effect given the data from the request.
    /// </summary>
    /// <returns></returns>
    [HttpPost("effects/set")]
    public async Task<ApiResponse> SetEffect(EffectRequest data)
    {
        Log.Information($"[HTTP] {nameof(SetEffect)} triggered with {data}.");

        if (string.IsNullOrEmpty(data.Location) || string.IsNullOrEmpty(data.Effect))
        {
            return new ApiResponse(HttpStatusCode.BadRequest, "A location and effect name are required");
        }

        var guid = data.Id;
        if (string.IsNullOrEmpty(guid))
        {
            guid = Guid.NewGuid().ToString("N").Substring(0, 8);
        }

        var location =
            _locationRegistry.GetLocation(data.Location);
        if (location == null)
        {
            Log.Warning($"[HTTP] No location found with the given data for request {nameof(SetEffect)}.");
            return new ApiResponse(HttpStatusCode.BadRequest, "Location not found");
        }

        if (!location.IsApiEnabled)
        {
            return new ApiResponse(HttpStatusCode.BadRequest, "API not enabled on location");
        }


        var effect = _effectRegistry.CreateEffectInstance(data.Effect, location.Canvas, data.Settings);

        if (effect == null)
        {
            return new ApiResponse(HttpStatusCode.BadRequest, $"No effect found with name {data.Effect}");
        }

        effect.SetId(guid);

        location.SetForcedEffect((LedEffect)effect);
        return new ApiResponse(HttpStatusCode.OK, guid);

    }

    /// <summary>
    /// Enqueues an effect to be played with the given data.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost("effects/enqueue")]
    public async Task<ApiResponse> EnqueueEffect(EffectRequest data)
    {
        Log.Information($"[HTTP] {nameof(EnqueueEffect)} triggered with {data}.");

        if (string.IsNullOrEmpty(data.Location) || string.IsNullOrEmpty(data.Effect))
        {
            return new ApiResponse(HttpStatusCode.BadRequest, "A location and effect name are required");
        }

        var guid = data.Id;
        if (string.IsNullOrEmpty(guid))
        {
            guid = Guid.NewGuid().ToString("N").Substring(0, 8);
        }

        var location =
            _locationRegistry.GetLocation(data.Location);
        if (location == null)
        {
            Log.Warning($"[HTTP] No location found with the given data for request {nameof(SetEffect)}.");
            return new ApiResponse(HttpStatusCode.BadRequest, "Location not found");
        }

        if (!location.IsApiEnabled)
        {
            return new ApiResponse(HttpStatusCode.BadRequest, "API not enabled on location");
        }

        if (location.IsEffectQueued(guid))
        {
            return new ApiResponse(HttpStatusCode.BadRequest, $"Effect with ID {guid} already exists in the queue");
        }

        var effect = _effectRegistry.CreateEffectInstance(data.Effect, location.Canvas, data.Settings);

        if (effect == null)
        {
            return new ApiResponse(HttpStatusCode.BadRequest, $"No effect found with name {data.Effect}");
        }

        effect.SetId(guid);

        location.EnqueueEffect(effect);
        return new ApiResponse(HttpStatusCode.OK, guid);
    }

    /// <summary>
    /// Clears the queue of effects for the given location.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost("effects/clear")]
    public async Task<ApiResponse> ClearEffectQueue(EffectRequest data)
    {

        Log.Information($"[HTTP] {nameof(ClearEffectQueue)} triggered with {data}.");

        if (string.IsNullOrEmpty(data.Location))
        {
            return new ApiResponse(HttpStatusCode.BadRequest, "A location is required");
        }

        var location =
            _locationRegistry.GetLocation(data.Location);
        if (location == null)
        {
            Log.Warning($"[HTTP] No location found with the given data for request {nameof(SetEffect)}.");
            return new ApiResponse(HttpStatusCode.BadRequest, "Location not found");
        }

        if (!location.IsApiEnabled)
        {
            return new ApiResponse(HttpStatusCode.BadRequest, "API not enabled on location");
        }

        location.GetEffectQueue().Clear();

        return new ApiResponse(HttpStatusCode.OK, "Effect queue cleared");

    }

    [HttpPost("effects/settings/set")]
    public async Task<ApiResponse> SetEffectSettings(EffectSettingsRequest data)
    {
        Log.Information($"[HTTP] { nameof(SetEffectSettings)} triggered with {data}.");

        if (string.IsNullOrEmpty(data.Location) || string.IsNullOrEmpty(data.Id))
        {
            return new ApiResponse(HttpStatusCode.BadRequest, "A location and effect Id are required");
        }

        var location =
            _locationRegistry.GetLocation(data.Location);

        if (location == null)
        {
                        Log.Warning($"[HTTP] No location found with the given data for request {nameof(SetEffect)}.");
            return new ApiResponse(HttpStatusCode.BadRequest, "Location not found");
        }

        if (!location.IsApiEnabled)
        {
            return new ApiResponse(HttpStatusCode.BadRequest, "API not enabled on location");
        }


        var effects = new List<LedEffect>() { location.ActiveEffect }.Concat(location.GetEffectQueue());
        var effect = effects.FirstOrDefault(e => e.Id == data.Id);
        if (effect == null)
        {
            return new ApiResponse(HttpStatusCode.BadRequest, $"No effect found with ID {data.Id}");
        }

        effect.SetEffectSettings(data.Settings,data.MergeDefaults);


        return new ApiResponse(HttpStatusCode.OK, JsonConvert.SerializeObject(effect.GetEffectSettings()));
    }
}