using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using IniParser;
using IniParser.Model;

class Program
{
    static void Main(string[] args)
    {
        // Paths for files
        string configPath = "config.ini";
        string readmePath = "readme.txt";
        string indexPath = "index.html";

        // Check if the configuration file exists
        if (!File.Exists(configPath))
        {
            Console.WriteLine("Configuration file not found. Creating default configuration...");

            var parser = new FileIniDataParser();
            IniData defaultConfig = new IniData();
            defaultConfig["Server"]["IP"] = "127.0.0.1";
            defaultConfig["Server"]["Port"] = "8080";

            try
            {
                parser.WriteFile(configPath, defaultConfig);
                Console.WriteLine("Default configuration created: config.ini");
                Console.WriteLine("Credits: Developed by [Your Name]");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating the configuration file: {ex.Message}");
                return;
            }
        }

        // Check if the README file exists
        if (!File.Exists(readmePath))
        {
            Console.WriteLine("README file not found. Creating README...");

            string readmeContent = "Welcome to the web server!\n\n" +
                                   "This is a simple web server created in C#.\n" +
                                   "Configuration file: config.ini\n" +
                                   "Modify IP and Port in the config.ini to customize the server.\n" +
                                   "The index.html file is used as the main page of the server.\n\n" +
                                   "Credits: Developed by [Your Name]\n";

            try
            {
                File.WriteAllText(readmePath, readmeContent);
                Console.WriteLine("README created: readme.txt");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating the README file: {ex.Message}");
                return;
            }
        }

        // Check if the index.html file exists
        if (!File.Exists(indexPath))
        {
            Console.WriteLine("Index file not found. Creating default index...");

            string indexContent = "<html><body><h1>Welcome to the web server!</h1><p>This is the default index page. Edit the index.html file to change the content.</p></body></html>";

            try
            {
                File.WriteAllText(indexPath, indexContent);
                Console.WriteLine("Index file created: index.html");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating the index file: {ex.Message}");
                return;
            }
        }

        // Load configuration from the .ini file using IniParser
        var parserForConfig = new FileIniDataParser();
        IniData config;

        try
        {
            config = parserForConfig.ReadFile(configPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading the configuration file: {ex.Message}");
            return;
        }

        string ip = config["Server"]["IP"] ?? "127.0.0.1";
        int port = int.TryParse(config["Server"]["Port"], out int parsedPort) ? parsedPort : 8080;

        HttpListener listener = new HttpListener();
        string url = $"http://{ip}:{port}/";
        listener.Prefixes.Add(url);

        bool stopRequested = false;

        try
        {
            listener.Start();
            Console.WriteLine($"Web server started at {url}");
            Console.WriteLine("Press any key to stop the server...");

            // Separate task for processing requests
            Task listenerTask = Task.Run(() =>
            {
                while (listener.IsListening && !stopRequested)
                {
                    try
                    {
                        var context = listener.GetContext(); // Waits for incoming requests

                        // Check if index.html can be read
                        string responseString;
                        if (File.Exists(indexPath))
                        {
                            responseString = File.ReadAllText(indexPath);
                        }
                        else
                        {
                            responseString = "<html><body><h1>File not found</h1><p>The index.html file has been deleted or is unavailable.</p></body></html>";
                        }

                        byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                        context.Response.ContentLength64 = buffer.Length;
                        context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                        context.Response.OutputStream.Close();
                    }
                    catch (HttpListenerException)
                    {
                        // Listener has been stopped
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing a request: {ex.Message}");
                    }
                }
            });

            Console.ReadKey(); // Wait for user input
            stopRequested = true;
            listener.Stop();
            listenerTask.Wait();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            if (listener.IsListening)
            {
                listener.Stop();
            }

            try
            {
                listener.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error closing the listener: {ex.Message}");
            }
        }
    }
}