namespace AccountingApp.Tests;

public class IosMediaPermissionPlistTests
{
    [Fact]
    public void Ios_info_plist_declares_camera_and_photo_library_usage_descriptions()
    {
        var plist = File.ReadAllText(Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/Platforms/iOS/Info.plist")));

        Assert.Contains("<key>NSCameraUsageDescription</key>", plist);
        Assert.Contains("<key>NSPhotoLibraryUsageDescription</key>", plist);
        Assert.Contains("<key>NSPhotoLibraryAddUsageDescription</key>", plist);
    }
}
