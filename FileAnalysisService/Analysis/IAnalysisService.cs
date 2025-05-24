namespace FileAnalisysService;
public interface IAnalysisService
{
    AnalysisResult PlagiatAnalyze(Guid fileId);
    WordAnalysisResult WordAnalyze(Guid fileId);
    
    
}