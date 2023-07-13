using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

class Program
{

    private static int hits = 0;

    static void Main(string[] args)
    {
        bool exit = false;

        while (!exit)
        {
            hits = 0;
            Console.Write("Enter folder path containing dump: ");
            string folderPath = Console.ReadLine();

            Console.Write("Enter string to search for: ");
            string searchString = Console.ReadLine();

            List<string> results = new List<string>();
            SearchDirectory(folderPath, searchString, results);

            if (results.Count > 0)
            {
                string json = JsonConvert.SerializeObject(results, Formatting.Indented);
                string modifiedInput = searchString.Replace("-", "");
                modifiedInput = searchString.Replace(":", "");
                modifiedInput = searchString.Replace("%", "");
                modifiedInput = searchString.Replace(";", "");
                modifiedInput = searchString.Replace(".", "");
                modifiedInput = searchString.Replace(",", "");
                File.WriteAllText(modifiedInput + ".txt", json);
                Console.WriteLine("");
                Console.WriteLine($"Results written to { modifiedInput }.txt.");
            }
            else
            {
                Console.WriteLine("String not found in any files.");
            }

            Console.Write("Search again? (Y/N): ");
            string input = Console.ReadLine();
            if (input.ToLower() != "y")
            {
                exit = true;
            }
            Console.Clear();
        }
    }

    static void SearchDirectory(string path, string searchString, List<string> results)
    {
        try
        {

            List<string> dirResults = new List<string>();

            // Process files in parallel
            // Console.WriteLine("Running parallel task");
            Parallel.ForEach(Directory.GetFiles(path), filePath =>
            {
                Console.SetCursorPosition(0, Console.CursorTop); // move cursor to beginning of initial line
                Console.Write("\rHits : " + hits.ToString());
                // Console.WriteLine("Checking for lua files");
                if (filePath.EndsWith(".lua") || filePath.EndsWith(".txt") || filePath.EndsWith(".json") || filePath.EndsWith(".xml") || filePath.EndsWith(".cfg") || filePath.EndsWith(".config") || filePath.EndsWith(".meta") || filePath.EndsWith(".js") || filePath.EndsWith(".html") || filePath.EndsWith(".css"))
                {
                    // Console.WriteLine("Reading lines of " + filePath);
                    string[] lines = File.ReadAllLines(filePath);
                    string line = Array.Find(lines, l => l.Contains(searchString, StringComparison.OrdinalIgnoreCase));

                    if (line != null)
                    {
                        // Console.WriteLine("Syncing results of " + filePath);
                        lock (dirResults) // Synchronize access to dirResults
                        {
                            // Console.WriteLine("Adding results of " + filePath);
                            dirResults.Add(line + " {File path: " + filePath + " }");
                            hits++;
                        }
                    }
                }
            });

            foreach (string subdirectoryPath in Directory.GetDirectories(path))
            {
                SearchDirectory(subdirectoryPath, searchString, dirResults);
            }

            if (dirResults.Count > 0)
            {
                results.AddRange(dirResults);
            }
        }
        catch (Exception ex)
        {
            // Console.WriteLine("Error searching directory: " + ex.Message);
        }
    }
}
