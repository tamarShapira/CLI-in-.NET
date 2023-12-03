using Microsoft.VisualBasic.FileIO;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.IO;

namespace fib
{      public class Program
    {
        static void Main(string[] args)
        {
            bool isValid=false;
            var bundleCommand = new Command("bundle", "Bundle code files to a single file");
            var rspCommand = new Command("create-rsp", "create response file with recommended options");

            var rspOptionOutput = new Option<string>("--output", "file path");

            var bundleOptionOutput = new Option<FileInfo>("--output", "file path and name");
            var bundleOptionLanguage = new Option<string[]>("--language", "selected programming language")
            {
                IsRequired = true

            };
            var bundleOptionNote = new Option<bool>("--note", "include directory and name of code file");
            var bundleOptionSort = new Option<string>("--sort", "sort the writing of codefiles by order");
            var bundleOptionDeleteEmptyLines = new Option<bool>("--remove-empty-lines", "delete empty lines from code");
            var bundleOptionAuthor = new Option<string>("--author", "note the file's author");

            bundleOptionAuthor.AddAlias("-a");
            bundleOptionDeleteEmptyLines.AddAlias("-d");
            bundleOptionLanguage.AddAlias("-l");
            bundleOptionNote.AddAlias("-n");
            bundleOptionOutput.AddAlias("-o");
            bundleOptionSort.AddAlias("-s");
          
            bundleOptionSort.SetDefaultValue(string.Empty);

            bundleCommand.AddOption(bundleOptionOutput);  
            bundleCommand.AddOption(bundleOptionLanguage);  
            bundleCommand.AddOption(bundleOptionNote);
            bundleCommand.AddOption(bundleOptionSort);
            bundleCommand.AddOption(bundleOptionDeleteEmptyLines);
            bundleCommand.AddOption(bundleOptionAuthor);

            rspCommand.AddOption(rspOptionOutput);

            bundleCommand.SetHandler((output,language,note,sort,remove,author) => 
            {
                try
                {

                    string[] codeFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "*", System.IO.SearchOption.AllDirectories).
                    Where(file =>
                          !file.Contains("\\bin\\") &&
                          !file.Contains("\\obj\\") &&
                          !file.Contains("\\debug\\") &&
                          !file.Contains("\\release\\")).ToArray();

                    if (language.Contains("all"))
                    {
                        language = codeFiles.Select(file => Path.GetExtension(file).TrimStart('.')).Distinct().ToArray();
                    }

                    IEnumerable<string> sortedFiles;
                    if (bundleOptionSort is not null && sort.Equals("type"))
                    {
                        sortedFiles = codeFiles.OrderBy(file => Path.GetExtension(file));
                    }
                    else
                    {
                        sortedFiles = codeFiles;
                    }

                    using (StreamWriter writer = new StreamWriter(output.FullName))
                    {
                        if (author is not null)
                        {
                            writer.WriteLine("===============author===============");
                            writer.WriteLine($"name: {author}");
                        }

                        foreach (string codeFile in sortedFiles)
                        {
                            string fileExtension = Path.GetExtension(codeFile).TrimStart('.');


                            if (language.Contains(fileExtension))
                            {
                                isValid = true;
                                if (note)
                                {
                                    writer.WriteLine("===============note===============");
                                    writer.WriteLine($"directory: {Directory.GetCurrentDirectory()}");
                                    writer.WriteLine($"name: {Path.GetFileName(codeFile)}");
                                }
                                string fileContent = File.ReadAllText(codeFile);
                                if (remove)
                                {
                                    fileContent = string.Join("\n", fileContent.Split('\n').Where(line => !string.IsNullOrWhiteSpace(line)));
                                }
                                writer.WriteLine($"Code File:");
                                writer.WriteLine("=========================================");
                                writer.WriteLine(fileContent);
                                writer.WriteLine();
                            }
                           
                        }
                        if(isValid==true)
                        Console.WriteLine("bundle file created succesfully");
                        else
                        {
                            Console.WriteLine("Note: the value you entered is invalid for language option. the bundle file is empty now.");
                        }
                    }
                }
                catch (DirectoryNotFoundException dr)
                {
                    Console.WriteLine(dr.Message);
                    Console.WriteLine("please enter a valid path.");
                    Console.WriteLine("If the file name consists of two words, write it in quotation marks ");
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

        }, bundleOptionOutput,bundleOptionLanguage,bundleOptionNote,bundleOptionSort,bundleOptionDeleteEmptyLines,bundleOptionAuthor);

            rspCommand.SetHandler((output) =>
            {
                var options = new[]
                {
                 new QuestionOption("output", "What is the recomemnded value for option 'output' "),
                 new QuestionOption("language", "What is the recomemnded value for option 'language' "),
                 new QuestionOption("note", "What is the recomemnded value for option 'note' "),
                 new QuestionOption("sort", "What is the recomemnded value for option 'sort' "),
                 new QuestionOption("remove empty lines", "What is the recomemnded value for option 'remove empty lines' "),
                 new QuestionOption("author", "What is the recomemnded value for option 'author' "),
                };
                var answers = new string[options.Length];

                for (int i = 0; i < options.Length; i++)
                {
                    var question = options[i].Question;
                    Console.WriteLine(question);
                    answers[i] = Console.ReadLine();
                }

                var rspContent = string.Join("\n", answers);
                File.WriteAllText(output, rspContent);

                Console.WriteLine("response file created succesfully");

            },rspOptionOutput);
            var rootCommand = new RootCommand("root command for file bundler CLI");
            rootCommand.AddCommand(bundleCommand);
            rootCommand.AddCommand(rspCommand);
            rootCommand.InvokeAsync(args);
        }
    }
    class QuestionOption
    {
        public string Option { get; }
        public string Question { get; }

        public QuestionOption(string option, string question)
        {
            Option = option;
            Question = question;
        }
    }
}
//לחילופין ניתן להקיש ניתוב מלא. 

//אם האפליקציה לא תצליח לשמור בניתוב זה היא תספק הודעת שגיאה מתאימה.

//יש לשים לב שאם יש רווח בניתוב של הקובץ (לדוגמא אם שם התיקיה מורכב משתי מילים) יש להקיש אותו בשורת הפקודה כשהוא עטוף במרכאות.
//עלייך לדאוג לבצע בדיקות תקינות (ולידציה) על הקלט מהמשתמש ולהציג הודעה מסודרת במקרה שהקלט לא תקין.

//אין לכלול ב-bundle קבצי קוד שנמצאים בתיקיות bin, debug וכדומה.