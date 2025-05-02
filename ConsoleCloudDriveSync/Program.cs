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
        static void Main(string[] args)
        {
            Thread.Sleep(100);
            Console.WriteLine("Start");
            if (!ServerConnectionEstablish())
                return;
            //AuthorizeConnectino();

            ConfigurationLoop();

            Console.ReadLine();
        }

        private static bool Login()
        {
            //Console.Clear();
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
                return false;
            }
            Console.WriteLine("Sucessful login");
            return true;
        }

        private static bool Registration()
        {
            //Console.Clear();
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
                return false;
            }
            Console.WriteLine("Sucessful registration");
            return true;
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
                //Console.Clear();
                Console.WriteLine(
                    $"Current configuration:\n {{\n {CloudDriveSyncSystem.Instance.Configuration}\n}}"
                );
                Dictionary<char, Operation> choices = getAvaliabeOpearions();

                Choice("What would you like to do: ", choices);
            }
        }

        private static Dictionary<char, Operation> getAvaliabeOpearions()
        {
            Dictionary<char, Operation> choices = new Dictionary<char, Operation>()
            {
                {
                    'Q',
                    new Operation(
                        "QUIT",
                        () =>
                        {
                            System.Environment.Exit(0);
                        }
                    )
                },
            };

            if (!CloudDriveSyncSystem.Instance.ServerConnection.CheckIfAuthirized())
            {
                choices.Add(
                    'L',
                    new Operation(
                        "Login",
                        () =>
                        {
                            RepeatAction(Login);
                        }
                    )
                );
                choices.Add(
                    'R',
                    new Operation(
                        "Register",
                        () =>
                        {
                            RepeatAction(Registration);
                        }
                    )
                );
                return choices;
            }
            choices.Add(
                'C',
                new Operation(
                    "Configure path to be sync",
                    () =>
                    {
                        RepeatAction(SetSyncPath);
                    }
                )
            );
            choices.Add(
                'L',
                new Operation(
                    "Logout",
                    () =>
                    {
                        CloudDriveSyncSystem.Instance.ServerConnection.Logout();
                    }
                )
            );

            //if (CloudDriveSyncSystem.Instance.Configuration.StorageLocation.Length > 0)
            //{
            //    choices.Add(
            //        'P',
            //        new Operation(
            //            "Sync",
            //            () =>
            //            {
            //                CloudDriveSyncSystem.Instance.SyncFiles();
            //            }
            //        )
            //    );
            //}

            return choices;
        }

        private static bool SetSyncPath()
        {
            Console.WriteLine("Provide directory path to sync: ");
            string path = Console.ReadLine();

            if (Directory.Exists(path))
            {
                CloudDriveSyncSystem.Instance.Configuration.StorageLocation = (path);

                CloudDriveSyncSystem.Instance.Configuration.SaveConfiguration();
            }
            else
            {
                Console.WriteLine("Path is not valid");
                return false;
            }

            return true;
        }

        private static void Choice(String quiestion, Dictionary<char, Operation> choices)
        {
            Console.WriteLine(quiestion + "\n");
            foreach (var VARIABLE in choices)
            {
                Console.WriteLine($"[{VARIABLE.Key}] -- {VARIABLE.Value.Description}");
            }

            while (true)
            {
                char choice = Char.ToUpper(Console.ReadKey().KeyChar);

                if (choices.ContainsKey(choice))
                {
                    choices[choice].Function.Invoke();
                    break;
                }
                Console.WriteLine("\nNot valid choice");
            }
        }

        private static void RepeatAction(Func<bool> func)
        {
            bool res = false;
            bool shouldContinue = true;
            do
            {
                res = func.Invoke();
                if (res == false)
                {
                    Choice(
                        "Would like to try again",
                        new Dictionary<char, Operation>()
                        {
                            {
                                'R',
                                new Operation(
                                    "Return",
                                    () =>
                                    {
                                        shouldContinue = false;
                                    }
                                )
                            },
                            {
                                'T',
                                new Operation(
                                    "Try again",
                                    () =>
                                    {
                                        shouldContinue = true;
                                    }
                                )
                            },
                        }
                    );
                }
                else
                {
                    shouldContinue = false;
                }
            } while (shouldContinue);
        }
    }
}
