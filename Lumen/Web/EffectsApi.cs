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
    public async Task<ApiResponse<string>> SetEffect(NewEffectRequest data)
    {
        Log.Information($"[HTTP] {nameof(SetEffect)} triggered with {data}.");

        if (string.IsNullOrEmpty(data.Location) || string.IsNullOrEmpty(data.Effect))
        {
            return new ApiResponse<string>(HttpStatusCode.BadRequest, "A location and effect name are required");
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
            return new ApiResponse<string>(HttpStatusCode.BadRequest, "Location not found");
        }

        if (!location.IsApiEnabled)
        {
            return new ApiResponse<string>(HttpStatusCode.BadRequest, "API not enabled on location");
        }


        var effect = _effectRegistry.CreateEffectInstance(data.Effect, location.Canvas, data.Settings);

        if (effect == null)
        {
            return new ApiResponse<string>(HttpStatusCode.BadRequest, $"No effect found with name {data.Effect}");
        }

        effect.SetId(guid);

        location.SetForcedEffect((LedEffect)effect);
        return new ApiResponse<string>(HttpStatusCode.OK, guid);

    }

    /// <summary>
    /// Enqueues an effect to be played with the given data.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost("effects/enqueue")]
    public async Task<ApiResponse<string>> EnqueueEffect(NewEffectRequest data)
    {
        Log.Information($"[HTTP] {nameof(EnqueueEffect)} triggered with {data}.");

        if (string.IsNullOrEmpty(data.Location) || string.IsNullOrEmpty(data.Effect))
        {
            return new ApiResponse<string>(HttpStatusCode.BadRequest, "A location and effect name are required");
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
            return new ApiResponse<string>(HttpStatusCode.BadRequest, "Location not found");
        }

        if (!location.IsApiEnabled)
        {
            return new ApiResponse<string>(HttpStatusCode.BadRequest, "API not enabled on location");
        }

        if (location.IsEffectQueued(guid))
        {
            return new ApiResponse<string>(HttpStatusCode.BadRequest, $"Effect with ID {guid} already exists in the queue");
        }

        var effect = _effectRegistry.CreateEffectInstance(data.Effect, location.Canvas, data.Settings);

        if (effect == null)
        {
            return new ApiResponse<string>(HttpStatusCode.BadRequest, $"No effect found with name {data.Effect}");
        }

        effect.SetId(guid);

        location.EnqueueEffect(effect);
        return new ApiResponse<string>(HttpStatusCode.OK, guid);
    }

    /// <summary>
    /// Clears the queue of effects for the given location.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost("effects/clearqueue")]
    public async Task<ApiResponse<string>> ClearEffectQueue(ClearEffectQueueRequest data)
    {

        Log.Information($"[HTTP] {nameof(ClearEffectQueue)} triggered with {data}.");

        if (string.IsNullOrEmpty(data.Location))
        {
            return new ApiResponse<string>(HttpStatusCode.BadRequest, "A location is required");
        }

        var location =
            _locationRegistry.GetLocation(data.Location);
        if (location == null)
        {
            Log.Warning($"[HTTP] No location found with the given data for request {nameof(SetEffect)}.");
            return new ApiResponse<string>(HttpStatusCode.BadRequest, "Location not found");
        }

        if (!location.IsApiEnabled)
        {
            return new ApiResponse<string>(HttpStatusCode.BadRequest, "API not enabled on location");
        }

        location.GetEffectQueue().Clear();

        return new ApiResponse<string>(HttpStatusCode.OK, "Cleared");

    }

    /// <summary>
    /// Clears the active effect by setting the forced effect to null and requesting the active effect to end.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost("effects/clearactive")]
    public async Task<ApiResponse<string>> ClearActiveEffect(ClearActiveEffectRequest data)
    {
        Log.Information($"[HTTP] {nameof(ClearActiveEffect)} triggered with {data}.");

        if (string.IsNullOrEmpty(data.Location))
        {
            return new ApiResponse<string>(HttpStatusCode.BadRequest, "A location is required");
        }

        var location =
            _locationRegistry.GetLocation(data.Location);
        if (location == null)
        {
            Log.Warning($"[HTTP] No location found with the given data for request {nameof(SetEffect)}.");
            return new ApiResponse<string>(HttpStatusCode.BadRequest, "Location not found");
        }

        if (!location.IsApiEnabled)
        {
            return new ApiResponse<string>(HttpStatusCode.BadRequest, "API not enabled on location");
        }

        // Request the active effect to end by setting the forced effect to nothing.
        location.SetForcedEffect(null);

        return new ApiResponse<string>(HttpStatusCode.OK, "Cleared");
    }

    /// <summary>
    /// Sets the settings for the effect with the given ID, if the effect is not found in the queue or active effect, a 400 is returned.
    /// If the effect is found and the settings are set successfully, a 200 is returned with the new settings in JSON format
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost("effects/settings/set")]
    public async Task<ApiResponse<string>> SetEffectSettings(SetEffectSettingsRequest data)
    {
        Log.Information($"[HTTP] { nameof(SetEffectSettings)} triggered with {data}.");

        if (string.IsNullOrEmpty(data.Location) || string.IsNullOrEmpty(data.Id))
        {
            return new ApiResponse<string>(HttpStatusCode.BadRequest, "A location and effect Id are required");
        }

        var location =
            _locationRegistry.GetLocation(data.Location);

        if (location == null)
        {
                        Log.Warning($"[HTTP] No location found with the given data for request {nameof(SetEffect)}.");
            return new ApiResponse<string>(HttpStatusCode.BadRequest, "Location not found");
        }

        if (!location.IsApiEnabled)
        {
            return new ApiResponse<string>(HttpStatusCode.BadRequest, "API not enabled on location");
        }


        var effects = new List<LedEffect>() { location.ActiveEffect }.Concat(location.GetEffectQueue());
        var effect = effects.FirstOrDefault(e => e.Id == data.Id);
        if (effect == null)
        {
            return new ApiResponse<string>(HttpStatusCode.BadRequest, $"No effect found with ID {data.Id}");
        }

        effect.SetEffectSettings(data.Settings,data.MergeDefaults);


        return new ApiResponse<string>(HttpStatusCode.OK, JsonConvert.SerializeObject(effect.GetEffectSettings()));
    }

    /// <summary>
    /// Gets the current settings for the effect with the given ID, if the effect is not found in the queue or active effect, a 400 is returned.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost("effects/settings/get")]
    public async Task<ApiResponse<string>> GetEffectSettings(GetEffectSettingsRequest data)
    {
        Log.Information($"[HTTP] {nameof(GetEffectSettings)} triggered with {data}.");

        if (string.IsNullOrEmpty(data.Location) || string.IsNullOrEmpty(data.Id))
        {
            return new ApiResponse<string>(HttpStatusCode.BadRequest, "A location and effect Id are required");
        }

        var location =
            _locationRegistry.GetLocation(data.Location);

        if (location == null)
        {
            Log.Warning($"[HTTP] No location found with the given data for request {nameof(SetEffect)}.");
            return new ApiResponse<string>(HttpStatusCode.BadRequest, "Location not found");
        }

        if (!location.IsApiEnabled)
        {
            return new ApiResponse<string>(HttpStatusCode.BadRequest, "API not enabled on location");
        }

        var effects = new List<LedEffect>() { location.ActiveEffect }.Concat(location.GetEffectQueue());
        var effect = effects.FirstOrDefault(e => e.Id == data.Id);
        if (effect == null)
        {
            return new ApiResponse<string>(HttpStatusCode.BadRequest, $"No effect found with ID {data.Id}");
        }

        return new ApiResponse<string>(HttpStatusCode.OK, JsonConvert.SerializeObject(effect.GetEffectSettings()));

    }
}