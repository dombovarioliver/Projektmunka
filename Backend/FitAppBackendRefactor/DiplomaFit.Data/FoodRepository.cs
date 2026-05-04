using DiplomaFit.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiplomaFit.Data
{
    public class FoodRepository
    {
        AppDbContext ctx;

        public FoodRepository(AppDbContext ctx)
        {
            this.ctx = ctx;
        }

        public async Task CreateAsync(Food food)
        {
            ctx.Set<Food>().Add(food);
            await ctx.SaveChangesAsync();
        }

        public async Task CreateManyAsync(IEnumerable<Food> foods)
        {
            ctx.Set<Food>().AddRange(foods);
            await ctx.SaveChangesAsync();
        }

        public Food FindById(string id)
        {
            return ctx.Set<Food>().FirstOrDefault(e => e.FoodId == id);
        }

        public async Task DeleteByIdAsync(string id)
        {
            var Food = FindById(id);
            if (Food != null)
            {
                ctx.Set<Food>().Remove(Food);
                await ctx.SaveChangesAsync();
            }
        }

        public async Task Delete(Food food)
        {
            ctx.Set<Food>().Remove(food);
            await ctx.SaveChangesAsync();
        }

        public IQueryable<Food> GetAll()
        {
            return ctx.Set<Food>();
        }

        public async Task UpdateAsync(Food food)
        {
            var old = FindById(food.FoodId);
            foreach (var prop in typeof(Food).GetProperties())
            {
                prop.SetValue(old, prop.GetValue(food));
            }
            ctx.Set<Food>().Update(old);
            await ctx.SaveChangesAsync();
        }
    }
}
