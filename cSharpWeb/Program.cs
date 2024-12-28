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

        // some Files
        string configPath = "config.ini";
        string readmePath = "readme.txt";
        string indexPath = "index.html";

        // Check if the config.ini File exists
        if(!File.Exists(configPath))
        {
            Console.WriteLine("Config File not found. default Config gets created.");

            var parser1 = new FileIniDataParser();
            IniData defaultConfig = new IniData();

            defaultConfig["Server"]["IP"] = "127.0.0.1";
            defaultConfig["Server"]["Port"] = "8080";

            try
            {
                parser1.WriteFile(configPath, defaultConfig);
                Console.WriteLine("Defaultconfig has been created!");
            } catch (Exception ex)
            {
                Console.WriteLine($"An Occurred Error while creating the Defaultconfig: {ex.ToString()}");
                return;
            }
        }

        // Check if the readme.txt file exists
        if (!File.Exists(readmePath))
        {
            Console.WriteLine("README-File not found. README-File gets created...");

            string readmeContent = "Welcome!\n\n" +
                                   "This is a Webserver and was created with C#.\n" +
                                   "You can Change the IPv4 and the Port in the config.ini File.\n\n" +
                                   "Credits: Developed with Heart by AustrianNoah\n";

            try
            {
                File.WriteAllText(readmePath, readmeContent);
                Console.WriteLine("README-File has been created!");
            } catch (Exception ex)
            {
                Console.WriteLine($"Error while creating the Readme File: {ex.Message}");
                return;
            }
        }

        // Check if the index.html File exists
        if (!File.Exists(indexPath))
        {
            Console.WriteLine("Index-Datei nicht gefunden. Erstelle Standard-Index...");

            string indexContent = "<html><body><h1>C# Webserver!</h1><p>Dies ist die Standard-Indexseite. Bearbeiten Sie die index.html-Datei, um den Inhalt zu ändern.</p></body></html>"; // some german spell o.O

            try
            {
                File.WriteAllText(indexPath, indexContent);
                Console.WriteLine("Index-Datei erstellt: index.html");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Erstellen der Index-Datei: {ex.Message}");
                return;
            }
        }

        // Loading the Configuration from the .ini File in the Folder with IniParser
        var parser = new FileIniDataParser();
        IniData config;

        try
        {
            config = parser.ReadFile("config.ini");
        } catch (Exception ex)
        {
            Console.WriteLine($"Error reading content from the Config: {ex.Message}");
            return;
        }

        string ipv4 = config["Server"]["IP"] ?? "127.0.0.1";
        int port = int.TryParse(config["Server"]["Port"], out int parsedPort) ? parsedPort : 8080;

        HttpListener listener = new HttpListener();
        string url = $"http://{ipv4}:{port}/";
        listener.Prefixes.Add(url);

        try
        {
            listener.Start();
            Console.WriteLine($"Webserver gestartet unter {url}");
            Console.WriteLine("Drücken Sie eine beliebige Taste, um den Server zu stoppen...");

            // Separate task for processing requests
            Task.Run(() =>
            {
                while (listener.IsListening)
                {
                    try
                    {
                        var context = listener.GetContext(); // Wait for request

                        // Check if the Index File can be accessed
                        string responseString;
                        if (File.Exists(indexPath))
                        {
                            responseString = File.ReadAllText(indexPath);
                        }
                        else
                        {
                            responseString = "<html><body><h1>Datei nicht gefunden</h1><p>Die index.html-Datei wurde gelöscht oder ist nicht verfügbar.</p></body></html>";
                        }

                        byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                        context.Response.ContentLength64 = buffer.Length;
                        context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                        context.Response.OutputStream.Close();
                    }
                    catch (HttpListenerException)
                    {
                        // Listener got stopped
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing a request: {ex.Message}");
                    }
                }
            });

            Console.ReadKey(); // Wait for Userinput
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
            listener.Close();
        }
    }
}