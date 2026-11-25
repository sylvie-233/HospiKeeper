using HospiKeeper.App.Views;

namespace HospiKeeper.App;

public partial class AppShell : Shell
{
    public AppShell(
        DashboardPage dashboard,
        RoomsPage rooms,
        ReservationsPage reservations,
        StaysPage stays,
        MinePage mine)
    {
        InitializeComponent();

        DashboardTab.Content = dashboard;
        RoomsTab.Content = rooms;
        ReservationsTab.Content = reservations;
        StaysTab.Content = stays;
        MineTab.Content = mine;
    }
}
