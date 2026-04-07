namespace AccountingApp.Views;

[QueryProperty(nameof(ImageRelativePath), "path")]
public partial class TransactionImageViewerPage : ContentPage
{
    public TransactionImageViewerPage()
    {
        InitializeComponent();
    }

    public string? ImageRelativePath
    {
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                ViewerImage.Source = null;
                return;
            }

            var relativePath = Uri.UnescapeDataString(value);
            var absolutePath = Path.Combine(FileSystem.AppDataDirectory, relativePath.Replace('/', Path.DirectorySeparatorChar));
            ViewerImage.Source = File.Exists(absolutePath)
                ? ImageSource.FromFile(absolutePath)
                : null;
        }
    }
}
