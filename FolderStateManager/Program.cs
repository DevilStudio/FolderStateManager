using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;
using CommandLine.Text;
using System.IO;
using System.Collections;
namespace FolderStateManager
{
    class Options
    {


        [Option('m', "maxsize", DefaultValue = 10737418240,
        HelpText = "Max size for folder")]
        public double MaxSize { get; set; }



        [Option('v', "verbose", DefaultValue = true,
          HelpText = "Prints all messages to standard output.")]
        public bool Verbose { get; set; }

        [Option('f', "folder", Required = true,
          HelpText = "Folder to be processed.")]
        public string Folder { get; set; }
        [Option('s', "filemask", DefaultValue = "*",
        HelpText = "Mask for files to be processed")]
        public string FileMask { get; set; }


        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this,
              (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
    class FileFullinfo
    {
        public FileFullinfo(string path, DateTime dateChange)
        {
            this.dateChange = dateChange;
            this.path = path;
        }
        public DateTime dateChange;
        public string path;
    }
    class Program
    {
        static void Main(string[] args)
        {
            var options = new Options();
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                if (options.MaxSize > 0)
                {
                    if (options.Verbose) Console.WriteLine("Max folder size: {0}", options.MaxSize);
                }
                Console.WriteLine(options.Folder);

                string[] findFiles = System.IO.Directory.GetFiles(options.Folder,options.FileMask, System.IO.SearchOption.AllDirectories);

                Array.Sort<string>(findFiles, delegate(string a, string b)
                {
                    return File.GetLastWriteTimeUtc(a).CompareTo(File.GetLastWriteTimeUtc(a));
                });

                Hashtable files = new Hashtable();
                

                DateTime last = DateTime.MinValue;
                string lastFile = "";
                Double AllSize = 0;

                foreach (string file in findFiles)
                {
                    DateTime filemodified = File.GetLastWriteTimeUtc(file);
                    FileFullinfo fileI = new FileFullinfo(file, filemodified );
                    if (filemodified > last)
                    {
                        last = filemodified;
                        lastFile = file;
                    }
                    files.Add(file, fileI);
                    AllSize += new FileInfo(file).Length;
                }
                files.Remove(lastFile);
                


                if (AllSize > options.MaxSize)
                {
                    options.MaxSize -= new FileInfo(lastFile).Length;
                    AllSize -= new FileInfo(lastFile).Length;
                    removeFiles(files, AllSize, options.MaxSize);
                }
                
                    





                // Values are available here
                if (options.Verbose) Console.WriteLine("Folder: {0}", options.Folder);

            }
        }
        public static void removeFiles(Hashtable files, double allSize, double maxSize)
        {
            if (files.Count == 0)
            {
                Console.Write("All files was removed, but size is exceed");
                return;
            }
            double filesize = allSize / (files.Count);
            int filescounttoremove = Convert.ToInt32(Math.Ceiling(((allSize - maxSize) / filesize)));
            int stepforremove = Convert.ToInt32(Math.Ceiling(Convert.ToDouble( (files.Count) / filescounttoremove)));

            int k = 1;
            foreach (DictionaryEntry f in files)
            {
                
                if (stepforremove==0 || k%stepforremove == 0)
                {
                    
                    allSize -= new FileInfo((f.Value as FileFullinfo).path).Length;
                    File.Delete((f.Value as FileFullinfo).path);
                    if (allSize > maxSize)
                    {
                        files.Remove(f.Key);
                        removeFiles(files, allSize, maxSize);
                        return;
                    }
                    else
                    {
                        return;
                    }
                }
                
                k++;
                
            }
        }
    }
}
