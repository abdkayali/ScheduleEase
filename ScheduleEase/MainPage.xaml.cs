using Microsoft.Graph.Models;
using System.Diagnostics;
using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;


namespace ScheduleEase;

public partial class MainPage : ContentPage
{
    int count = 0;
    private GraphService graphService;
    private User user;

    string endpoint = "https://timetable.cognitiveservices.azure.com/";
    string key = "fe12a19d0c404e44923d2258cdd9160c";
    string[] TimeTable;
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


                String text =$"";
                for (int i = 0; i < result.Tables.Count; i++)
                {
                    Debug.WriteLine($"table {i}");
                    DocumentTable table = result.Tables[i];
                    text += $"  Table {i} has {table.RowCount} rows and {table.ColumnCount} columns.";

                    foreach (DocumentTableCell cell in table.Cells)
                    {
                        text += $"    Cell ({cell.RowIndex}, {cell.ColumnIndex}) has content: '{cell.Content}'. \n";
                    }
                }
                await DisplayAlert("Alert", text, "OK");



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
}

