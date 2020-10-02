using System;
using System.Data;
using System.DirectoryServices;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using AForge.Imaging.Filters;

namespace Flower_Space
{
    static class Helper
    {

        static public bool DeleteAllData()
        {
            if (MessageBox.Show("Confirm delete all data", "Delete all data", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    System.IO.DirectoryInfo directory = new System.IO.DirectoryInfo(Helper.DataPath());

                    foreach (System.IO.FileInfo file in directory.GetFiles()) file.Delete();
                    foreach (System.IO.DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error\n" + ex.Message, "Delete all data failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            return true;
        }
        
        static public bool ExportAllData()
        {
            MessageBox.Show("Sorry the export function is not implimented", "Export data", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return false;
        }

        public static bool ApplicationDataBaseExists()
        {
            if (Directory.Exists(DataPath()))
            {
                if ( File.Exists(ApplicationDataBase())) return true;

                return CopyNewDataBase();
            }
            
            return false;
        }

        public static string ApplicationDataBase()
                {
                    return DataPath() + @"\" + Flower_Space.Properties.Settings.Default.DataBaseName;
                }  // End of ApplicationDataBase

        /// <summary>
                /// If the application store does not exist quit
                /// </summary>
                /// <returns></returns>
        private static string DataPath()
                {
                    string DataPath = ApplicationDataPath();
                    if (!(DataPath == ""))
                    {
                        return DataPath;
                    }
                    else
                    {
                        MessageBox.Show("Application data store not found. See your administrator","Application error",MessageBoxButtons.OK,MessageBoxIcon.Stop);
                        Environment.Exit(1);
                        return "";
                    }
                }

        /// <summary>
                /// Check if the application data path exists, if not create it
                /// </summary>
                /// <returns>Path to database</returns>
        public static String ApplicationDataPath()
                {
                    string ApplicationDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    string DataFolder = Flower_Space.Properties.Settings.Default.DataFolder;
                    string DataBasePath = Path.Combine(ApplicationDataPath, DataFolder);


                    if (Directory.Exists(DataBasePath))
                    { return DataBasePath; }
                    else
                    {
                        try
                        {
                            Directory.CreateDirectory(DataBasePath);

                        }
                        catch (Exception)
                        {
                            return "";
                        }
                        return DataBasePath;
                    }

                }  // End of ApplicationDataPathAvailable

        public static string  ExportPath()
                {
                    string pathP1 = Properties.Settings.Default.InportFolder;
                    string pathP3 = DateTime.Now.ToString("yyyy-MM-dd HH mm tt");

                    using (FolderBrowserDialog fbd = new FolderBrowserDialog())
                    {
                        fbd.Description = "Select/create folder to export data to.";
                        fbd.ShowNewFolderButton = false;
                        fbd.SelectedPath = pathP1;

                        if (fbd.ShowDialog() == DialogResult.OK)
                        {
                        }
                    }


                    string pathP2 = Flower_Space.Properties.Settings.Default.ExportFolder;


                    if(PathExistsCreate(pathP1 + "\\" + pathP2))
                    {
                        if (PathExistsCreate(pathP1 + "\\" + pathP2 + "\\" + pathP3))
                        {
                            return pathP1 + "\\" + pathP2 + "\\" + pathP3;
                        }
                    }

                    return "";
                }

        public static string ExportFolder()
        {
            //string pathP1 = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            //string pathP2 = Flower_Space.Properties.Settings.Default.ExportFolder;


            return "";
        }

        public static string ApplicationMetaDataFolder()
            {
            string ApplicationDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string ApplicationMetaDataPath = Path.Combine(ApplicationDataPath, Properties.Settings.Default.MetaDataFolder);

            if (PathExistsCreate(ApplicationMetaDataPath))
            {
                return ApplicationMetaDataPath;
            }

            return "";
        }
        
        public static string FolderBrowser(string rootFolder, string Description, bool showNew)
        { 
            FolderBrowserDialog FBD = new FolderBrowserDialog();

            FBD.Description = Description;
            if (rootFolder.Length > 6) FBD.SelectedPath = rootFolder;
            FBD.ShowNewFolderButton = showNew;

            DialogResult result = FBD.ShowDialog();

            if (result == DialogResult.OK)
            {
                return FBD.SelectedPath;
            }
            return "";
        }

        /// <summary>
                /// Check if the required path exists if not create it
                /// </summary>
                /// <returns>true if required path exists else false</returns>
        public static bool PathExistsCreate(string requiredPath)
                {
                    if (Directory.Exists(requiredPath))
                    { return true; }
                    else
                    {
                        try
                        {
                            Directory.CreateDirectory(requiredPath);

                        }
                        catch (Exception)
                        {
                            return false;
                        }
                        return true;
                    }

                }  // PathExistsCreate

        /// <summary>
                /// Copy a new database to the data folder
                /// </summary>
                /// <param name="MyDB"></param>
                /// <returns></returns>
        public static bool CopyNewDataBase()
                {
                    string source = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\" + Flower_Space.Properties.Settings.Default.DataBaseName;
                    try
                    {
                        System.IO.File.Copy(source, Helper.ApplicationDataBase());
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                    return true;
                }

        /// <summary>
                /// Gets the users full name from directory services
                /// 
                /// Requires: using System.DirectoryServices.AccountManagement;
                /// </summary>
                /// <returns></returns>
        public static string GetUserName()
                {
                    //            UserPrincipal userPrincipal = UserPrincipal.Current;
                    //            return userPrincipal.DisplayName;

                    string domain = Environment.UserDomainName;
                    string userName = Environment.UserName;
                    DirectoryEntry userEntry = new DirectoryEntry("WinNT://" + domain + "/" + userName + ",User");
                    return (string)userEntry.Properties["fullname"].Value;
                }

        public static DataSet CreateDataSet()
                {
                    DataSet ds = new DataSet("BloomData");

                    DataTable dtb = new DataTable("Bloom");
                    dtb.Columns.Add("bloomid", typeof(string));
                    dtb.Columns.Add("LargeThumbNailName", typeof(string));
                    dtb.Columns.Add("SmallThumbNailName", typeof(string));
                    dtb.Columns.Add("LargeColourThumbNailName", typeof(string));
                    dtb.Columns.Add("SmallColourThumbNailName", typeof(string));
                    dtb.Columns.Add("Name", typeof(string));
                    dtb.Columns.Add("Cultivar", typeof(string));
                    dtb.Columns.Add("Genus", typeof(string));
                    dtb.Columns.Add("Species", typeof(string));
                    dtb.Columns.Add("Shape", typeof(string));
                    dtb.Columns.Add("Size", typeof(string));
                    dtb.Columns.Add("Inflorescence", typeof(string));
                    dtb.Columns.Add("created", typeof(string));
                    dtb.Columns.Add("changed", typeof(string));
                    ds.Tables.Add(dtb);

                    DataTable dtc = new DataTable("Bloom_Colour");
                    dtc.Columns.Add("Bloom_ID", typeof(string));
                    dtc.Columns.Add("BandIndex", typeof(int));
                    dtc.Columns.Add("UPOV_Colour_ID", typeof(int));
                    dtc.Columns.Add("BandWidth", typeof(int));
                    ds.Tables.Add(dtc);


                    DataTable dtp = new DataTable("Bloom_Photo");
                    dtc.Columns.Add("photoid", typeof(string));
                    dtc.Columns.Add("bloomid", typeof(string));
                    dtc.Columns.Add(" filename", typeof(string));
                    dtc.Columns.Add("datecaptured", typeof(string));
                    ds.Tables.Add(dtp);

                    DataTable dtpv = new DataTable("BloomPhotoBloomView");
                    dtpv.Columns.Add("photoid", typeof(string));
                    dtpv.Columns.Add("viewid", typeof(string));
                    ds.Tables.Add(dtpv);



                    return ds;
                }


        /// <summary>
                /// Export app data
                /// Export to default folder or custome folder.
                /// A folder with  a name following the "yyyy-MM-dd HH mm tt" pattern will be created in the selected folder.
                /// Optional clear the application data folder
                /// </summary>
        public static string ExportDataPath()
        {
            FolderBrowserDialog FBD = new FolderBrowserDialog();
            string pathP1 = Properties.Settings.Default.ExportFolder;

            FBD.Description = "Select folder to import data to.";
            FBD.SelectedPath = pathP1;
            FBD.ShowNewFolderButton = true;

            DialogResult result = FBD.ShowDialog();

            if (result == DialogResult.Cancel) return "";

            return Path.Combine(FBD.SelectedPath, DateTime.Now.ToString("yyyy-MM-dd HH mm tt")); 
        }
                
        public static Bitmap CropToMaxSquare(Bitmap TargetImage)
        {
            if (TargetImage == null) return null;

            int sq = Math.Min(TargetImage.Width, TargetImage.Height);
            int left = (TargetImage.Width - sq) / 2;

            Rectangle rec = new Rectangle(left, 0, sq, sq);

            // create filter
            Crop filter = new Crop(rec);
            // apply the filter
            Bitmap newImage = filter.Apply(TargetImage);

            return newImage;
        }
          
    }
    //static class Helper
}
// namespace Flower_Space
