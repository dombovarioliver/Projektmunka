using DiplomaFit.Data;
using DiplomaFit.Model.Dto.User;
using DiplomaFit.Service.Dto;
using DiplomaFit.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace DiplomaFit.Service.UserService
{
    public class UserService
    {
        private readonly UserRepository _userRepository;
        private readonly DtoProvider _dtoProvider;

        public UserService(UserRepository userRepository, DtoProvider dtoProvider)
        {
            _userRepository = userRepository;
            _dtoProvider = dtoProvider;
        }

        public List<UserResponseDto> GetAll()
        {
            return _userRepository
                .GetAll()
                .Select(user => _dtoProvider.Mapper.Map<UserResponseDto>(user))
                .ToList();
        }

        public UserResponseDto? GetById(string id)
        {
            var user = _userRepository.FindById(id);

            if (user == null)
                return null;

            return _dtoProvider.Mapper.Map<UserResponseDto>(user);
        }

        public async Task<UserResponseDto> CreateAsync(UserCreateDto dto)
        {
            var user = _dtoProvider.Mapper.Map<User>(dto);

            await _userRepository.CreateAsync(user);

            return _dtoProvider.Mapper.Map<UserResponseDto>(user);
        }

        public async Task<UserResponseDto> FindByEmailAsync(string email)
        {
            var user = await _userRepository.FindByEmailAsync(email);
            return _dtoProvider.Mapper.Map<UserResponseDto>(user);
        }

        public async Task<List<UserResponseDto>> CreateManyAsync(List<UserCreateDto> dtos)
        {
            var users = dtos.Select(dto =>
            {
                var u = _dtoProvider.Mapper.Map<User>(dto);
                u.Id = Guid.NewGuid().ToString();
                return u;
            }).ToList();

            await _userRepository.CreateManyAsync(users);

            return users.Select(u => _dtoProvider.Mapper.Map<UserResponseDto>(u)).ToList();
        }

        public async Task<UserResponseDto?> UpdateAsync(string id, UserUpdateDto dto)
        {
            var user = _userRepository.FindById(id);

            if (user == null)
                return null;

            _dtoProvider.Mapper.Map(dto, user);

            await _userRepository.UpdateAsync(user);

            return _dtoProvider.Mapper.Map<UserResponseDto>(user);
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var user = _userRepository.FindById(id);

            if (user == null)
                return false;

            await _userRepository.Delete(user);

            return true;
        }

        public async Task<string?> UploadProfilePictureAsync(string id, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;

            var user = _userRepository.FindById(id);

            if (user == null)
                return null;

            var allowedExtensions = new[]
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".webp"
            };

            var extension = Path
                .GetExtension(file.FileName)
                .ToLower();

            if (!allowedExtensions.Contains(extension))
                return null;

            var uploadFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads",
                "profile-pictures"
            );

            if (!Directory.Exists(uploadFolder))
                Directory.CreateDirectory(uploadFolder);

            var fileName =
                $"{id}_{Guid.NewGuid()}{extension}";

            var filePath = Path.Combine(
                uploadFolder,
                fileName
            );

            using (var stream = new FileStream(
                filePath,
                FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            user.ProfilePictureUrl =
                $"/uploads/profile-pictures/{fileName}";

            await _userRepository.UpdateAsync(user);

            return user.ProfilePictureUrl;
        }
    }
}
