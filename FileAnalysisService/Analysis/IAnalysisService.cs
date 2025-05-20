namespace FileAnalisysService;
public interface IAnalysisService
{
    AnalysisResult PlagiatAnalyze(Guid fileId);
}