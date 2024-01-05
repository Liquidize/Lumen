using System.Net;
using Lumen.Registries;
using Lumen.Server;
using Microsoft.AspNetCore.Mvc;

namespace Lumen.Web
{
    [ApiController]
    [Route("api/locations")]
    public class LocationsApi : ControllerBase
    {
        private readonly ILocationRegistry _locationRegistry;
        private readonly IEffectRegistry _effectRegistry;
        private readonly ICanvasRegistry _canvasRegistry;

        public LocationsApi(ILocationRegistry locationRegistry, IEffectRegistry effectRegistry,
            ICanvasRegistry canvasRegistry)
        {
            _locationRegistry = locationRegistry;
            _effectRegistry = effectRegistry;
            _canvasRegistry = canvasRegistry;
        }

        /// <summary>
        /// Gets all the locations that are enabled for API access.
        /// </summary>
        /// <returns>Returns a list of locations that are enabled for API access.</returns>
        [HttpGet("get/all")]
        public async Task<ActionResult<IEnumerable<Location>>> GetLocations()
        {
            try
            {
                return Ok(_locationRegistry.GetLocations().Where(location => location.IsApiEnabled));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, $"An unexpected error occurred. {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the details of a location by its name if it is enabled for API access.
        /// </summary>
        /// <param name="name">The name of the location to retrieve.</param>
        /// <returns>
        /// Returns the details of the location if it is found and enabled for API access,
        /// otherwise returns a 404 status code if the location is not found,
        /// or a 403 status code if the location is not enabled for API access.
        /// </returns>
        [HttpGet("get")]
        public async Task<ActionResult<Location>> GetLocation([FromQuery] string name)
        {
            try
            {
                var location = _locationRegistry.GetLocation(name);
                if (location == null)
                    return NotFound($"Location {name} not found.");

                if (location.IsApiEnabled == false)
                    return StatusCode((int)HttpStatusCode.Forbidden, $"Location {name} is not enabled for API access.");

                return Ok(location);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, $"An unexpected error occurred. {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the names of all locations that are enabled for API access.
        /// </summary>
        /// <returns>Returns a list of names of locations that are enabled for API access.</returns>
        [HttpGet("get/names")]
        public async Task<ActionResult<IEnumerable<string>>> GetLocationNames()
        {
            try {
                return Ok(_locationRegistry.GetLocations().Where(location => location.IsApiEnabled).Select(location => location.Name));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, $"An unexpected error occurred. {ex.Message}");
            }
        }

    }
}
