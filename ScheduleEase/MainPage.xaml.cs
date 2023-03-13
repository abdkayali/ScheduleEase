using Microsoft.Graph.Models;
using System.Diagnostics;
using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using ScheduleEase.Models;
using System.Globalization;

namespace ScheduleEase;

public partial class MainPage : ContentPage
{
    private GraphService graphService;

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

                if(await DisplayAlert("Alert", text, "Add To Calendar", "Cancel"))
                {
                    if (graphService == null)
                    {
                        graphService = new GraphService();
                    }
                    foreach (var item in sessions)
                    {
                        await AddSessionToCalendar(item);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }

    private async void Add_Event(object sender, EventArgs e)
    {
        if (graphService == null)
        {
            graphService = new GraphService();
        }
        
       await graphService.AddEventToCalendar("Pathology event", 3, 10, 10, 0, 2,"this is a body");
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
        sessions.Clear();
        for (int i = 0; i < result.Tables.Count; i++)
        {
            DocumentTable table = result.Tables[i];
            DateTime date = new DateTime();
            bool first_date = true;
            foreach (DocumentTableCell cell in table.Cells)
            {

                if(cell.ColumnIndex == 0 && cell.RowIndex != 0)
                {
                    try
                    {
                        string dateLiteral = cell.Content.Substring(cell.Content.IndexOf(" ") + 1).Replace(" ", "");
                        if(!first_date)
                        {
                            if(DateTime.ParseExact(dateLiteral, "dd/MM/yyyy", CultureInfo.CurrentCulture) != date.AddDays(1))
                            {
                                DisplayAlert("Error", "There was an error scanning your timetable. Please make sure the picture is in good format and quality", "Try again");
                                break;
                            }
                        }
                        first_date = false;
                        date = DateTime.ParseExact(dateLiteral, "dd/MM/yyyy", CultureInfo.CurrentCulture);
                        continue;
                    }
                    catch(Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        date = date.AddDays(1);
                    }
                }
                if(cell.ColumnIndex > 1 && cell.RowIndex > 0)
                {
                    DateTime startDate = date.AddHours(12 + cell.ColumnIndex);
                    if(String.IsNullOrEmpty(cell.Content))
                        sessions.Add(new Session
                        {
                            Name = GetSessionName(cell.Content),
                            Professor = GetProfessorName(cell.Content),
                            StartTime = startDate,
                            EndTime = startDate.AddHours(cell.ColumnSpan),
                        });
                }
                
            }
        }
    }
    string GetProfessorName(string content)
    {
        if (content.Contains("Pr"))
        {
            return "Pr" + content.Split("Pr")[1];
        }
        if (content.Contains("Dr"))
        {
            return "Dr" + content.Split("Dr")[1];
        }
        return String.Empty;
    }
    string GetSessionName(string content)
    {
        if (content.Contains("Pr"))
        {
            content = content.Split("Pr")[0];
        }
        else if (content.Contains("Dr"))
        {
            content = content.Split("Dr")[0];
        }
        return content.Trim();
    }
    private async Task AddSessionToCalendar(Session session)
    {
        try
        {
            if (graphService == null)
            {
                graphService = new GraphService();
            }
            string timezone = "W. Central Africa Standard Time";
            await graphService.AddEventToCalendar(session.Name, new DateTimeTimeZone { DateTime = session.StartTime.ToString("o"), TimeZone = timezone }, new DateTimeTimeZone { DateTime = session.EndTime.ToString("o"), TimeZone = timezone }, session.ToString());
        }
        catch(Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }
}

