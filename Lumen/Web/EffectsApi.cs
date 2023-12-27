using System.Net;
using Lumen.Registries;
using Lumen.Server;
using Lumen.Utils;
using Lumen.Web.Request;
using Microsoft.AspNetCore.Mvc;
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
    public async Task<ApiResponse> SetEffect(EffectRequestData data)
    {
        Log.Information($"[HTTP] {nameof(SetEffect)} triggered with {data}.");

        if (string.IsNullOrEmpty(data.Location) || string.IsNullOrEmpty(data.Effect))
        {
            return new ApiResponse(HttpStatusCode.BadRequest, "A location and effect name are required");
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


        var effect = _effectRegistry.CreateEffectInstance(data.Effect);

        if (effect == null)
        {
            return new ApiResponse(HttpStatusCode.BadRequest, $"No effect found with name {data.Effect}");
        }

        effect.SetEffectParameters(data.Settings);
        location.SetForcedEffect(effect);
        return new ApiResponse(HttpStatusCode.OK, "Force set effect successfully");

    }

    /// <summary>
    /// Enqueues an effect to be played with the given data.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost("effects/enqueue")]
    public async Task<ApiResponse> EnqueueEffect(EffectRequestData data)
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

        var effect = _effectRegistry.CreateEffectInstance(data.Effect);

        if (effect == null)
        {
            return new ApiResponse(HttpStatusCode.BadRequest, $"No effect found with name {data.Effect}");
        }


        location.EnqueueEffect(new QueuedEffect(data.Effect, guid, data.Settings));
        return new ApiResponse(HttpStatusCode.OK, guid);
    }

}