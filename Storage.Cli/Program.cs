// See https://aka.ms/new-console-template for more information


using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Storage.GoogleDrive.Service;


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

    var credentialsPath = Environment.GetEnvironmentVariable("GOOGLEDRIVE_CREDENTIALS_PATH");

    if (string.IsNullOrWhiteSpace(credentialsPath))
    {
        Console.WriteLine("Please set env: GOOGLEDRIVE_CREDENTIALS_PATH");
        return 0;
    }

    await using var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read);

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

    var search = new Search(service);


    var cancellationTokenSource = new CancellationTokenSource();
    Console.CancelKeyPress += delegate {
        cancellationTokenSource.Cancel();
    };

    await foreach (var filesChunk in search.List(searchValue, cancellationTokenSource.Token))
        foreach (var fileMetaData in filesChunk)
            Console.WriteLine(fileMetaData.Name);

    return 1;
}
catch (FileNotFoundException e)
{
    Console.WriteLine(e.Message);
    return 0;
}