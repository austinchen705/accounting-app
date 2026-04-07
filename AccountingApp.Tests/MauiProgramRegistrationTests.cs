namespace AccountingApp.Tests;

public class MauiProgramRegistrationTests
{
    [Fact]
    public void MauiProgram_registers_transaction_image_service_for_transaction_form_navigation()
    {
        var code = File.ReadAllText(Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/MauiProgram.cs")));

        Assert.Contains("builder.Services.AddSingleton<TransactionImageService>();", code);
        Assert.Contains("builder.Services.AddTransient<TransactionFormViewModel>();", code);
    }
}
