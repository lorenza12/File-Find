using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Word;

namespace File_Find
{
    class Searcher
    {
        private string searchDirectory;
        private string searchWord;
        private string fileType;
        private bool navSubDirectories;
        private bool matchWholeWord;
        private bool findInFiles;
        private bool matchCase;
        private string[] foundFiles;

        public string errorMessage;

        public Searcher()
        {
            this.searchDirectory = null;
            this.SearchWord = null;
            this.fileType = null;
            this.navSubDirectories = false;
            this.matchWholeWord = false;
            this.findInFiles = false;
            this.matchCase = false;
            this.foundFiles = new string[] { };
        }

        public Searcher(string srchhDir, string srchWord, string fileTyp, bool navSubDirs, bool matchWholeWrd, bool findInFls, bool matchCase)
        {
            this.searchDirectory = srchhDir;
            this.SearchWord = srchWord;
            this.fileType = fileTyp;
            this.navSubDirectories = navSubDirs;
            this.matchWholeWord = matchWholeWrd;
            this.findInFiles = findInFls;
            this.matchCase = matchCase;
            this.foundFiles = new string[] { };

        }

        public string SearchWord { get => searchWord; set => searchWord = value; }
        public string SearchDirectory { get => searchDirectory; set => searchDirectory = value; }
        public string FileType { get => fileType; set => fileType = value; }
        public bool NavSubDirectories { get => navSubDirectories; set => navSubDirectories = value; }
        public bool MatchWholeWord { get => matchWholeWord; set => matchWholeWord = value; }
        public bool FindInFiles { get => findInFiles; set => findInFiles = value; }
        public bool MatchCase { get => matchCase; set => matchCase = value; }
        public string[] FoundFiles { get => foundFiles; set => foundFiles = value; }

        public void SearchForFiles()
        {
            try
            {
                this.errorMessage = "";
                ClearFoundFiles();

                if (!Directory.Exists(this.searchDirectory))
                {

                    this.errorMessage = "Error: Directory Doesn't Exist";

                }

                else
                {

                    string searchFile = "";
                    if (this.MatchWholeWord)
                    {
                        searchFile = this.SearchWord + this.FileType;
                    }

                    else
                    {
                        searchFile = $"*{this.SearchWord.Trim()}*{this.FileType}";
                    }

                    if (!this.MatchCase)
                    {
                        this.SearchWord = this.SearchWord.ToLower();
                    }

                    //Not the most effeiciant way of searching for files/directories but this 
                    //method prevents the search from canceling if an I/O execption is raised
                    var pathsToSearch = new Queue<string>();
                    var foundFiles = new List<string>();

                    //only used if we are searching within files
                    var foundInFiles = new List<string>();

                    pathsToSearch.Enqueue(this.SearchDirectory);

                    while (pathsToSearch.Count > 0)
                    {
                        var dir = pathsToSearch.Dequeue();

                        try
                        {
                            //if user wants to search in files, we need to check all files (*) regardless of their name
                            var files = Directory.GetFiles(dir, (this.FindInFiles ? "*" : searchFile));

                            if (this.findInFiles)
                            {
                                //foundInFiles.Concat(CheckInFiles(files));

                                var inFilesResults = CheckInFiles(files);

                                //can't concat null list so must specifically check
                                if (foundInFiles.Count == 0 && inFilesResults.Count > 0)
                                {
                                    foundInFiles = inFilesResults;
                                }

                                else if (inFilesResults.Count == 0 && foundInFiles.Count > 0)
                                {
                                    //skip because nothing was found in the files so we don't need to add any
                                }

                                else
                                {
                                    //both list have data or both list are empty so concat them 
                                    foundInFiles = foundInFiles.Concat(inFilesResults).ToList();
                                }

                            }

                            foreach (var file in Directory.GetFiles(dir, searchFile))
                            {
                                if (this.MatchCase)
                                {
                                    //Contains method is case sensitive search
                                    if (file.Contains(this.SearchWord))
                                    {
                                        foundFiles.Add(file);
                                    }

                                }
                                else
                                {
                                    foundFiles.Add(file);
                                }
                            }

                            if (this.NavSubDirectories)
                            {
                                //if user selected to naviagte sub directories
                                //queue sub directories up
                                foreach (var subDir in Directory.GetDirectories(dir))
                                {
                                    pathsToSearch.Enqueue(subDir);
                                }
                            }

                        }

                        catch (UnauthorizedAccessException)
                        {
                            //Skip files/folders that we don't have access to and continue
                            //Console.WriteLine(dir.ToString());
                        }
                        catch (Exception)
                        {
                            //skip any other I/O errors and continue
                        }
                    }

                    if (this.findInFiles)
                    {
                        //can't concat null list so must specifically check
                        if (foundInFiles.Count == 0 && foundFiles.Count > 0)
                        {
                            this.foundFiles = foundFiles.ToArray();
                        }
                        else if (foundFiles.Count == 0 && foundInFiles.Count > 0)
                        {
                            this.foundFiles = foundInFiles.ToArray();
                        }
                        else
                        {
                            //both list have data or both list are empty so concat them 
                            this.foundFiles = foundInFiles.Concat(foundFiles).ToList().ToArray();
                        }
                    }

                    else
                    {
                        this.foundFiles = foundFiles.ToArray();
                    }
                }
            }
            catch (Exception e)
            {
                //this.errorMessage = e.ToString();
                this.errorMessage = "Error: " + e.ToString();
            }
        }
        public string GetFileType(string lngFileType)
        {
            string fileType;

            switch (lngFileType)
            {
                case "Text File (*.txt)":
                    fileType = ".txt";
                    break;

                case "Word Document (*.docx)":
                    fileType = ".docx";
                    break;

                case "Excel Workbook (*.xlsx)":
                    fileType = ".xlsx";
                    break;

                case "CSV (*.csv)":
                    fileType = ".csv";
                    break;

                case "All Files (*.*)":
                    fileType = ".*";
                    break;

                default:
                    fileType = ".*";
                    break;

            }
            return fileType;
        }

