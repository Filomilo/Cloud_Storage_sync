using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using Cloud_Storage_Desktop_lib;
using Lombok.NET;

namespace ConsoleCloudDriveSync
{
    [AllArgsConstructor]
    partial class Operation
    {
        public Operation(string v, Action value)
        {
            Description = v;
            Function = value;
        }

        public string Description { get; set; }
        public Action Function { get; set; }
    }

    class Program
    {
        static  void Main(string[] args)
        {
            Thread.Sleep(100);
            Console.WriteLine("Start");
            if (!ServerConnectionEstablish()) return;
            AuthorizeConnectino();
            ConfigurationLoop();

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

        private static void ConfigurationLoop()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine($"Current configuration:\n {{\n {CloudDriveSyncSystem.Instance.Configuration}\n}}");
                Choice("What would you like to do: ", new Dictionary<char, Operation>()
                {
                    { 'Q', new Operation("QUIT",() => { Console.WriteLine("QUIT"); })},
                    { 'L', new Operation("QUIT",() => { Console.WriteLine("Logut"); })},
                });
            }
        }

        private static void Choice(String quiestion, Dictionary<char, Operation> choices)
        {

            Console.WriteLine(quiestion+"\n");
            foreach (var VARIABLE in choices)
            {
                Console.WriteLine($"[{VARIABLE.Key}] -- {VARIABLE.Value.Description}");
            }

            while (true)
            {
                char choice = Console.ReadKey().KeyChar;

                if (choices.ContainsKey(choice))
                {
                      choices[choice].Function.Invoke();
                break;
                }
       Console.WriteLine("Not valid choice");
             
            }
        
        }

    }
}
