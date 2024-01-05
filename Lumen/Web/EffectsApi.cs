using System.Net;
using Lumen.Api.Effects;
using Lumen.Registries;
using Lumen.Server;
using Lumen.Utils;
using Lumen.Web.Request;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace Lumen.Web;

[ApiController]
[Route("api/effects")]
public class EffectsApi : ControllerBase
{
    private readonly IEffectRegistry _effectRegistry;
    private readonly ICanvasRegistry _canvasRegistry;
    private readonly ILocationRegistry _locationRegistry;


    private readonly ActionResult _apiNotEnabledResult = new BadRequestObjectResult("API not enabled on location.");
    private readonly ActionResult _locationNotFoundResult = new NotFoundObjectResult("Location not found.");
    private readonly ActionResult _effectNotFoundResult = new NotFoundObjectResult("Effect not found.");
    private readonly ActionResult _locationAndEffectNameRequiredResult = new BadRequestObjectResult("A location and effect name are required.");
    private readonly ActionResult _effectIdRequiredResult = new BadRequestObjectResult("An effect ID is required.");

    private readonly ActionResult _effectTypeNotFound =
        new NotFoundObjectResult("Effect type with provided name was not found in registry.");


    public EffectsApi(IEffectRegistry effectRegistry, ICanvasRegistry canvasRegistry, ILocationRegistry locationRegistry)
    {
        _effectRegistry = effectRegistry;
        _canvasRegistry = canvasRegistry;
        _locationRegistry = locationRegistry;
    }




    /// <summary>
    /// Sets an effect to play immediately on the location with the given effect name and settings.
    /// </summary>
    /// <param name="data">The request data containing the location, effect name, settings, and optional effect ID.</param>
    /// <returns>Returns the unique identifier of the effect.</returns>

    [HttpPost("set")]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult<string>> SetEffect(NewEffectRequest data)
    {
        try
        {
            Log.Information($"[HTTP] {nameof(SetEffect)} triggered with {data}.");

            if (string.IsNullOrEmpty(data.Location) || string.IsNullOrEmpty(data.Effect))
            {
                return _locationAndEffectNameRequiredResult;
            }

            var guid = data.Id;
            if (string.IsNullOrEmpty(guid))
            {
                guid = Guid.NewGuid().ToString("N").Substring(0, 8);
            }

            var location = _locationRegistry.GetLocation(data.Location);
            if (location == null)
            {
                Log.Warning($"[HTTP] No location found with the given data for request {nameof(SetEffect)}.");
                return _locationNotFoundResult;
            }

            if (!location.IsApiEnabled)
            {
                return _apiNotEnabledResult;
            }

            var effectType = _effectRegistry.GetEffectType(data.Effect);
            if (effectType == null)
            {
                return _effectNotFoundResult;
            }

            var effect = _effectRegistry.CreateEffectInstance(data.Effect, location.Canvas, data.Settings);
            if (effect == null)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    $"An internal error caused the effect {data.Effect} to not get created.");
            }

            effect.SetId(guid);
            location.SetForcedEffect((LedEffect?)effect);

