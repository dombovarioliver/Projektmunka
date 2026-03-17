namespace DiplomaFit.Api.Application.Dtos.ML
{
    public class DietInputDto
    {
        public double gender { get; set; }
        public double age { get; set; }
        public double height_cm { get; set; }
        public double weight_kg { get; set; }
        public double bodyfat_percent { get; set; }
        public double activity_level { get; set; }
        public double goal_type { get; set; }
        public double goal_delta_kg { get; set; }
        public double goal_time_weeks { get; set; }
    }
}
