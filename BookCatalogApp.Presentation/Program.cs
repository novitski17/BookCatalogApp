using BookCatalogApp.Presentation.Services;
using Microsoft.Extensions.DependencyInjection;
using BookCatalogApp.Core.Interfaces;

namespace BookCatalogApp.Presentation
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            ServiceProvider serviceProvider = ServiceConfiguration.Configure();

            IBookService bookService = serviceProvider.GetService<IBookService>();

            await RunAsync(bookService);

            if (serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        private static async Task RunAsync(IBookService bookService)
        {
            while (true)
            {
                try
                {
                    string option = GetUserOption();
                    if (option.ToLower() == "exit")
                    {
                        break;
                    }

                    await HandleUserOptionAsync(option, bookService);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }

                Console.WriteLine(new string('-', 50));
            }
        }

        private static string GetUserOption()
        {
            Console.WriteLine("Select an option (type 'exit' to quit):");
            Console.WriteLine("1. Add Books from File");
            Console.WriteLine("2. Search Books");

            return Console.ReadLine();
        }

        private static async Task HandleUserOptionAsync(string option, IBookService bookService)
        {
            switch (option)
            {
                case "1":
                    string filePath = GetFilePathFromUser();
                    await bookService.AddBooksFromFileAsync(filePath);
                    break;
                case "2":
                    string outputDirectory = GetOutputDirectoryFromUser();
                    await bookService.SearchBooksAsync(outputDirectory);
                    break;
                default:
                    Console.WriteLine("Invalid option selected. Please try again.");
                    break;
            }
        }

        private static string GetFilePathFromUser()
        {
            Console.WriteLine("Enter the file path of the CSV file:");
            return Console.ReadLine();
        }

        private static string GetOutputDirectoryFromUser()
        {
            Console.WriteLine("Enter the directory where you want to save the file:");
            return Console.ReadLine();
        }
    }
}
