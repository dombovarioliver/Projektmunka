using DiplomaFit.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiplomaFit.Data
{
    public class ExerciseRepository
    {
        AppDbContext ctx;

        public ExerciseRepository(AppDbContext ctx)
        {
            this.ctx = ctx;
        }

        public async Task CreateAsync(Exercise exercise)
        {
            ctx.Set<Exercise>().Add(exercise);
            await ctx.SaveChangesAsync();
        }

        public async Task CreateManyAsync(IEnumerable<Exercise> exercises)
        {
            ctx.Set<Exercise>().AddRange(exercises);
            await ctx.SaveChangesAsync();
        }

        public Exercise FindById(string id)
        {
            return ctx.Set<Exercise>().FirstOrDefault(e => e.ExerciseId == id);
        }

        public async Task DeleteByIdAsync(string id)
        {
            var exercise = FindById(id);
            if (exercise != null)
            {
                ctx.Set<Exercise>().Remove(exercise);
                await ctx.SaveChangesAsync();
            }
        }

        public async Task Delete(Exercise exercise)
        {
            ctx.Set<Exercise>().Remove(exercise);
            await ctx.SaveChangesAsync();
        }

        public IQueryable<Exercise> GetAll()
        {
            return ctx.Set<Exercise>();
        }

        public async Task UpdateAsync(Exercise exercise)
        {
            var old = FindById(exercise.ExerciseId);
            foreach (var prop in typeof(Exercise).GetProperties())
            {
                prop.SetValue(old, prop.GetValue(exercise));
            }
            ctx.Set<Exercise>().Update(old);
            await ctx.SaveChangesAsync();
        }
    }
}
