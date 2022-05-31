﻿// See https://aka.ms/new-console-template for more information


using Dropbox.Api;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Storage.Common;
using DropboxSearch = Storage.Dropbox.Service.Search;
using GoogleDriveSearch = Storage.GoogleDrive.Service.Search;

if (args.Length == 0)
{
    Console.WriteLine("Please enter a search value");
    return 0;
}

var searchValue = args[0];


try
{
    string[] scopes = {DriveService.Scope.DriveReadonly};
    const string applicationName = "Storage";
    const string credPath = "tokens/token.json";

    var googleDriveCredentialsPath = Environment.GetEnvironmentVariable("GOOGLEDRIVE_CREDENTIALS_PATH");
    var dropboxAccessToken = Environment.GetEnvironmentVariable("DROPBOX_ACCESS_TOKEN");

    if (string.IsNullOrWhiteSpace(googleDriveCredentialsPath))
    {
        Console.WriteLine("Please set env: GOOGLEDRIVE_CREDENTIALS_PATH");
        return 0;
    }

    await using var stream = new FileStream(googleDriveCredentialsPath, FileMode.Open, FileAccess.Read);

    var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
        GoogleClientSecrets.FromStream(stream).Secrets,
        scopes,
        "user",
        CancellationToken.None,
        new FileDataStore(credPath, true)).Result;

    var service = new DriveService(new BaseClientService.Initializer
    {
        HttpClientInitializer = credential,
        ApplicationName = applicationName
    });

    var dropboxClient = new DropboxClient(dropboxAccessToken);
    var dropboxSearch = new DropboxSearch(dropboxClient);


    var googleDriveSearch = new GoogleDriveSearch(service);


    var cancellationTokenSource = new CancellationTokenSource();
    Console.CancelKeyPress += delegate { cancellationTokenSource.Cancel(); };

    var searchAggregator = new SearchAggregator(new IStorageSearch[] {dropboxSearch, googleDriveSearch});

    try
    {
        await foreach (var filesChunk in searchAggregator.List(searchValue, cancellationTokenSource.Token))
        {
            foreach (var fileMetaData in filesChunk)
            {
                Console.WriteLine($"{fileMetaData.Name} [{fileMetaData.StorageProviderName}]");
            }
        }
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        throw;
    }

    return 1;
}
catch (FileNotFoundException e)
{
    Console.WriteLine(e.Message);
    return 0;
}