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
[Route("api")]
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
    ///     Force sets an effect skipping the queue and overriding the active effect given the data from the request.
    /// </summary>
    /// <returns></returns>
    [HttpPost("effects/set")]
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
    /// Enqueues an effect to be played with the given data.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost("effects/enqueue")]
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
    /// Clears the queue of effects for the given location.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost("effects/clearqueue")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult> ClearEffectQueue(ClearEffectQueueRequest data)
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
    /// Clears the active effect by setting the forced effect to null and requesting the active effect to end.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost("effects/clearactive")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult> ClearActiveEffect(ClearActiveEffectRequest data)
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
    /// Sets the settings for the effect with the given ID, if the effect is not found in the queue or active effect, a 400 is returned.
    /// If the effect is found and the settings are set successfully, a 200 is returned with the new settings in JSON format
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost("effects/settings/set")]
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
    /// Gets the current settings for the effect with the given ID, if the effect is not found in the queue or active effect, a 400 is returned.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost("effects/settings/get")]
    [ProducesResponseType(typeof(EffectSettings), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult<EffectSettings>> GetEffectSettings(GetEffectSettingsRequest data)
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