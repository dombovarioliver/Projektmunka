using DiplomaFit.Model.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiplomaFit.Data
{
    public class UserRepository
    {
        AppDbContext ctx;

        public UserRepository(AppDbContext ctx)
        {
            this.ctx = ctx;
        }

        public async Task CreateAsync(User user)
        {
            ctx.Set<User>().Add(user);
            await ctx.SaveChangesAsync();
        }

        public async Task CreateManyAsync(IEnumerable<User> users)
        {
            ctx.Set<User>().AddRange(users);
            await ctx.SaveChangesAsync();
        }

        public User FindById(string id)
        {
            return ctx.Set<User>().FirstOrDefault(e => e.Id == id);
        }

        public async Task<User> FindByEmailAsync(string email)
        {
            return await ctx.Set<User>().FirstAsync(e => e.Email == email);
        }

        public async Task DeleteByIdAsync(string id)
        {
            var user = FindById(id);
            if (user != null)
            {
                ctx.Set<User>().Remove(user);
                await ctx.SaveChangesAsync();
            }
        }

        public async Task Delete(User user)
        {
            ctx.Set<User>().Remove(user);
            await ctx.SaveChangesAsync();
        }

        public IQueryable<User> GetAll()
        {
            return ctx.Set<User>();
        }

        public async Task UpdateAsync(User user)
        {
            var old = FindById(user.Id);
            foreach (var prop in typeof(User).GetProperties())
            {
                prop.SetValue(old, prop.GetValue(user));
            }
            ctx.Set<User>().Update(old);
            await ctx.SaveChangesAsync();
        }
    }
}
