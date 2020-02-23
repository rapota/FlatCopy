using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FlatCopy.Services;
using Microsoft.Extensions.Configuration;

namespace FlatCopy
{
    class Program
    {
        static void Main(string[] args)
        {
            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder()
                .AddCommandLine(args);
            IConfigurationRoot configuration = configurationBuilder.Build();

            string source = configuration["source"];
            string target = configuration["target"];

            if (string.IsNullOrEmpty(source))
            {
                Console.WriteLine("Wrong source folder.");
                return;
            }
            if (string.IsNullOrEmpty(target))
            {
                Console.WriteLine("Wrong target folder.");
                return;
            }

            if (!Directory.Exists(source))
            {
                Console.WriteLine("Source folder not found.");
            }
            if (!Directory.Exists(target))
            {
                Directory.CreateDirectory(target);
                Console.WriteLine("Created target folder {0}", target);
            }

            Parallel.ForEach(
                FlatFolderService.GetFileLinks(source, target, "*.*").Where(x => !File.Exists(x.Target)),
                fileLink =>
                {
                    File.Copy(fileLink.Source, fileLink.Target);
                });

            Console.WriteLine("Done!");
        }
    }
}
