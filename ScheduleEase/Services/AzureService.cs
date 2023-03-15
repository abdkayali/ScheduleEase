

using Azure.AI.FormRecognizer.DocumentAnalysis;
using Azure;

namespace ScheduleEase.Services
{
    public class AzureService
    {
        static string endpoint = "https://timetable.cognitiveservices.azure.com/";
        static string key = "fe12a19d0c404e44923d2258cdd9160c";
        public static async Task<AnalyzeResult> AnalyzeImage(Stream documentStream)
        {
            AzureKeyCredential credential = new AzureKeyCredential(key);
            DocumentAnalysisClient client = new DocumentAnalysisClient(new Uri(endpoint), credential);

            AnalyzeDocumentOperation operation = await client.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-layout", documentStream);

            AnalyzeResult result = operation.Value;
            return result;

        }
    }
}
