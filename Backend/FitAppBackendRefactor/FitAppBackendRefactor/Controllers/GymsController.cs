using DiplomaFit.Model.Dto.Gym;
using DiplomaFit.Service.GymService;
using Microsoft.AspNetCore.Mvc;

namespace FitAppBackend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GymsController : ControllerBase
    {
        private readonly GooglePlacesGymService _gymService;

        public GymsController(GooglePlacesGymService gymService)
        {
            _gymService = gymService;
        }

        [HttpPost("budapest/refresh")]
        public async Task<ActionResult<List<GymDto>>> RefreshBudapestGyms(CancellationToken cancellationToken)
        {
            var gyms = await _gymService.RefreshBudapestGymsAsync(cancellationToken);
            return Ok(gyms);
        }

        [HttpGet("budapest")]
        public async Task<ActionResult<List<GymDto>>> GetBudapestGyms(CancellationToken cancellationToken)
        {
            var gyms = await _gymService.GetBudapestGymsAsync(cancellationToken);
            return Ok(gyms);
        }
    }
}
