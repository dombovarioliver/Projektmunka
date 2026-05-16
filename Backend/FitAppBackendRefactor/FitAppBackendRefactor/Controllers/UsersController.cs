using DiplomaFit.Model.Dto.User;
using DiplomaFit.Service.UserService;
using Microsoft.AspNetCore.Mvc;

namespace FitAppBackendRefactor.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;

        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_userService.GetAll());
        }

        [HttpGet("{id}")]
        public IActionResult GetById(string id)
        {
            var user = _userService.GetById(id);

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> Create(UserCreateDto dto)
        {
            var createdUser = await _userService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = createdUser.Id },
                createdUser);
        }

        [HttpPost("bulk")]
        public async Task<IActionResult> CreateMany(List<UserCreateDto> dtos)
        {
            var createdUsers = await _userService.CreateManyAsync(dtos);

            return Ok(createdUsers);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, UserUpdateDto dto)
        {
            var updatedUser = await _userService.UpdateAsync(id, dto);

            if (updatedUser == null)
                return NotFound();

            return Ok(updatedUser);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var deleted = await _userService.DeleteAsync(id);

            if (!deleted)
                return NotFound();

            return NoContent();
        }

        [HttpPost("{id}/profile-picture")]
        public async Task<IActionResult> UploadProfilePicture(string id, IFormFile file)
        {
            var result = await _userService.UploadProfilePictureAsync(id, file);

            if (result == null)
                return NotFound();

            return Ok(new
            {
                profilePictureUrl = result
            });
        }
    }
}