using System.Windows;
using Cloud_Storage_desktop.Logic;

namespace Cloud_Storage_desktop;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private ServiceOperator Operator;

    public MainWindow()
    {
        InitializeComponent();
        Operator = new ServiceOperator();
        OnSerivceUdpate();
    }

    #region Serivice

    private void OnSerivceUdpate()
    {
        bool doesExist = Operator.Exist;
        bool isRunning = Operator.Exist ? Operator.IsServiceRunning() : false;
        Button_startStop.IsEnabled = doesExist;
        Button_createDestroy.Content = doesExist ? "Destroy" : "Create";
        Button_startStop.Content = isRunning ? "Stop" : "Start";
        Label_serviceStatus.Content = doesExist ? (isRunning ? "Running" : "Stopped") : "Not Exist";
    }

    private void CreateService()
    {
        try
        {
            Operator.CreateService();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void DestroyService()
    {
        try
        {
            Operator.DeleteService();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void StopService()
    {
        try
        {
            Operator.StopService();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void StartService()
    {
        try
        {
            Operator.StartService();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Button_createDestroy_Click(object sender, RoutedEventArgs e)
    {
        Button_createDestroy.IsEnabled = false;

        if (Operator.Exist)
        {
            DestroyService();
        }
        else
        {
            CreateService();
        }
        Button_createDestroy.IsEnabled = true;
        OnSerivceUdpate();
    }

    private void Button_startStop_Click(object sender, RoutedEventArgs e)
    {
        Button_createDestroy.IsEnabled = false;
        Button_startStop.IsEnabled = false;

        if (Operator.IsServiceRunning())
        {
            StopService();
        }
        else
        {
            StartService();
        }

        Button_createDestroy.IsEnabled = true;
        Button_startStop.IsEnabled = true;
        OnSerivceUdpate();
    }

    #endregion
}