        private List<string> CheckInFiles(string[] checkFiles)
        {
            var results = new List<string>();

            switch (FileType)
            {
                case ".txt":
                case ".csv":
                case ".*":
                    results = CheckTextFile(checkFiles);
                    break;

                case ".docx":
                    results = CheckWordFile(checkFiles);
                    break;

                case ".xlsx":
                    results = CheckExcelFile(checkFiles);
                    break;
            }
            return results;
        }

        private List<string> CheckWordFile(string[] checkFiles)
        {
            var matches = new List<string>();

            Microsoft.Office.Interop.Word.Application word = new Microsoft.Office.Interop.Word.Application();
            Microsoft.Office.Interop.Word.Document doc = new Microsoft.Office.Interop.Word.Document();

            foreach (string file in checkFiles)
            {
                object fileName = file;
                // Define an object to pass to the API for missing parameters
                object missing = System.Type.Missing;

                try
                {
                    doc = word.Documents.Open(ref fileName,
                            ref missing, ref missing, ref missing, ref missing,
                            ref missing, ref missing, ref missing, ref missing,
                            ref missing, ref missing, ref missing, ref missing,
                            ref missing, ref missing, ref missing);
                    string ReadValue = string.Empty;
                    // Activate the document
                    doc.Activate();
                    foreach (Microsoft.Office.Interop.Word.Range tmpRange in doc.StoryRanges)
                    {
                        if (this.MatchCase)
                        {
                            ReadValue += tmpRange.Text;
                        }
                        else
                        {
                            ReadValue += tmpRange.Text.ToLower();
                        }

                    }

                    if (this.MatchWholeWord)
                    {
                        string searchRegex = @"(^|\s)" + this.SearchWord + @"(\s|$)";
                        if (Regex.IsMatch(ReadValue, searchRegex))
                        {
                            matches.Add(file);
                        }
                    }
                    else
                    {
                        if (ReadValue.Contains(this.SearchWord))
                        {
                            matches.Add(file);
                        }
                    }

                    doc.Close();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(doc);
                }

                catch (Exception e)
                {
                    //Skip file
                    //could be a temporary word file that is created when opening word files
                }

            }

            return matches;

        }

