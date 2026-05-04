using DiplomaFit.Data;
using DiplomaFit.Model.Dto.Food;
using DiplomaFit.Model.Entities;
using DiplomaFit.Service.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiplomaFit.Service.FoodService
{
    public class FoodService
    {
        private readonly FoodRepository _foodRepository;
        private readonly DtoProvider _dtoProvider;

        public FoodService(FoodRepository foodRepository, DtoProvider dtoProvider)
        {
            _foodRepository = foodRepository;
            _dtoProvider = dtoProvider;
        }

        public List<FoodResponseDto> GetAll()
        {
            return _foodRepository
                .GetAll()
                .Select(food => _dtoProvider.Mapper.Map<FoodResponseDto>(food))
                .ToList();
        }

        public FoodResponseDto? GetById(string id)
        {
            var food = _foodRepository.FindById(id);

            if (food == null)
                return null;

            return _dtoProvider.Mapper.Map<FoodResponseDto>(food);
        }

        public async Task<FoodResponseDto> CreateAsync(FoodCreateDto dto)
        {
            var food = _dtoProvider.Mapper.Map<Food>(dto);

            await _foodRepository.CreateAsync(food);

            return _dtoProvider.Mapper.Map<FoodResponseDto>(food);
        }

        public async Task<List<FoodResponseDto>> CreateManyAsync(List<FoodCreateDto> dtos)
        {
            var foods = dtos.Select(dto =>
            {
                var f = _dtoProvider.Mapper.Map<Food>(dto);
                f.FoodId = Guid.NewGuid().ToString();
                return f;
            }).ToList();

            await _foodRepository.CreateManyAsync(foods);

            return foods.Select(f => _dtoProvider.Mapper.Map<FoodResponseDto>(f)).ToList();
        }

        public async Task<FoodResponseDto?> UpdateAsync(string id, FoodUpdateDto dto)
        {
            var food = _foodRepository.FindById(id);

            if (food == null)
                return null;

            _dtoProvider.Mapper.Map(dto, food);

            await _foodRepository.UpdateAsync(food);

            return _dtoProvider.Mapper.Map<FoodResponseDto>(food);
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var food = _foodRepository.FindById(id);

            if (food == null)
                return false;

            await _foodRepository.Delete(food);

            return true;
        }
    }
}
