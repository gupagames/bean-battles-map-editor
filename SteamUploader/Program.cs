using Steamworks;
using System;
using System.Threading.Tasks;

namespace BeanBattlesMapEditorSteamUploader
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Environment.ExitCode = 1;

            bool success = SteamAPI.Init();

            Console.WriteLine("Steam Init: " + success);

            if (!success) return;

            Console.WriteLine("Continuing as : " + SteamFriends.GetPersonaName());

            if (args.Length == 0)
            { Console.WriteLine("Missing path arg");  return;}

            string path = args[0];

            Task<bool> uploadTask = SteamWorkshopManager.PublishMapAsync(path);

            while (!uploadTask.IsCompleted)
            { SteamAPI.RunCallbacks(); await Task.Delay(50); }

            bool result = await uploadTask;

            SteamAPI.Shutdown();

            await Task.Delay(1000);

            Environment.ExitCode = result ? 0 : 1;

            if (!result)
            {
                Console.WriteLine("An issue occurred, press any key to exit...");
                Console.ReadKey();
            }
        }
    }
}