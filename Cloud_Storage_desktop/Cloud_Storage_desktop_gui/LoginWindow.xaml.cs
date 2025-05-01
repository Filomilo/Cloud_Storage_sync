using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Cloud_Storage_Desktop_lib.Interfaces;

namespace Cloud_Storage_desktop
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private IServerConnection ServerConnection;

        public LoginWindow(IServerConnection serverConnection)
        {
            InitializeComponent();
            ServerConnection = serverConnection;
        }

        private void Button_login_Click(object sender, RoutedEventArgs e)
        {
            String email = TextBoc_Email.Text;
            String password = TextBox_Passwor.Password;
            try
            {
                ServerConnection.login(email, password);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void Login(IServerConnection serverConnection)
        {
            LoginWindow loginWindow = new LoginWindow(serverConnection);
            loginWindow.ShowDialog();
        }
    }
}
