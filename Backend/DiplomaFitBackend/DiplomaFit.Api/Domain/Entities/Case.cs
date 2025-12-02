namespace DiplomaFit.Api.Domain.Entities
{
    public class Case
    {
        public Guid CaseId { get; set; }

        public int Gender { get; set; }            // 0 = férfi, 1 = nő
        public int Age { get; set; }
        public int HeightCm { get; set; }
        public double WeightKg { get; set; }

        public double? BodyfatPercent { get; set; }   // NULL kezelése fontos

        public int ActivityLevel { get; set; }     // pl. 1..5
        public int GoalType { get; set; }          // szám, pl. 0,1,2
        public int GoalDeltaKg { get; set; }
        public int GoalTimeWeeks { get; set; }

        public Guid? PlanId { get; set; }
        public DietPlan? Plan { get; set; } = null!;
    }
}