            return Ok(guid);
        }
        catch (ArgumentException ex)
        {
            return StatusCode((int)HttpStatusCode.BadRequest, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
        }
        catch (Exception ex)
        {
            Log.Error($"An unexpected error occurred: {ex}");
            return StatusCode((int)HttpStatusCode.InternalServerError, "An unexpected error occurred.");
        }

    }

    /// <summary>
    /// Queues an effect to be played from the provided effect name and settings.
    /// </summary>
    /// <param name="data">The request data containing the location, effect name, settings, and optional effect ID.</param>
    /// <returns>Returns the unique identifier of the effect.</returns>
    [HttpPost("enqueue")]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult<string>> EnqueueEffect(NewEffectRequest data)
    {
        try
        {
            Log.Information($"[HTTP] {nameof(EnqueueEffect)} triggered with {data}.");

            if (string.IsNullOrEmpty(data.Location) || string.IsNullOrEmpty(data.Effect))
            {
                return _locationAndEffectNameRequiredResult;
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
                Log.Warning($"[HTTP] No location found with the given data for request {nameof(EnqueueEffect)}.");
                return _locationNotFoundResult;
            }

            if (!location.IsApiEnabled)
            {
                return _apiNotEnabledResult;
            }

            if (location.IsEffectQueued(guid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest,
                    $"Effect with ID {guid} already exists in the queue");
            }

            var effect = _effectRegistry.CreateEffectInstance(data.Effect, location.Canvas, data.Settings);

            if (effect == null)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    $"An internal error caused the effect {data.Effect} to not get created.");
            }
            effect.SetId(guid);
            location.EnqueueEffect(effect);
            
            return Ok(guid);
        }
        catch (ArgumentException ex)
        {
            return StatusCode((int)HttpStatusCode.BadRequest, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
        }
        catch (Exception ex)
        {
            Log.Error($"An unexpected error occurred: {ex}");
            return StatusCode((int)HttpStatusCode.InternalServerError, "An unexpected error occurred.");
        }
    }

    /// <summary>
    /// Clears the queue of effects for a location.
    /// </summary>
    /// <param name="data">The request data containing the location.</param>
    /// <returns>Returns an HTTP status code indicating the result of the operation.</returns>
    [HttpDelete("clear/queue")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult> ClearEffectQueue([FromQuery] ClearEffectQueueRequest data)
    {
        try
        {
            Log.Information($"[HTTP] {nameof(ClearEffectQueue)} triggered with {data}.");

            if (string.IsNullOrEmpty(data.Location))
            {
                return BadRequest("A Location is required for this request.");
            }

            var location =
                _locationRegistry.GetLocation(data.Location);

            if (location == null)
            {
                Log.Warning($"[HTTP] No location found with the given data for request {nameof(ClearEffectQueue)}.");
                return _locationNotFoundResult;
            }

            if (!location.IsApiEnabled)
            {
                return _apiNotEnabledResult;
            }

            location.GetEffectQueue().Clear();

            return Ok();
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"[HTTP] {nameof(ClearEffectQueue)} failed with {data}.");
            return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
        }

    }

    /// <summary>
    /// Clears the active effect by requesting it to end immediately.
    /// </summary>
    /// <param name="data">The request data containing the location.</param>
    /// <returns>Returns an HTTP status code indicating the result of the operation.</returns>
    [HttpDelete("clear/active")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult> ClearActiveEffect([FromQuery]ClearActiveEffectRequest data)
    {
        try
        {
            Log.Information($"[HTTP] {nameof(ClearActiveEffect)} triggered with {data}.");

            if (string.IsNullOrEmpty(data.Location))
            {
                return StatusCode((int)HttpStatusCode.BadRequest,
                    $"A location is required for the {nameof(ClearActiveEffect)} request.");
            }

            var location =
                _locationRegistry.GetLocation(data.Location);
            if (location == null)
            {
                Log.Warning($"[HTTP] No location found with the given data for request {nameof(SetEffect)}.");
                return _locationNotFoundResult;
            }

            if (!location.IsApiEnabled)
            {
                return _apiNotEnabledResult;
            }

            // Request active effect to end
            location.ActiveEffect?.RequestEnd();

            return Ok();
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"[HTTP] {nameof(ClearActiveEffect)} failed with {data}.");
            return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    /// <summary>
    /// Sets the settings for an effect from the provided effect ID and settings. If a setting is not provided, it will be set to the default value.
    /// </summary>
    /// <param name="data">The request data containing the location, effect ID, and new settings for the effect.</param>
    /// <returns>Returns the updated settings of the effect.</returns>
    [HttpPatch("settings/set")]
    [ProducesResponseType(typeof(EffectSettings), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult<EffectSettings>> SetEffectSettings(SetEffectSettingsRequest data)
    {
        try
        {
            Log.Information($"[HTTP] {nameof(SetEffectSettings)} triggered with {data}.");

            if (string.IsNullOrEmpty(data.Location) || string.IsNullOrEmpty(data.Id))
            {
                return BadRequest("A Location and Effect Id are required");
            }

            var location =
                _locationRegistry.GetLocation(data.Location);

            if (location == null)
            {
                Log.Warning($"[HTTP] No location found with the given data for request {nameof(SetEffect)}.");
                return _locationNotFoundResult;
            }

            if (!location.IsApiEnabled)
            {
                return _apiNotEnabledResult;
            }


            var effects = new List<LedEffect?>() { location.ActiveEffect }.Concat(location.GetEffectQueue());
            var effect = effects.FirstOrDefault(e => e != null && e.Id == data.Id);
            if (effect == null)
            {
                return NotFound($"No effect found with ID {data.Id} on location {data.Location}");
            }

            effect.SetEffectSettings(data.Settings);

            var settings = effect.GetEffectSettings();

            return Ok(settings);
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"[HTTP] {nameof(SetEffectSettings)} failed with {data}.");
            return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    /// <summary>
    /// Gets the current settings of an effect from the provided location and effect ID. If the effect is not found in the queue or active effect, a 400 is returned.
    /// </summary>
    /// <param name="data">The request data containing the location and effect ID.</param>
    /// <returns>Returns the settings of the effect if found, otherwise an HTTP status code indicating the result of the operation.</returns>
    [HttpGet("settings/get")]
    [ProducesResponseType(typeof(EffectSettings), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult<EffectSettings>> GetEffectSettings([FromQuery]GetEffectSettingsRequest data)
    {
        try
        {
            Log.Information($"[HTTP] {nameof(GetEffectSettings)} triggered with {data}.");

            if (string.IsNullOrEmpty(data.Location) || string.IsNullOrEmpty(data.Id))
            {
                BadRequest("A location and effect Id are required");
            }

            var location =
                _locationRegistry.GetLocation(data.Location);

            if (location == null)
            {
                Log.Warning($"[HTTP] No location found with the given data for request {nameof(SetEffect)}.");
                return _locationNotFoundResult;
            }

            if (!location.IsApiEnabled)
            {
                return _apiNotEnabledResult;
            }

            var effects = new List<LedEffect?>() { location.ActiveEffect }.Concat(location.GetEffectQueue());
            var effect = effects.FirstOrDefault(e => e != null && e.Id == data.Id);
            if (effect == null)
            {
                return NotFound($"No effect found with ID {data.Id}");
            }

            var settings = effect.GetEffectSettings();

            return Ok(settings);
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"[HTTP] {nameof(GetEffectSettings)} failed with {data}.");
            return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
        }
    }

}