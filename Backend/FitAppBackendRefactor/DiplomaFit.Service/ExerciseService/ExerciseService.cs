using DiplomaFit.Data;
using DiplomaFit.Model.Dto.Exercise;
using DiplomaFit.Model.Entities;
using DiplomaFit.Service.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiplomaFit.Service.ExerciseService
{
    public class ExerciseService
    {
        private readonly ExerciseRepository _exerciseRepository;
        private readonly DtoProvider _dtoProvider;

        public ExerciseService(ExerciseRepository exerciseRepository, DtoProvider dtoProvider)
        {
            _exerciseRepository = exerciseRepository;
            _dtoProvider = dtoProvider;
        }

        public List<ExerciseResponseDto> GetAll()
        {
            return _exerciseRepository
                .GetAll()
                .Select(exercise => _dtoProvider.Mapper.Map<ExerciseResponseDto>(exercise))
                .ToList();
        }

        public ExerciseResponseDto? GetById(string id)
        {
            var exercise = _exerciseRepository.FindById(id);

            if (exercise == null)
                return null;

            return _dtoProvider.Mapper.Map<ExerciseResponseDto>(exercise);
        }

        public async Task<ExerciseResponseDto> CreateAsync(ExerciseCreateDto dto)
        {
            var exercise = _dtoProvider.Mapper.Map<Exercise>(dto);

            await _exerciseRepository.CreateAsync(exercise);

            return _dtoProvider.Mapper.Map<ExerciseResponseDto>(exercise);
        }

        public async Task<List<ExerciseResponseDto>> CreateManyAsync(List<ExerciseCreateDto> dtos)
        {
            var exercises = dtos.Select(dto =>
            {
                var e = _dtoProvider.Mapper.Map<Exercise>(dto);
                e.ExerciseId = Guid.NewGuid().ToString();
                return e;
            }).ToList();

            await _exerciseRepository.CreateManyAsync(exercises);

            return exercises.Select(e => _dtoProvider.Mapper.Map<ExerciseResponseDto>(e)).ToList();
        }

        public async Task<ExerciseResponseDto?> UpdateAsync(string id, ExerciseUpdateDto dto)
        {
            var exercise = _exerciseRepository.FindById(id);

            if (exercise == null)
                return null;

            _dtoProvider.Mapper.Map(dto, exercise);

            await _exerciseRepository.UpdateAsync(exercise);

            return _dtoProvider.Mapper.Map<ExerciseResponseDto>(exercise);
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var exercise = _exerciseRepository.FindById(id);

            if (exercise == null)
                return false;

            await _exerciseRepository.Delete(exercise);

            return true;
        }

        public ExerciseVideoDto? GetVideoById(string id)
        {
            var exercise = _exerciseRepository.FindById(id);
            if (exercise == null)
                return null;
            return _dtoProvider.Mapper.Map<ExerciseVideoDto>(exercise);
        }
    }
}
