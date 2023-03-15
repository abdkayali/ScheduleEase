using Microsoft.Graph.Models;
using ScheduleEase.Services;
using System.Diagnostics;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using ScheduleEase.Models;
using System.Globalization;
using Microsoft.Identity.Client;
using ScheduleEase.Helpers;
using Microsoft.Datasync.Client;

namespace ScheduleEase;

public partial class MainPage : ContentPage
{
    private GraphService graphService;

    List<Session> sessions = new List<Session>();
    public MainPage()
    {
        InitializeComponent();
        
        //DisplayAlert("Salem",AppInfo.Current.PackageName,"Back");
    }
    public User user;
    private async void OnUploadButtonClicked(object sender, EventArgs e)
    {
        try
        {
            loadingText.Text = "Choose an image..";
            SwitchLoading();
            FileResult photo = await MediaPicker.Default.PickPhotoAsync();

            if (photo != null)
            {
                await ScanImage(photo);
            }
            SwitchLoading();

        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            loadingStack.IsVisible = false;
            backgroundBox.IsVisible = false;
        }
    }
    private async void OnScanButtonClicked(object sender, EventArgs e)
    {
        try
        {
            loadingText.Text = "Please take a picture..";
            SwitchLoading();
            FileResult photo = OperatingSystem.IsWindows() ? await CaptureAsync() : await MediaPicker.Default.CapturePhotoAsync();

            if (photo != null)
            {
                await ScanImage(photo);
            }
            SwitchLoading();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            loadingStack.IsVisible = false;
            backgroundBox.IsVisible = false;
        }
    }

    private async Task ScanImage(FileResult photo)
    {
        loadingText.Text = "Scanning Image";
        var sourceStream = await photo.OpenReadAsync();

        // save the file into local storage
        string localFilePath = Path.Combine(FileSystem.CacheDirectory, photo.FileName);

        FileStream localFileStream = File.OpenWrite(localFilePath);

        await sourceStream.CopyToAsync(localFileStream);

        localFileStream.Close();

        image.Source = ImageSource.FromFile(localFilePath);
        SemanticProperties.SetDescription(image, "Your timetable");

        AnalyzeResult result = await AzureService.AnalyzeImage(File.OpenRead(localFilePath));
        loadingText.Text = "Finishing";
        AddSessions(result);

        string text = $"";
        foreach (var item in sessions)
        {
            text += item.ToString() + "\n";
        }


        if (await DisplayAlert("Alert", text, "Add To Calendar", "Cancel"))
        {
            loadingText.Text = "Adding sessions to your calendar";

            if (graphService == null)
            {
                graphService = new GraphService();
            }
            foreach (var item in sessions)
            {
                await AddSessionToCalendar(item);
            }
            loadingText.Text = "Finishing...";
            DisplayAlert("Success", "Sessions added successfully to your calendar", "Close");
        }
    }

    void AddSessions(AnalyzeResult result)
    {
        sessions.Clear();
        for (int i = 0; i < result.Tables.Count; i++)
        {
            DocumentTable table = result.Tables[i];
            DateTime date = new DateTime();
            int multiplier = table.ColumnCount <= 4 ? 2 : 1;
            bool first_date = true;
            foreach (DocumentTableCell cell in table.Cells)
            {

                if (cell.ColumnIndex == 0 && cell.RowIndex != 0)
                {
                    try
                    {
                        string dateLiteral = cell.Content.Substring(cell.Content.IndexOf(" ") + 1).Replace(" ", "");
                        if (!first_date)
                        {
                            if (DateTime.ParseExact(dateLiteral, "dd/MM/yyyy", CultureInfo.CurrentCulture) != date.AddDays(1))
                            {
                                DisplayAlert("Error", "There was an error scanning your timetable. Please make sure the picture is in good format and quality", "Try again");
                                break;
                            }
                        }
                        first_date = false;
                        date = DateTime.ParseExact(dateLiteral, "dd/MM/yyyy", CultureInfo.CurrentCulture);
                        continue;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        date = date.AddDays(1);
                    }
                }
                if (cell.ColumnIndex > 1 && cell.RowIndex > 0)
                {
                    DateTime startDate = date.AddHours(12 + cell.ColumnIndex + (cell.ColumnIndex - 2) * (multiplier - 1));
                    
                    if (!String.IsNullOrEmpty(cell.Content))
                        sessions.Add(new Session
                        {
                            Name = GetSessionName(cell.Content),
                            Professor = GetProfessorName(cell.Content),
                            StartTime = startDate,
                            EndTime = startDate.AddHours(cell.ColumnSpan * multiplier),
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
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }
    public async Task<FileResult> CaptureAsync(MediaPickerOptions options = null, bool photo = true)
    {
        //var captureUi = new CameraCaptureUI(options);

        //var file = await captureUi.CaptureFileAsync();

        //if (file != null)
        //    return new FileResult(file.Path, file.ContentType);

        return null;
    }
    private void SwitchLoading()
    {
        loadingStack.IsVisible = !loadingStack.IsVisible;
        backgroundBox.IsVisible = !backgroundBox.IsVisible;
    }
    
}

