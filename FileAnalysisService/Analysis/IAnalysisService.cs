namespace FileAnalisysService;
public interface IAnalysisService
{
    AnalysisResult Analyze(Guid fileId);
}