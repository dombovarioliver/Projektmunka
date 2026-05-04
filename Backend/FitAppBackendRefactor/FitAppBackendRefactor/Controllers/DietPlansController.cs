using DiplomaFit.Service.DietService;
using DiplomaFit.Service.UserService;
using Microsoft.AspNetCore.Mvc;

namespace FitAppBackendRefactor.Controllers
{
    [ApiController]
    [Route("api/diet-plans")]
    public class DietPlansController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly DietPlanService _dietPlanService;

        public DietPlansController(
            UserService userService,
            DietPlanService dietPlanService)
        {
            _userService = userService;
            _dietPlanService = dietPlanService;
        }

        [HttpPost("generate/{userId}")]
        public async Task<IActionResult> Generate(string userId, CancellationToken cancellationToken)
        {
            var user = _userService.GetById(userId);

            if (user == null)
                return NotFound("A megadott felhasználó nem található.");

            var plan = await _dietPlanService.GenerateWeeklyPlanAsync(user, cancellationToken);

            return Ok(plan);
        }
    }
}