        private void ClearFoundFiles()
        {
            Array.Clear(this.FoundFiles, 0, this.FoundFiles.Length);
        }

        private List<string> CheckTextFile(string[] filesToCheck)
        {
            var matches = new List<string>();
            foreach (string file in filesToCheck)
            {
                try
                {
                    //For file types not listed on the dropdown, try and read them like text files
                    //If we cant, just skip them
                    string fileContents;
                    using (StreamReader streamReader = new StreamReader(file, Encoding.UTF8))
                    {
                        if (this.MatchCase)
                        {
                            fileContents = streamReader.ReadToEnd();
                        }
                        else
                        {
                            fileContents = streamReader.ReadToEnd().ToLower();
                        }
                    }

                    if (this.MatchWholeWord)
                    {
                        string searchRegex = @"(^|\s)" + this.SearchWord + @"(\s|$)";
                        if (Regex.IsMatch(fileContents, searchRegex))
                        {
                            matches.Add(file);
                        }
                    }
                    else
                    {
                        if (fileContents.Contains(this.SearchWord))
                        {
                            matches.Add(file);
                        }
                    }
                }
                catch (Exception e)
                {
                    //pass - we couldn't read the file
                }

            }

            return matches;

        }

        private List<string> CheckExcelFile(string[] filesToCheck)
        {
            var matches = new List<string>();

            foreach (string file in filesToCheck)
            {
                DataSet data = OpenExcelFile(file);

                foreach (System.Data.DataTable table in data.Tables)
                {

                    foreach (DataRow dr in table.Rows)
                    {
                        foreach (var item in dr.ItemArray)
                        {
                            string temp;
                            if (this.MatchCase)
                            {
                                temp = item.ToString().Trim();
                            }

                            else
                            {
                                temp = item.ToString().Trim().ToLower();
                            }

                            if (this.MatchWholeWord)
                            {
                                string searchRegex = @"(^|\s)" + this.SearchWord + @"(\s|$)";
                                if (Regex.IsMatch(temp, searchRegex))
                                {
                                    matches.Add(file);
                                }
                            }
                            else
                            {
                                if (temp.Contains(this.SearchWord))
                                {
                                    matches.Add(file);

                                }
                            }
                        }
                    }
                }
            }
            return matches;
        }

        private DataSet OpenExcelFile(string file)
        {
            string connectionString = "Provider = Microsoft.ACE.OLEDB.12.0; Data Source = " + file + "; Extended Properties = 'Excel 12.0;HDR=NO;IMEX=1;'";
            string[] excelsheetNames = GetExcelSheetNames(connectionString);

            DataSet excelData = Parse(connectionString);
            return excelData;
        }

        private string[] GetExcelSheetNames(string connectionString)
        {
            try
            {
                OleDbConnection con = null;
                System.Data.DataTable dt = null;
                con = new OleDbConnection(connectionString);
                con.Open();
                dt = con.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

                if (dt == null)
                {
                    return null;
                }

                String[] excelSheetNames = new String[dt.Rows.Count];
                int i = 0;

                foreach (DataRow row in dt.Rows)
                {
                    excelSheetNames[i] = row["TABLE_NAME"].ToString();
                    i++;

                }

                return excelSheetNames;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public DataSet Parse(string connectionString)
        {
            try
            {
                DataSet data = new DataSet();

                int sheetCounter = 0;
                foreach (var sheetName in GetExcelSheetNames(connectionString))
                {
                    using (OleDbConnection con = new OleDbConnection(connectionString))
                    {
                        var dataTable = new System.Data.DataTable();

                        string query = string.Format("SELECT * FROM [{0}]", sheetName);
                        con.Open();
                        OleDbDataAdapter adapter = new OleDbDataAdapter(query, con);

                        adapter.Fill(dataTable);
                        data.Tables.Add(dataTable);

                        data.Tables[sheetCounter].TableName = sheetName;

                        sheetCounter += 1;
                    }
                }

                return data;
            }
            catch (Exception e)
            {
                return null;
            }
        }

    }
}
