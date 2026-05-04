using DiplomaFit.Model.Dto.Food;
using DiplomaFit.Service.FoodService;
using Microsoft.AspNetCore.Mvc;

namespace FitAppBackendRefactor.Controllers
{
    [ApiController]
    [Route("api/foods")]
    public class FoodsController : ControllerBase
    {
        private readonly FoodService _foodService;

        public FoodsController(FoodService foodService)
        {
            _foodService = foodService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_foodService.GetAll());
        }

        [HttpGet("{id}")]
        public IActionResult GetById(string id)
        {
            var food = _foodService.GetById(id);

            if (food == null)
                return NotFound();

            return Ok(food);
        }

        [HttpPost]
        public async Task<IActionResult> Create(FoodCreateDto dto)
        {
            var createdFood = await _foodService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = createdFood.FoodId },
                createdFood);
        }

        [HttpPost("bulk")]
        public async Task<IActionResult> CreateMany(List<FoodCreateDto> dtos)
        {
            var createdFoods = await _foodService.CreateManyAsync(dtos);

            return Ok(createdFoods);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, FoodUpdateDto dto)
        {
            var updatedFood = await _foodService.UpdateAsync(id, dto);

            if (updatedFood == null)
                return NotFound();

            return Ok(updatedFood);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var deleted = await _foodService.DeleteAsync(id);

            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}