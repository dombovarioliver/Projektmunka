namespace DiplomaFit.Api.Application.Dtos.Cases
{
    public class CreateCaseRequestDto
    {
        //0 = male, 1 = female
        public int Gender { get; set; }
        public int Age { get; set; }
        public int HeightCm { get; set; }
        public double WeightKg { get; set; }
        //Ha nem ismert: 0
        public double BodyfatPercent { get; set; }
        //Aktivitási szint pl. 1-5
        public int ActivityLevel { get; set; }

        //0 = Maintain, 1 = Lose, 2 = Gain
        public int GoalType { get; set; }

        //Hány kg-ot szeretne változtatni
        public int GoalDeltaKg { get; set; }

        //Cél időtáv hétben
        public int GoalTimeWeeks { get; set; }
    }
}
