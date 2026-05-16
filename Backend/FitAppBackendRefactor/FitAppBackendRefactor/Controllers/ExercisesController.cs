using DiplomaFit.Model.Dto.Exercise;
using DiplomaFit.Service.ExerciseService;
using Microsoft.AspNetCore.Mvc;

namespace FitAppBackendRefactor.Controllers
{
    [ApiController]
    [Route("api/exercises")]
    public class ExercisesController : ControllerBase
    {
        private readonly ExerciseService _exerciseService;

        public ExercisesController(ExerciseService exerciseService)
        {
            _exerciseService = exerciseService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_exerciseService.GetAll());
        }

        [HttpGet("{id}")]
        public IActionResult GetById(string id)
        {
            var exercise = _exerciseService.GetById(id);

            if (exercise == null)
                return NotFound();

            return Ok(exercise);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ExerciseCreateDto dto)
        {
            var createdExercise = await _exerciseService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = createdExercise.ExerciseId },
                createdExercise);
        }

        [HttpPost("bulk")]
        public async Task<IActionResult> CreateMany(List<ExerciseCreateDto> dtos)
        {
            var createdExercises = await _exerciseService.CreateManyAsync(dtos);

            return Ok(createdExercises);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, ExerciseUpdateDto dto)
        {
            var updatedExercise = await _exerciseService.UpdateAsync(id, dto);

            if (updatedExercise == null)
                return NotFound();

            return Ok(updatedExercise);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var deleted = await _exerciseService.DeleteAsync(id);

            if (!deleted)
                return NotFound();

            return NoContent();
        }

        [HttpGet("{id}/video")]
        public IActionResult GetVideoById(string id)
        {
            var video = _exerciseService.GetVideoById(id);
            if (video == null)
                return NotFound();
            return Ok(video);
        }
    }
}