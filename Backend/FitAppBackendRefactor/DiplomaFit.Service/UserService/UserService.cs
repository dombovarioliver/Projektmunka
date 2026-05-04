using DiplomaFit.Data;
using DiplomaFit.Model.Dto.User;
using DiplomaFit.Service.Dto;
using DiplomaFit.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
