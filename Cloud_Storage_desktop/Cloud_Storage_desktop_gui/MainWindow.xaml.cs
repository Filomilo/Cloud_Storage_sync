using System.Windows;
using System.Windows.Controls;
using Cloud_Storage_Common;
using Cloud_Storage_Desktop_lib;
using Cloud_Storage_Desktop_lib.Interfaces;
using Cloud_Storage_Desktop_lib.Services;
using Cloud_Storage_desktop.Logic;
using Microsoft.Win32;

namespace Cloud_Storage_desktop;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private ServiceOperator Operator;
    private Configuration EditedConfig;
    private IConfiguration Config;
    private IServerConnection ServerConnection;
    private ICredentialManager CredentialManager;
    private FileLogWatcher FileLogWatcher;

    public MainWindow()
    {
        InitializeComponent();
        Operator = new ServiceOperator();
        OnSerivceUdpate();
        EditedConfig = (Configuration?)Configuration.InitConfig();
        CredentialManager = new CredentialManager();
        EditedConfig.LoadConfiguration();
        OnConfigChanged();
        OnConfigSaved();
        InitLogs();
    }

    #region Logs

    private void InitLogs()
    {
        TextBoc_logs.Text = "";
        FileLogWatcher = new FileLogWatcher(CloudDriveLogging.Instance.getLogFilePath());
        FileLogWatcher.NewLinesAddedHandler += AddLogText;
        FileLogWatcher.StartWatching();
    }

    private void AddLogText(string newConetnt)
    {
        Dispatcher.InvokeAsync(() =>
        {
            TextBoc_logs.Text += newConetnt;
        });
        ;
    }

    private void TextBoc_logs_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        Dispatcher.InvokeAsync(() =>
        {
            ScrolViewr_logs.UpdateLayout();
            ScrolViewr_logs.ScrollToEnd();
            TextBoc_logs.ScrollToEnd();
        });
    }

    #endregion

    #region Server Connction

    private void OnConfigSaved()
    {
        Config = new Configuration();
        Config.LoadConfiguration();
        try
        {
            ServerConnection = new ServerConnection(
                Config.ApiUrl,
                this.CredentialManager,
                new NullWebSocket()
            );
            OnConnectionStateChange(true);
            ServerConnection.ConnectionChangeHandler += OnConnectionStateChange;
        }
        catch (Exception ex)
        {
            OnConnectionStateChange(false);
        }
    }

    private void OnConnectionStateChange(bool isConnected)
    {
        if (!isConnected)
        {
            Label_ConnectionStatus.Content = "Not Connected";
            Button_Login.IsEnabled = false;
            Button_Logout.Visibility = Visibility.Collapsed;
            Button_Register.IsEnabled = false;
        }
        else
        {
            Label_ConnectionStatus.Content = "Connected";
            if (ServerConnection.CheckIfAuthirized())
            {
                Button_Logout.Visibility = Visibility.Visible;
                Button_Register.Visibility = Visibility.Collapsed;
                Button_Login.Visibility = Visibility.Collapsed;
                Label_ConnectionStatus.Content = CredentialManager.GetEmail();
            }
            else
            {
                Button_Logout.Visibility = Visibility.Collapsed;
                Button_Register.Visibility = Visibility.Visible;
                Button_Login.Visibility = Visibility.Visible;
                Label_ConnectionStatus.Content = "Not Authirized";
            }
        }
    }

    #endregion



    #region Serivice

    private void ValidateSavedConfig()
    {
        Configuration config = new Configuration();

        config.LoadConfiguration();
        config.ValidateConfiguration();
    }

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
            ValidateSavedConfig();
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

    #region Configuraiton


    private void OnConfigChanged()
    {
        this.TextBox_apiUrl.Text = EditedConfig.ApiUrl;
        this.TextBox_maxFileSync.Text = EditedConfig.MaxStimulationsFileSync.ToString();
        this.Label_Location.Content = EditedConfig.StorageLocation;
    }

    private void Button_Save_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            EditedConfig.ApiUrl = this.TextBox_apiUrl.Text;
            EditedConfig.MaxStimulationsFileSync = int.Parse(this.TextBox_maxFileSync.Text);
            EditedConfig.StorageLocation = this.Label_Location.Content?.ToString();
            EditedConfig.ValidateConfiguration();
            EditedConfig.SaveConfiguration();
            OnConfigSaved();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Button_changelocation_Click(object sender, RoutedEventArgs e)
    {
        var folderDialog = new OpenFolderDialog { };
        if (folderDialog.ShowDialog() == true)
        {
            EditedConfig.StorageLocation = folderDialog.FolderName;
        }

        OnConfigChanged();
    }
    #endregion

    private void Button_Logout_Click(object sender, RoutedEventArgs e)
    {
        ServerConnection.Logout();
        OnConnectionStateChange(true);
    }

    private void Button_Login_Click(object sender, RoutedEventArgs e)
    {
        LoginWindow.Login(ServerConnection);
    }

    private void Button_Register_Click(object sender, RoutedEventArgs e)
    {
        RegisterWindow.Register(ServerConnection);
    }
}
