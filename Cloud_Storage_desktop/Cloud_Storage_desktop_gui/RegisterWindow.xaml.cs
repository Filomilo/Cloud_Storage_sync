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
using Cloud_Storage_Desktop_lib;
using Cloud_Storage_Desktop_lib.Interfaces;

namespace Cloud_Storage_desktop
{
    /// <summary>
    /// Interaction logic for RegisterWindow.xaml
    /// </summary>
    public partial class RegisterWindow : Window
    {
        IServerConnection ServerConnection;

        public RegisterWindow(IServerConnection serverConnection)
        {
            InitializeComponent();
            ServerConnection = serverConnection;
        }

        private void Button_Register_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string email = TextBoc_Email.Text;
                string password = TextBox_Passwor.Password;
                string passwordRepeat = TextBox_RepeatPasswor.Password;

                if (password != passwordRepeat)
                {
                    throw new Exception("Pasword dont match");
                }
                ServerConnection.Register(email, password);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void Register(IServerConnection serverConnection)
        {
            RegisterWindow registerWindow = new RegisterWindow(serverConnection);
            registerWindow.ShowDialog();
        }
    }
}
