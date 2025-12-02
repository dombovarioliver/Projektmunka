namespace DiplomaFit.Api.Application.Interfaces
{
    public interface IImportService
    {
        Task ImportPlansAndCasesAsync(string planCsvPath, string caseCsvPath);
        Task ImportFoodsAsync(string csvPath);
        Task ImportExercisesAsync(string exercisesCsvPath);
    }
}
