using System;
using System.IO;
using System.Text;
using System.Xml;

namespace AGSModuleExporter
{
    class AGSModuleExporter
    {
        const string MODULE_FILE_SIGNATURE = "AGSScriptModule\0";
        const uint MODULE_FILE_TRAILER = 0xb4f76a65;
        const string MODULE_FILE_EXTENSION = ".scm";

        const string SCRIPT_FILE_EXTENSION = ".asc";
        const string SCRIPT_HEADER_FILE_EXTENSION = ".ash";
        const string SCRIPT_METADATA_FILE_EXTENSION = ".xml";

        const string ARG_SCRIPT = "-script";
        const string ARG_MODULE = "-module";
        const string ARG_HELP = "-help";

        static string _scriptFileName = string.Empty;
        static string _scriptText = string.Empty;
        static string _headerFileName = string.Empty;
        static string _headerText = string.Empty;
        static string _moduleFileName = string.Empty;
        static string _author = string.Empty;
        static string _description = string.Empty;
        static string _name = string.Empty;
        static string _version = string.Empty;
        static int _uniqueKey;

        public static void Main(string[] args)
        {
            string scriptBaseFileName = null;
            _moduleFileName = null;
            for (int i = 0; i < (args.Length - 1); ++i)
            {
                switch (args[i])
                {
                    case ARG_SCRIPT:
                        scriptBaseFileName = args[++i];
                        break;
                    case ARG_MODULE:
                        _moduleFileName = args[++i];
                        break;
                    default:
                        break;
                }
            }
            if (string.IsNullOrEmpty(scriptBaseFileName))
            {
                PrintHelpText();
                return;
            }
            if (scriptBaseFileName.EndsWith(SCRIPT_FILE_EXTENSION) || scriptBaseFileName.EndsWith(SCRIPT_HEADER_FILE_EXTENSION))
            {
                if (scriptBaseFileName.Length == 4)
                {
                    PrintHelpText();
                    return;
                }
                scriptBaseFileName = scriptBaseFileName.Substring(0, scriptBaseFileName.Length - 4);
            }
            if (_moduleFileName == null)
            {
                _moduleFileName = scriptBaseFileName;
            }
            if (!_moduleFileName.EndsWith(MODULE_FILE_EXTENSION))
            {
                _moduleFileName += MODULE_FILE_EXTENSION;
            }
            if (File.Exists(_moduleFileName))
            {
                try
                {
                    File.Delete(_moduleFileName);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to delete existing script module!");
                    Console.WriteLine(e.Message);
                    return;
                }
            }
            _scriptFileName = scriptBaseFileName + SCRIPT_FILE_EXTENSION;
            _scriptText = ReadTextFile(_scriptFileName);
            _headerFileName = scriptBaseFileName + SCRIPT_HEADER_FILE_EXTENSION;
            _headerText = ReadTextFile(_headerFileName);
            _uniqueKey = new Random().Next(Int32.MaxValue);
            ReadMetadata(scriptBaseFileName + SCRIPT_METADATA_FILE_EXTENSION);
            ExportScriptModule();
            Console.WriteLine("Finished. Succeeded? " + File.Exists(_moduleFileName));
        }

        static void PrintHelpText()
        {
            Console.WriteLine("AGS Script Module Exporter by monkey0506");
            Console.WriteLine("");
            Console.WriteLine("  " + ARG_SCRIPT + " <script file>\n");
            Console.WriteLine("    The location of the script .ASC or .ASH file.\n    Missing script/header is defaulted to empty text.\n\n");
            Console.WriteLine("  " + ARG_MODULE + " <exported module file>\n");
            Console.WriteLine("    The filename of the exported script module.\n    \"" + MODULE_FILE_EXTENSION + "\" file extension is added if not present.\n\n");
            Console.WriteLine("  " + ARG_HELP + "\n");
            Console.WriteLine("    Prints this text.\n");
            Console.WriteLine("\nThe script module name, description, author, version, and \"UniqueKey\" may be set in a separate XML file. See README for details.\n");
        }

        public static string GetElementString(XmlNode node, string elementName)
        {
            XmlNode foundNode = node.SelectSingleNode(elementName);
            if (foundNode == null)
            {
                return string.Empty;
            }
            return foundNode.InnerText;
        }

        static void ReadMetadata(string filename)
        {
            if (!File.Exists(filename))
            {
                return;
            }
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(filename);
                _name = GetElementString(doc.DocumentElement, "Name");
                _description = GetElementString(doc.DocumentElement, "Description");
                _author = GetElementString(doc.DocumentElement, "Author");
                _version = GetElementString(doc.DocumentElement, "Version");
                int oldUniqueKey = _uniqueKey;
                if (!Int32.TryParse(GetElementString(doc.DocumentElement, "Key"), out _uniqueKey))
                {
                    _uniqueKey = oldUniqueKey;
                }
            }
            catch (FileNotFoundException) { }
            catch (XmlException) { }
            catch (IOException) { }
        }

        static string ReadTextFile(string filename)
        {
            try
            {
                return File.Exists(filename) ? File.ReadAllText(filename) : string.Empty;
            }
            catch (IOException) { }
            return string.Empty;
        }

        static void ExportScriptModule()
        {
            using (BinaryWriter writer = new BinaryWriter(new FileStream(_moduleFileName, FileMode.Create, FileAccess.Write)))
            {
                try
                {
                    writer.Write(Encoding.ASCII.GetBytes(MODULE_FILE_SIGNATURE));
                    writer.Write(1); // version

                    WriteNullTerminatedString(_author, writer);
                    WriteNullTerminatedString(_description, writer);
                    WriteNullTerminatedString(_name, writer);
                    WriteNullTerminatedString(_version, writer);

                    writer.Write(_scriptText.Length);
                    WriteNullTerminatedString(_scriptText, writer);

                    writer.Write(_headerText.Length);
                    WriteNullTerminatedString(_headerText, writer);

                    writer.Write(_uniqueKey);
                    writer.Write(0); // Permissions
                    writer.Write(0); // We are owner
                    writer.Write(MODULE_FILE_TRAILER);
                }
                catch (IOException) { }
            }
        }

        private static void WriteNullTerminatedString(string text, BinaryWriter writer)
        {
            writer.Write(Encoding.Default.GetBytes(text));
            writer.Write((byte)0);
        }
    }
}
