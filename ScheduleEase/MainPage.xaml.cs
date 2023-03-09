using Microsoft.Graph.Models;
using System.Diagnostics;
using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using ScheduleEase.Models;
using static System.Net.Mime.MediaTypeNames;
using System.Globalization;

namespace ScheduleEase;

public partial class MainPage : ContentPage
{
    int count = 0;
    private GraphService graphService;
    private User user;

    string endpoint = "https://timetable.cognitiveservices.azure.com/";
    string key = "fe12a19d0c404e44923d2258cdd9160c";

    List<Session> sessions = new List<Session>();
    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnScanButtonClicked(object sender, EventArgs e)
    {
        try
        {
            FileResult photo = await MediaPicker.Default.PickPhotoAsync();

            if (photo != null)
            {

                var sourceStream = await photo.OpenReadAsync();

                // save the file into local storage
                string localFilePath = Path.Combine(FileSystem.CacheDirectory, photo.FileName);

                FileStream localFileStream = File.OpenWrite(localFilePath);

                await sourceStream.CopyToAsync(localFileStream);

                localFileStream.Close();
                image.Source = ImageSource.FromFile(localFilePath);

                AnalyzeResult result = await AnalyzeImage(File.OpenRead(localFilePath));

                AddSessions(result);

                string text = $"";
                foreach (var item in sessions)
                {
                    text += item.ToString() + "\n";
                }
                //string text = $"";
                //foreach (var item in result.Paragraphs)
                //{
                //    text += item.Content;
                //    text += "\n";
                //}
                await DisplayAlert("Alert", text, "OK");

                //text = $"";
                //for (int i = 0; i < result.Tables.Count; i++)
                //{
                //    Debug.WriteLine($"table {i}");
                //    DocumentTable table = result.Tables[i];
                //    text += $"  Table {i} has {table.RowCount} rows and {table.ColumnCount} columns.";

                //    foreach (DocumentTableCell cell in table.Cells)
                //    {
                //        text += $"    Cell ({cell.RowIndex}, {cell.ColumnIndex}) has content: '{cell.Content}'. And ColumnSpan: '{cell.ColumnSpan}' \n";
                //    }
                //}
                //await DisplayAlert("Alert", text    , "OK");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
        count++;

       
    }

    private async void Add_Event(object sender, EventArgs e)
    {
        if (graphService == null)
        {
            graphService = new GraphService();
        }
        
        user = await graphService.AddEventToCalendar("Pathology event", 3, 10, 10, 0, 2,"this is a body");
       HelloLabel.Text = $"Done";
    }

    private async Task<AnalyzeResult> AnalyzeImage(Stream documentStream)
    {
        AzureKeyCredential credential = new AzureKeyCredential(key);
        DocumentAnalysisClient client = new DocumentAnalysisClient(new Uri(endpoint), credential);

        //// sample document
        Uri fileUri = new Uri("https://scontent.ftun2-2.fna.fbcdn.net/v/t1.15752-9/334913916_759827175365600_3302611857041079055_n.jpg?_nc_cat=110&ccb=1-7&_nc_sid=ae9488&_nc_ohc=-hLlZu0ENiIAX_14tRQ&_nc_ht=scontent.ftun2-2.fna&oh=03_AdRYXxzfY11ArFW4Dz__QBUN3aoUI1-0Ti2CBw15SNZODA&oe=642F027D");

        AnalyzeDocumentOperation operation = await client.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-layout",documentStream);

        AnalyzeResult result = operation.Value;
        return result;
        
    }
    void AddSessions(AnalyzeResult result)
    {
        for (int i = 0; i < result.Tables.Count; i++)
        {
            DocumentTable table = result.Tables[i];
            DateTime date = new DateTime();

            foreach (DocumentTableCell cell in table.Cells)
            {

                if(cell.ColumnIndex == 0 && cell.RowIndex != 0)
                {
                    try
                    {
                        string dateLiteral = cell.Content.Substring(cell.Content.IndexOf(" ") + 1);
                        date = DateTime.ParseExact(dateLiteral, "dd/MM/yyyy", CultureInfo.CurrentCulture);
                        continue;
                    }
                    catch(Exception ex)
                    {

                    }
                }
                if(cell.ColumnIndex > 1 && cell.RowIndex > 0)
                {
                    DateTime startDate = date.AddHours(12 + cell.ColumnIndex);

                    sessions.Add(new Session
                    {
                        Name = cell.Content,
                        StartTime = startDate,
                        EndTime = startDate.AddHours(cell.ColumnSpan),
                    });
                }
                
            }
        }
    }
}

