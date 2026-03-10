using AccountingApp.Views;

namespace AccountingApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("TransactionFormPage", typeof(TransactionFormPage));
        Routing.RegisterRoute("CategoryListPage", typeof(CategoryListPage));
        Routing.RegisterRoute("CategoryFormPage", typeof(CategoryFormPage));
    }
}
