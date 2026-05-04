using AutoMapper;
using DiplomaFit.Model.Dto.Exercise;
using DiplomaFit.Model.Dto.Food;
using DiplomaFit.Model.Dto.User;
using DiplomaFit.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiplomaFit.Service.Dto
{
    public class DtoProvider
    {
        public IMapper Mapper { get; }

        public DtoProvider()
        {
            Mapper = new Mapper(new MapperConfiguration(cfg =>
            {
                // User
                cfg.CreateMap<UserCreateDto, User>();
                cfg.CreateMap<UserUpdateDto, User>();
                cfg.CreateMap<User, UserResponseDto>();

                // Food
                cfg.CreateMap<FoodCreateDto, Food>();
                cfg.CreateMap<FoodUpdateDto, Food>();
                cfg.CreateMap<Food, FoodResponseDto>();

                // Exercise
                cfg.CreateMap<ExerciseCreateDto, Exercise>();
                cfg.CreateMap<ExerciseUpdateDto, Exercise>();
                cfg.CreateMap<Exercise, ExerciseResponseDto>();
            }));
        }
    }
}
