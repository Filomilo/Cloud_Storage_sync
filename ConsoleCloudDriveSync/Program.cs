using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Desktop_lib;

namespace ConsoleCloudDriveSync
{
    class Program
    {
        static  void Main(string[] args)
        {
            Thread.Sleep(100);
            Console.WriteLine("Start");
            if (!ServerConnectionEstablish()) return;
            AuthorizeConnectino();


            Console.ReadLine();
        }
        private static void AuthorizeConnectino()
        {
            while(!CloudDriveSyncSystem.Instance.ServerConnection.CheckIfAuthirized())
            {
                Console.WriteLine("Connection is not authorized please register or login");
                Console.Clear();
                Console.WriteLine("L - Login , R- register");
                string choice = Console.ReadLine();
                if (choice == "L")
                {
                    Login();
                }
                else if (choice == "R")
                {
                    Registration();
                }
                 else
                 {
                     continue;
                 }
            }

            Console.WriteLine("Connection Authorized");
        }

        private static void Login()
        {
            Console.Clear();
            Console.WriteLine("Provide email adress you would like to login with: ");
            string email = Console.ReadLine();

            Console.WriteLine("Provide password you would like to login with: ");
            string password = Console.ReadLine();
            try
            {
                CloudDriveSyncSystem.Instance.ServerConnection.login(email, password);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Coulnt login: {ex.Message}");
                return;
            }
            Console.WriteLine("Sucessful login");
        }

        private static void Registration()
        {
            Console.Clear();
            Console.WriteLine("Provide email adress you would like to register with: ");
            string email = Console.ReadLine();

            Console.WriteLine("Provide password you would like to register with: ");
            string password = Console.ReadLine();
            try
            {
                CloudDriveSyncSystem.Instance.ServerConnection.Register(email, password);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Coulnt register: {ex.Message}");
                return;
            }
            Console.WriteLine("Sucessful registration");
        }


        private static bool ServerConnectionEstablish()
        {
            if (!CloudDriveSyncSystem.Instance.ServerConnection.CheckIfHelathy())
            {
                Console.WriteLine("Error couldnt establish connection with server");
                return false;
            }

            Console.WriteLine("Conneciotn with server established");
            return true;
        }
    }
}
