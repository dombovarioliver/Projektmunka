using Microsoft.EntityFrameworkCore;

namespace DiplomaFit.Api.Domain.Entities
{
    public class DietPlan
    {
        public Guid PlanId { get; set; } 
        public double Calories { get; set; }
        public double Protein { get; set; }
        public double Carbs { get; set; }
        public double Fat { get; set; }
        public int MealsPerDay { get; set; }
        public int SnacksPerDay { get; set; }

        public ICollection<Meal> Meals { get; set; } = new List<Meal>();
        public ICollection<Case> Cases { get; set; } = new List<Case>();
    }
}
