using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Flower_Space
{
    static class BloomData
    {
        const char delimiter = '\\';
        static public bool IsVlid = false;
        

        static public string DataBaseVersion()
        {
            int dbMajorVersion = 0;
            int dbMinorVersion = 0;
            int dbRevision = 0;
            string dbVirsionNotes = "No database";

            DataTable dbv = GetDBVersion();

            if (dbv.Rows.Count > 0)
            {
                try
                {
                    dbMajorVersion = Convert.ToInt32(dbv.Rows[0].Field<int>("majorversion"));
                    dbMinorVersion = Convert.ToInt32(dbv.Rows[0].Field<int>("minorversion"));
                    dbRevision = Convert.ToInt32(dbv.Rows[0].Field<int>("revision"));
                    dbVirsionNotes = dbv.Rows[0].Field<string>("versionnotes");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Can't continue. Error:\n\n" + ex.Message, "Database corrupt", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return " Error geting database version";
                }
            }

            return dbMajorVersion.ToString("00") + "-" + dbMinorVersion.ToString("00") + "-" + dbRevision.ToString("00") + "   " + dbVirsionNotes;
        }


        #region Validate Database

        /// <summary>
        /// Test if database is exists and is valid
        /// If the db does not exist try copying it from the application folder
        /// 
        /// Application proporties hold the required Major version and Minor version
        /// Test them against the database version
        /// </summary>
        /// <returns></returns>
        static public bool ValidDataBase()
        {
            int dbMajorVersion;
            int dbMinorVersion;
            int dbRevision;
            string dbVirsionNotes;

            string adb = Helper.ApplicationDataBase();

            if (!(File.Exists(adb)))
            {
                if (!(Helper.CopyNewDataBase()))
                {
                    MessageBox.Show("Database not found", "Could not find or create the data base.\n", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    IsVlid = false;
                    return false;
                }
            }

            DataTable dbv = GetDBVersion();

            if (dbv.Rows.Count > 0)
            {
                try
                {
                    dbMajorVersion = Convert.ToInt32(dbv.Rows[0].Field<int>("majorversion"));
                    dbMinorVersion = Convert.ToInt32(dbv.Rows[0].Field<int>("minorversion"));
                    dbRevision = Convert.ToInt32(dbv.Rows[0].Field<int>("revision"));
                    dbVirsionNotes = dbv.Rows[0].Field<string>("versionnotes");
                }
                catch (Exception ex)
                {
                    MessageBox.Show( "Can't continue. Error:\n\n" + ex.Message, "Database corrupt", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                if (!(Properties.Settings.Default.DBMajorVersion == dbMajorVersion))
                {
                    string mess = @"This application requires database version " + Properties.Settings.Default.DBMajorVersion +
                        " you currently have version " + dbMajorVersion.ToString() + ".\n\nThis database can't be upgraded.\n\n" +
                        "    Select 'Yes' to export photos & data.\n" +
                        "    Select 'No' to delete data & photos\n" +
                        "    Select 'Cancel' to close the application";
                    DialogResult dr = MessageBox.Show(mess, "Incorrect daabase version",MessageBoxButtons.YesNoCancel,MessageBoxIcon.Question);
                    switch (dr)
                    {
                        case DialogResult.Cancel:
                            Environment.Exit(1);
                            break;
                        case DialogResult.Yes:
                            if(!Helper.ExportAllData()) Environment.Exit(1);
                            break;
                        case DialogResult.No:
                            if(!Helper.DeleteAllData()) Environment.Exit(1);
                            Helper.CopyNewDataBase();
                            break;
                        default:
                            Environment.Exit(1);
                            break;
                    }
                }


                if (!(Properties.Settings.Default.DBMinorVersion == dbMinorVersion))
                {
                    string rqv = Properties.Settings.Default.DBMajorVersion.ToString() + "-" + Properties.Settings.Default.DBMinorVersion.ToString() + "-?";
                    string gov = dbMajorVersion.ToString() + "-" + dbMinorVersion.ToString() + "-" + dbRevision.ToString();
                    return false;
                }
                IsVlid = true;
                return true;
            }

            MessageBox.Show("An unknow error has occured\nPlease contact you application administor.", "Unknown database error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            IsVlid = false;
            return false;
        }  // End ValidDataBase =================================================================

        #endregion  // Validate Database
        // Validate Database

        /// <summary>
        /// Property database connection string
        /// </summary>
        static string Connection_String
        {
            get
            {
                return "Data Source=" + Helper.ApplicationDataBase() + ";Version=3; New=False;";
            }
        }  
        // End of Connection_String

        /// <summary>
        /// Gets the database version record
        /// There should be 1 and 1 only record in this table
        /// </summary>
        /// <returns>DataTable</returns>
        static public DataTable GetDBVersion()
        {
            SQLiteConnection objConn;
            DataTable dt = new DataTable();
            try
            {
                objConn = new SQLiteConnection(Connection_String, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fatel error \n" + ex.Message,"Can't open database",MessageBoxButtons.OK,MessageBoxIcon.Error);
                throw;
            }

            string sql = "SELECT * FROM databaseversion ORDER BY majorversion DESC, minorversion DESC, " +
                        "  revision DESC   LIMIT 1;";


            //string sql = "SELECT majorversion, minorversion, revision, versionnotes FROM databaseversion;";
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(sql, objConn);
            SQLiteCommandBuilder command = new SQLiteCommandBuilder(adapter);
            adapter.Fill(dt);
            adapter.Dispose();
            objConn.Close();
            objConn.Dispose();
            return dt;

        }  // End of GetDBVersion

        static public string GetDBVersionDisplay()
        {
            DataTable dbv = GetDBVersion();
            int dbMajorVersion = Convert.ToInt32(dbv.Rows[0].Field<int>("majorversion"));
            int dbMinorVersion = Convert.ToInt32(dbv.Rows[0].Field<int>("minorversion"));
            int dbRevision = Convert.ToInt32(dbv.Rows[0].Field<int>("revision"));

            return dbMajorVersion.ToString() + "." + dbMinorVersion.ToString() + "." + dbRevision.ToString();
        }
        // GetDBVersionDisplay

        #region Insert Data
        public static bool WriteBloomData(DataSet ds)
        {
            SQLiteConnection objConn = new SQLiteConnection(Connection_String, true);
            objConn.Open();

            using (var transaction = objConn.BeginTransaction())
            {
                string sql;
                
                sql = @"INSERT INTO Bloom (bloomid, ltnname, stnname, lctnname,
                                            sctnname, name, cultivar,  genus, species, shape, size, 
                                            inflorescence, width, height, created, changed)
                                            VALUES (@bloomid, @ltnname, @stnname, @lctnname, 
                                                    @sctnname, @name, @cultivar, @genus, @species,
                                                    @shape, @size, @inflorescence, @width, @height, @created, @changed);";
                SQLiteCommand CommBloom = objConn.CreateCommand();
                CommBloom.CommandText = sql;

                sql = @"INSERT INTO Bloom_Colour (bloom_id, bandindex, bandwidth, upov_colour_id,  colour, red, green, blue)
                                         VALUES (@bloom_id, @bandindex,  @bandwidth, @upov_colour_id, @colour, @Red, @green, @blue);";
                SQLiteCommand CommColour = objConn.CreateCommand();
                CommColour.CommandText = sql;

                sql = @"INSERT INTO Photo_View ( photoid, viewid) VALUES (@photoid, @viewid);";
                SQLiteCommand CommView = objConn.CreateCommand();
                CommView.CommandText = sql;

                sql = @"INSERT INTO Bloom_Photo (photoid, bloomid, filename, cameraangle, thumbnail)  
                                        VALUES ( @photoid, @bloomid, @filename, @cameraangle, @thumbnail);";
                SQLiteCommand CommBloom_Photo = objConn.CreateCommand();
                CommBloom_Photo.CommandText = sql;

                //SQLiteCommand CommPhoto = objConn.CreateCommand();
                //CommPhoto.CommandText = sql;

                foreach (DataTable dt in ds.Tables)
                {
                    switch (dt.TableName)
                    {
                        case "Bloom":
                            foreach (DataRow row in dt.Rows)
                            {
                                CommBloom.Parameters.Clear();
                                CommBloom.Parameters.AddWithValue("@bloomid", row.Field<string>("bloomid"));
                                CommBloom.Parameters.AddWithValue("@ltnname", row.Field<string>("ltnname"));
                                CommBloom.Parameters.AddWithValue("@stnname", row.Field<string>("stnname"));
                                CommBloom.Parameters.AddWithValue("@lctnname", row.Field<string>("lctnname"));
                                CommBloom.Parameters.AddWithValue("@sctnname", row.Field<string>("sctnname"));
                                CommBloom.Parameters.AddWithValue("@name", row.Field<string>("name"));
                                CommBloom.Parameters.AddWithValue("@cultivar", row.Field<string>("cultivar"));
                                CommBloom.Parameters.AddWithValue("@genus", row.Field<string>("genus"));
                                CommBloom.Parameters.AddWithValue("@species", row.Field<string>("species"));
                                CommBloom.Parameters.AddWithValue("@shape", row.Field<string>("shape"));
                                CommBloom.Parameters.AddWithValue("@size", row.Field<int>("size"));
                                CommBloom.Parameters.AddWithValue("@inflorescence", row.Field<string>("inflorescence"));
                                CommBloom.Parameters.AddWithValue("@width", row.Field<int>("width"));
                                CommBloom.Parameters.AddWithValue("@height", row.Field<int>("height"));
                                CommBloom.Parameters.AddWithValue("@created", row.Field<string>("created"));
                                CommBloom.Parameters.AddWithValue("@changed", row.Field<string>("changed"));
                                CommBloom.ExecuteNonQuery();
                            }
                            break;
                        case "Bloom_Colour":
                            foreach (DataRow row in dt.Rows)
                            {
                                CommColour.Parameters.Clear();
                                CommColour.Parameters.AddWithValue("@bloom_id", row.Field<string>(0));
                                CommColour.Parameters.AddWithValue("@bandindex", row.Field<int>(1));
                                CommColour.Parameters.AddWithValue("@bandwidth", row.Field<int>(3));
                                CommColour.Parameters.AddWithValue("@upov_colour_id", row.Field<int>(2));
                                CommColour.Parameters.AddWithValue("@colour", row.Field<string>(4));
                                CommColour.Parameters.AddWithValue("@red", row.Field<int>(5));
                                CommColour.Parameters.AddWithValue("@green", row.Field<int>(6));
                                CommColour.Parameters.AddWithValue("@blue", row.Field<int>(7));
                                CommColour.ExecuteNonQuery();
                            }
                                break;
                        case "Photo_View":
                            foreach (DataRow row in dt.Rows)
                            {
                                CommView.Parameters.Clear();
                                CommView.Parameters.AddWithValue("@photoid", row.Field<string>(0));
                                CommView.Parameters.AddWithValue("@viewid", row.Field<string>(1));
                                CommView.ExecuteNonQuery();
                            }
                            break;
                        case "Bloom_Photo":
                            try
                            {
                                foreach (DataRow row in dt.Rows)
                                {

                                    CommBloom_Photo.Parameters.AddWithValue("@photoid", row.Field<string>(0));
                                    CommBloom_Photo.Parameters.AddWithValue("@bloomid", row.Field<string>(1));
                                    CommBloom_Photo.Parameters.AddWithValue("@filename", row.Field<string>(2));
                                    CommBloom_Photo.Parameters.AddWithValue("@cameraangle", row.Field<string>(3));
                                    CommBloom_Photo.Parameters.AddWithValue("@thumbnail", row.Field<string>(4));
                                    CommBloom_Photo.ExecuteNonQuery();
                                }
                            }
                            catch (Exception ex)
                            {
                                //TODO extend error handling to all saves
                                transaction.Rollback();
                                MessageBox.Show("Error saving photo data\n" + ex.Message, "Stop everything and report this",MessageBoxButtons.OK,MessageBoxIcon.Error);
                                return false;
                            }
                            break;
                        default:
                            break;
                    }
                }
                

                transaction.Commit();
            }

            return false;
        }

        public static bool InsertBloom(string BloomID, string LargeThumbNailName, string SmallThumbNailName, string LargeColourThumbNailName, string SmallColourThumbNailName)
        {
            string created = System.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
            string changed = System.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
            string sql = @"INSERT INTO bloom (bloomid, LargeThumbNailName, SmallThumbNailName, LargeColourThumbNailName, SmallColourThumbNailName,
                      created,  changed )
                  VALUES( @bloomid, @LargeThumbNailName, @SmallThumbNailName, @LargeColourThumbNailName, @SmallColourThumbNailName, @created, @changed); ";

            SQLiteConnection objConn = new SQLiteConnection(Connection_String, true);
            objConn.Open();
            SQLiteCommand objCommand = objConn.CreateCommand(); 
            objCommand.CommandText = sql;
            objCommand.Parameters.AddWithValue("@bloomid", BloomID);
            objCommand.Parameters.AddWithValue("@LargeThumbNailName", LargeThumbNailName);
            objCommand.Parameters.AddWithValue("@SmallThumbNailName", SmallThumbNailName);
            objCommand.Parameters.AddWithValue("@LargeColourThumbNailName", LargeColourThumbNailName);
            objCommand.Parameters.AddWithValue("@SmallColourThumbNailName", SmallColourThumbNailName);
            objCommand.Parameters.AddWithValue("@created", created);
            objCommand.Parameters.AddWithValue("@changed", changed);

            try
            {
                objCommand.ExecuteNonQuery();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public static void TestBuildWrite()
        {
            // string sql1 = "CREATE TABLE BloomPhoto (bloom_id STRING PRIMARY KEY, filename STRING, datecaptured STRING, Camera_Angle_Name STRING, Thumbnail STRING DEFAULT[No]);";

            //string sql1 = "INSERT INTO BloomPhoto(bloom_id, filename, datecaptured, Camera_Angle_Name, Thumbnail) " +
            //           "VALUES('bloom_id1', 'filename', 'datecaptured', 'Camera_Angle_Name', 'Thumbnail' );";

            string sql1 = "INSERT INTO Bloom_Photo(photoid, bloomid, filename, datecaptured, Camera_Angle_Name, Thumbnail) " +
                        " VALUES('photoidx', 'bloomid', 'filename', 'datecaptured', 'Camera_Angle_Name', 'Thumbnail');";

            SQLiteConnection objConn = new SQLiteConnection(Connection_String, true);
            objConn.Open();
            SQLiteCommand objCommand = objConn.CreateCommand();
            objCommand.CommandText = sql1;
            try
            {
                objCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        public static void TestWrite()
        {
            string sql1 = "CREATE TABLE BloomPhoto (bloom_id STRING PRIMARY KEY, filename STRING, datecaptured STRING, Camera_Angle_Name STRING, Thumbnail STRING DEFAULT[No]);";

            SQLiteConnection objConn = new SQLiteConnection(Connection_String, true);
            objConn.Open();
            SQLiteCommand objCommand = objConn.CreateCommand();
            objCommand.CommandText = sql1;
            try
            {
                objCommand.ExecuteNonQuery();
            }
            catch (Exception)
            {
                return;
            }
        }
  

        public static bool InsertBloomPhoto(string photoid, string bloomid, string filename, string datecaptured,
            string Camera_Angle_Name, string Thumbnail)
        {
                       string sql = @"INSERT INTO Bloom_Photo (photoid, bloomid, filename, datecaptured, " +
                                    " Camera_Angle_Name, Thumbnail) " +
                                    "VALUES ('" + photoid + "', '" + bloomid + "', '" + filename + "', " +
                                    "'" + datecaptured + "', '" + Camera_Angle_Name + "', '" + Thumbnail + "');";

            SQLiteConnection objConn = new SQLiteConnection(Connection_String, true);
            objConn.Open();
            SQLiteCommand objCommand = objConn.CreateCommand();
            objCommand.CommandText = sql;
            try
            {
                objCommand.ExecuteNonQuery();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public static bool InsertBloomView(string photoid, int viewid)
        {
            string sql = @"INSERT INTO BloomPhotoBloomView (photoid, viewid) VALUES (@photoid, @viewid);";

            SQLiteConnection objConn = new SQLiteConnection(Connection_String, true);
            objConn.Open();
            SQLiteCommand objCommand = objConn.CreateCommand();
            objCommand.CommandText = sql;
            objCommand.Parameters.AddWithValue("@photoid", photoid);
            objCommand.Parameters.AddWithValue("@viewid", viewid);

            try
            {
                objCommand.ExecuteNonQuery();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static bool InsertBloomColour(string Bloom_ID, int BandIndex, int UPOV_Colour_ID, int BandWidth, string ColourName, int Red, int Green, int Blue)
        {

            string sql = @"INSERT INTO Bloom_Colour(Bloom_ID, BandIndex, UPOV_Colour_ID, BandWidth, ColourName, Red, Green, Blue)
                         VALUES(@Bloom_ID, @BandIndex, @UPOV_Colour_ID, @BandWidth, @ColourName, @Red, @Green, @Blue );";

            SQLiteConnection objConn = new SQLiteConnection(Connection_String, true);
            objConn.Open();
            SQLiteCommand objCommand = objConn.CreateCommand();
            objCommand.CommandText = sql;
            objCommand.Parameters.AddWithValue("@Bloom_ID", Bloom_ID);
            objCommand.Parameters.AddWithValue("@BandIndex", BandIndex);
            objCommand.Parameters.AddWithValue("@UPOV_Colour_ID", UPOV_Colour_ID);
            objCommand.Parameters.AddWithValue("@BandWidth", BandWidth);
            objCommand.Parameters.AddWithValue("@ColourName", ColourName);
            objCommand.Parameters.AddWithValue("@Red", Red);
            objCommand.Parameters.AddWithValue("@Green", Green);
            objCommand.Parameters.AddWithValue("@Blue", Blue);

            try
            {
                objCommand.ExecuteNonQuery();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public static int InsertCommon_Name(string name)
        {
            string sql = @"INSERT INTO Common_Name (Name) VALUES (@name);";
            SQLiteConnection objConn = new SQLiteConnection(Connection_String, true);
            objConn.Open();
            SQLiteCommand objCommand = objConn.CreateCommand();
            objCommand.CommandText = sql;
            objCommand.Parameters.AddWithValue("@name", name);

            try
            {
                objCommand.ExecuteNonQuery();
            }
            catch (Exception)
            {
                return -1;
            }

            sql = "SELECT DISTINCT last_insert_rowid() FROM Common_Name;";
            objCommand.CommandText = sql;

            int i;

            try
            {
                i = Convert.ToInt32(objCommand.ExecuteScalar());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,"Could not get ID for inserted name",MessageBoxButtons.OK,MessageBoxIcon.Exclamation);
                return -1;
            }

            return i;
        }

        public static int InsertCultivar_Name(string cultivar)
        {
            string sql = @"INSERT INTO Cultivar_Name (Cultivar) VALUES (@cultivar);";
            SQLiteConnection objConn = new SQLiteConnection(Connection_String, true);
            objConn.Open();
            SQLiteCommand objCommand = objConn.CreateCommand();
            objCommand.CommandText = sql;
            objCommand.Parameters.AddWithValue("@cultivar", cultivar);

            try
            {
                objCommand.ExecuteNonQuery();
            }
            catch (Exception)
            {
                return -1;
            }
            sql = "SELECT DISTINCT last_insert_rowid() FROM Cultivar_Name;";
            objCommand.CommandText = sql;

            int i;

            try
            {
                i = Convert.ToInt32(objCommand.ExecuteScalar());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Could not get ID for inserted cutivar", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return -1;
            }

            return i;
        }

        #endregion

        static public DataSet GetEmptyBloomDataSet()
        {
            DataSet datasetBloom = new DataSet("Bloom_Data");

            SQLiteConnection objConn = new SQLiteConnection(Connection_String, true);
            objConn.Open();

            // Bloom table
            SQLiteDataAdapter adapterA = new SQLiteDataAdapter("SELECT *  FROM Bloom WHERE bloomid = '-1';", objConn);
            adapterA.FillSchema(datasetBloom,SchemaType.Source, "Bloom");
            adapterA.Dispose();

            // Photo_View
            SQLiteDataAdapter adapterB = new SQLiteDataAdapter("SELECT *  FROM Photo_View WHERE photoid = '<<';", objConn);
            adapterB.FillSchema(datasetBloom, SchemaType.Source, "Photo_View");
            adapterB.Dispose();

            // Bloom_Colour
            SQLiteDataAdapter adapterC = new SQLiteDataAdapter("SELECT *  FROM Bloom_Colour  WHERE bloom_id = '-1' ;", objConn);
            adapterC.FillSchema(datasetBloom, SchemaType.Source, "Bloom_Colour");
            adapterC.Dispose();

            // Bloom_Photo
            SQLiteDataAdapter adapterD = new SQLiteDataAdapter("SELECT *  FROM Bloom_Photo WHERE bloomid = '-1';", objConn);
            adapterD.FillSchema(datasetBloom, SchemaType.Source, "Bloom_Photo");
            adapterD.Dispose();

            return datasetBloom;
        }
        // ===============================================================================================================

        /// <summary>
        /// Fills a dataset with 4 tables containg the data for the listview selected item
        ///     Bloom
        ///     Bloom_Photo
        ///     BloomPhotoView
        ///     Bloom_Colour
        /// </summary>
        /// <param name="BloomID"></param>
        /// <returns>DataSet</returns>
        static public DataTable GetListViewSelectedItemData(string BloomID)
        {
            DataSet selectedItemData = new DataSet("SelectedItemData");

            SQLiteConnection objConn = new SQLiteConnection(Connection_String, true);
            objConn.Open();

            #region Long sql statment
            string mySQL = "SELECT  Bloom.Name, Bloom.ltnname, Bloom.stnname, Bloom.lctnname, " +
                            "Bloom.sctnname, Bloom.cultivar, Bloom.genus, Bloom.species, Bloom.Inflorescence, " +
                            "Bloom.width, Bloom.height, Bloom.shape, Bloom.size, Bloom_Photo.filename, " +
                            "Bloom_Photo.photoid, Bloom_Photo.cameraangle, Camera_Angle.description, Bloom_Photo.Thumbnail " +
                    "FROM Bloom " +
                    "INNER JOIN Bloom_Photo on Bloom.bloomid = Bloom_Photo.bloomid " +
                    "INNER JOIN Camera_Angle on Bloom_photo.cameraangle = Camera_Angle.name " +
                    "Where Bloom.bloomid = '" + BloomID + "' " +
                    "Order By Bloom_Photo.filename; "; 
            #endregion

            SQLiteDataAdapter adapterE = new SQLiteDataAdapter(mySQL, objConn);
            adapterE.Fill(selectedItemData, "Selected_Bloom_Data");
            adapterE.Dispose();

            return selectedItemData.Tables["Selected_Bloom_Data"]; 
        }

        /// <summary>
        /// Fills table with views for PhotoID
        /// </summary>
        /// <param name="PhotoID"></param>
        /// <returns>DataTable</returns>
        static public DataTable GetViewsForPhoto(string PhotoID)
        {
            string mySQL = "SELECT photoid, viewid, view_description " +
                "FROM Photo_View INNER JOIN Views ON Photo_View.viewid = Views.view_name " +
                "WHERE photoid = '" + PhotoID + "'; ";

            DataSet selectedItemData = new DataSet("PhotoViews");

            SQLiteConnection objConn = new SQLiteConnection(Connection_String, true);
            objConn.Open();

            SQLiteDataAdapter adapterA = new SQLiteDataAdapter(mySQL, objConn);
            adapterA.Fill(selectedItemData, "PhotoViews");
            adapterA.Dispose();

            return selectedItemData.Tables["PhotoViews"];
        }

        static public DataTable GetListViewSelectedItemColour(string BloomID)
        {
            DataSet datasetBloom = new DataSet("Bloom_Data");

            SQLiteConnection objConn = new SQLiteConnection(Connection_String, true);
            objConn.Open();

            string mySQL = "SELECT bloom_id, bandindex, bandwidth, upov_colour_id, colour, Red, Green, Blue " +
                            " FROM Bloom_Colour " +
                            " Where Bloom_ID = '" + BloomID + "';";

            SQLiteDataAdapter adapterC = new SQLiteDataAdapter(mySQL, objConn);
            adapterC.Fill(datasetBloom, "Bloom_Colour");
            adapterC.Dispose();

            return datasetBloom.Tables["Bloom_Colour"];
        }

 
        static public bool IsThereAnyInflorescenceMetaData()
        {
            SQLiteConnection objConn = new SQLiteConnection(Connection_String, true);
            objConn.Open();

            string mySQL = "SELECT count(name)  FROM Inflorescence;";
            SQLiteCommand objCommand = objConn.CreateCommand();
            objCommand.CommandText = mySQL;

            try
            {
                string count = objCommand.ExecuteScalar().ToString();
                if (!(count == "0")) return true;

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error:\n" + ex.Message, "Could not get view index", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return false;
        }


        static public bool IsThereAnyShapeMetaData()
        {
            SQLiteConnection objConn = new SQLiteConnection(Connection_String, true);
            objConn.Open();

            string mySQL = "SELECT count(name) FROM Shape;";
            SQLiteCommand objCommand = objConn.CreateCommand();
            objCommand.CommandText = mySQL;

            try
            {
                string count = objCommand.ExecuteScalar().ToString();
                if (!(count == "0")) return true;

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error:\n" + ex.Message, "Could not get view index", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return false;
        }

        static public DataSet GetSelectedData(string id, bool SelectAll)
        {
            string sql;

            //DataSet ds = Helper.CreateDataSet();
            DataSet ds = new DataSet("Bloom_Data");

            if(SelectAll)
            {
                DataTable Appx = new DataTable("Application");
                //Appx.TableName = ;
                Appx.Columns.Add("Application", typeof(string));
                Appx.Columns.Add("Version", typeof(string));
                Appx.Columns.Add("Database_version", typeof(string));
                Appx.Columns.Add("User_name", typeof(string));
                Appx.Columns.Add("Computer_name", typeof(string));
                Appx.Columns.Add("Export_date", typeof(string));

                Appx.Rows.Add(Application.ProductName, Properties.Settings.Default.ApplicationVersion, BloomData.GetDBVersionDisplay(), Helper.GetUserName(),
                            System.Environment.MachineName, DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
                ds.Tables.Add(Appx);
            }


            SQLiteConnection objConn = new SQLiteConnection(Connection_String, true);
            objConn.Open();

            /*
             *  Get data from Bloom table for selected bloom
             */
            if (SelectAll) { sql = "SELECT *  FROM Bloom;"; }
            else {sql = "SELECT *  FROM Bloom WHERE bloomid = '" + id + "';"; }

            SQLiteDataAdapter adapterb = new SQLiteDataAdapter(sql,objConn);
            adapterb.Fill(ds, "Bloom");
            adapterb.Dispose();

            /*
             *  Get data from Bloom_Colour table for selected bloom
             */
            if (SelectAll) { sql = "SELECT * FROM Bloom_Colour;"; }
            else { sql = "SELECT * FROM Bloom_Colour WHERE Bloom_ID = '" + id + "';"; }
                
            SQLiteDataAdapter adapterc = new SQLiteDataAdapter(sql, objConn);
            adapterc.Fill(ds, "Bloom_Colour");
            adapterc.Dispose();

            /*
             *  Get data from Bloom_Photo table for selected bloom
             */
            if (SelectAll) { sql = "SELECT * FROM Bloom_Photo;"; }
            else { sql = "SELECT * FROM Bloom_Photo  WHERE bloomid = '" + id + "';"; }
            
            SQLiteDataAdapter adapterp = new SQLiteDataAdapter(sql, objConn);
            adapterp.Fill(ds, "Bloom_Photo");
            adapterp.Dispose();

            /*
             *  Get data from BloomPhotoView table for selected bloom
             */
            if (SelectAll) { sql = "SELECT * FROM BloomPhotoView;"; }
            else { sql = "SELECT * FROM BloomPhotoView  WHERE bloomid = '" + id + "';"; }
            SQLiteDataAdapter adapterv = new SQLiteDataAdapter(sql, objConn);
            adapterv.Fill(ds, "BloomPhotoView");
            adapterv.Dispose();

            if (SelectAll)
            {
                sql = "SELECT * FROM bloomview;";
                SQLiteDataAdapter adaptbv = new SQLiteDataAdapter(sql, objConn);
                adaptbv.Fill(ds, "bloomView");
                adaptbv.Dispose();

                sql = "SELECT * FROM Common_Name;";
                SQLiteDataAdapter adaptcn = new SQLiteDataAdapter(sql, objConn);
                adaptcn.Fill(ds, "Common_Name");
                adaptcn.Dispose();

                sql = "SELECT * FROM Cultivar;";
                SQLiteDataAdapter adaptc = new SQLiteDataAdapter(sql, objConn);
                adaptc.Fill(ds, "Cultivar");
                adaptc.Dispose();

                sql = "SELECT * FROM databaseversion;";
                SQLiteDataAdapter adaptdbv = new SQLiteDataAdapter(sql, objConn);
                adaptdbv.Fill(ds, "databaseversion");
                adaptdbv.Dispose();

                sql = "SELECT * FROM Genus;";
                SQLiteDataAdapter adaptg = new SQLiteDataAdapter(sql, objConn);
                adaptg.Fill(ds, "Genus");
                adaptg.Dispose();

                sql = "SELECT * FROM Shape;";
                SQLiteDataAdapter adapts = new SQLiteDataAdapter(sql, objConn);
                adapts.Fill(ds, "Shape");
                adapts.Dispose();

                sql = "SELECT * FROM Species;";
                SQLiteDataAdapter adaptsp = new SQLiteDataAdapter(sql, objConn);
                adaptsp.Fill(ds, "Species");
                adaptsp.Dispose();

                sql = "SELECT* FROM UPOV_Colour;";
                SQLiteDataAdapter adaptup = new SQLiteDataAdapter(sql, objConn);
                adaptup.Fill(ds, "UPOV_Colour");
                adaptup.Dispose();

            }

            objConn.Close();

            return ds;
        }

        static public DataTable getUPOVColours()
        {
            DataTable ColourDomain = new DataTable();
            ColourDomain.Columns.Add("Name", typeof(string));
            ColourDomain.Columns.Add("Red", typeof(int));
            ColourDomain.Columns.Add("Green", typeof(int));
            ColourDomain.Columns.Add("Blue", typeof(int));
            ColourDomain.Columns.Add("ID", typeof(int));
            ColourDomain.TableName = "Colours";
            SQLiteConnection objConn = new SQLiteConnection(Connection_String, true);
            string sql = "SELECT upov_id || ' - ' || name As Name, red as Red, green as Green, " +
                "blue as Blue, upov_id as ID FROM Colour Order by upov_id;";
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(sql, objConn);
            SQLiteCommandBuilder command = new SQLiteCommandBuilder(adapter);
            adapter.Fill(ColourDomain);
            return ColourDomain;
        }

        static public DataTable GetCommonNameAutoComplete()
        {
            DataTable CommonNameAutoComplete = new DataTable();
            CommonNameAutoComplete.TableName = "CommonNameAutoComplete";
            CommonNameAutoComplete.Columns.Add("Common_Name", typeof(string));

            SQLiteConnection objConn = new SQLiteConnection(Connection_String, true);
            string sql = "SELECT Name FROM Common_Name;";
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(sql, objConn);
            SQLiteCommandBuilder command = new SQLiteCommandBuilder(adapter);
            adapter.Fill(CommonNameAutoComplete);
            return CommonNameAutoComplete;
        }

        static public DataTable GetCultivarAutoComplete()
        {
            DataTable CultivarAutoComplete = new DataTable();
            CultivarAutoComplete.TableName = "CultivarAutoComplete";
            CultivarAutoComplete.Columns.Add("Name", typeof(string));

            SQLiteConnection objConn = new SQLiteConnection(Connection_String, true);
            string sql = "SELECT Name FROM Cultivar;";
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(sql, objConn);
            SQLiteCommandBuilder command = new SQLiteCommandBuilder(adapter);
            adapter.Fill(CultivarAutoComplete);
            return CultivarAutoComplete;
        }

        static public DataTable GetGenusAutoComplete()
        {
            DataTable GenusAutoComplete = new DataTable();
            GenusAutoComplete.TableName = "GenusAutoComplete";
            GenusAutoComplete.Columns.Add("Name", typeof(string));
            SQLiteConnection objConn = new SQLiteConnection(Connection_String, true);
            string sql = "SELECT Name FROM Genus;";
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(sql, objConn);
            SQLiteCommandBuilder command = new SQLiteCommandBuilder(adapter);
            adapter.Fill(GenusAutoComplete);
            return GenusAutoComplete;
        }

        static public DataTable GetSpeciesAutoComplete()
        {
            //SELECT Species_ID,       Species,       Genus_ID  FROM Species_Name;
            DataTable SpeciesAutoComplete = new DataTable();
            SpeciesAutoComplete.TableName = "SpeciesAutoComplete";
            SpeciesAutoComplete.Columns.Add("Name", typeof(string));
            SQLiteConnection objConn = new SQLiteConnection(Connection_String, true);
            string sql = "SELECT Name FROM Species;";
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(sql, objConn);
            SQLiteCommandBuilder command = new SQLiteCommandBuilder(adapter);
            adapter.Fill(SpeciesAutoComplete);
            return SpeciesAutoComplete;
        }
        static public DataTable GetCommonNameSelect()
        {
            DataTable CommonNameSelect = new DataTable();
            CommonNameSelect.TableName = "CommonNameSelect";
            CommonNameSelect.Columns.Add("ID", typeof(int));
            CommonNameSelect.Columns.Add("Name", typeof(string));

            SQLiteConnection objConn = new SQLiteConnection(Connection_String, true);
            string sql = "SELECT ROWID AS ID, Name FROM Common_Name;";
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(sql, objConn);
            SQLiteCommandBuilder command = new SQLiteCommandBuilder(adapter);
            adapter.Fill(CommonNameSelect);

            return CommonNameSelect;
        }

        static public DataTable GetCultivarSelect()
        {
            DataTable CultivarSelect = new DataTable();
            CultivarSelect.TableName = "CultivarSelect";
            CultivarSelect.Columns.Add("ID", typeof(int));
            CultivarSelect.Columns.Add("Name", typeof(string));

            SQLiteConnection objConn = new SQLiteConnection(Connection_String, true);
            string sql = "SELECT ROWID AS ID,  Name FROM Cultivar; ";
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(sql, objConn);
            SQLiteCommandBuilder command = new SQLiteCommandBuilder(adapter);
            adapter.Fill(CultivarSelect);

            return CultivarSelect;
        }

        static public DataTable GetGenusSelect()
        {
            DataTable GenusSelect = new DataTable();
            GenusSelect.TableName = "GenusSelect";
            GenusSelect.Columns.Add("ID", typeof(int));
            GenusSelect.Columns.Add("Name", typeof(string));

            SQLiteConnection objConn = new SQLiteConnection(Connection_String, true);
            string sql = "SELECT ROWID AS ID, Name FROM Genus; ";
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(sql, objConn);
            SQLiteCommandBuilder command = new SQLiteCommandBuilder(adapter);
            adapter.Fill(GenusSelect);

            return GenusSelect;
        }

        static public DataTable GetSpeciesSelect()
        {
            DataTable SpeciesSelect = new DataTable();
            SpeciesSelect.TableName = "SpeciesSelect";
            SpeciesSelect.Columns.Add("ID", typeof(int));
            SpeciesSelect.Columns.Add("Name", typeof(string));

            SQLiteConnection objConn = new SQLiteConnection(Connection_String, true);
            string sql = "SELECT ROWID AS ID, Name FROM Species; ";
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(sql, objConn);
            SQLiteCommandBuilder command = new SQLiteCommandBuilder(adapter);
            adapter.Fill(SpeciesSelect);

            return SpeciesSelect;
        }

        static public string GetGenusOfSpecies(string species)
        {

              string sql = @"SELECT  [Genus] FROM [Species] where [name] = @species;";
  //          string sql = @"SELECT  [Genus] FROM [Species] where [name] = '" + species + "';";

            SQLiteConnection objConn = new SQLiteConnection(Connection_String, true);
            objConn.Open();
            SQLiteCommand objCommand = objConn.CreateCommand();
            objCommand.CommandText = sql;
            objCommand.Parameters.AddWithValue("@species", species);

            try
            {
                string genus = objCommand.ExecuteScalar().ToString();
                return genus;

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error:\n" + ex.Message, "Could not get view index", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return "";
            }
        }

        static public string GetBloomViewName(int viewID)
        {
            string sql = @"SELECT  bloomviewname  FROM bloomview where bloomviewid = " + viewID + ";";
            SQLiteConnection objConn = new SQLiteConnection(Connection_String, true);
            SQLiteCommand  objcomm = new SQLiteCommand(sql, objConn);
            objConn.Open();
            string s = (string)objcomm.ExecuteScalar();
            objConn.Close();
            return s;
        }

        static public DataTable GetViews()
        {
            DataTable Views = new DataTable();
            Views.TableName = "View";

            SQLiteConnection objConn = new SQLiteConnection(Connection_String, true);
            string sql = "SELECT *  FROM Views;";
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(sql, objConn);
            SQLiteCommandBuilder command = new SQLiteCommandBuilder(adapter);
            adapter.Fill(Views);

            return Views;
        }


        public static int GetBloomViewID(string bloomviewname)
        {
            Int32 id = -1;

            string sql = @"SELECT bloomviewid FROM bloomview where bloomviewname = @bloomviewname ;";

            SQLiteConnection objConn = new SQLiteConnection(Connection_String, true);
            objConn.Open();
            SQLiteCommand objCommand = objConn.CreateCommand();
            objCommand.CommandText = sql;
            objCommand.Parameters.AddWithValue("@bloomviewname", bloomviewname);

            try
            {
              id = Convert.ToInt32(objCommand.ExecuteScalar());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error:\n" + ex.Message, "Could not get view index", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
            return id;
        }

        static public DataTable GetSelectListData(string SearchTerm = "+=+=")
        {
            string sql;

            if (SearchTerm == "+=+=")
            { 
            sql = @"SELECT bloomid, ltnname, stnname, lctnname, sctnname,
                    name, cultivar, genus, species FROM Bloom;";
            }
            else
            {
                sql = @"SELECT bloomid, ltnname, stnname, lctnname, sctnname,
                    name, cultivar, genus, species FROM Bloom
                    WHERE name LIKE @SearchTerm OR cultivar LIKE @SearchTerm OR genus LIKE @SearchTerm OR Species LIKE @SearchTerm;";
            }

            // WHERE Name LIKE '%Gerb%' OR Cultivar LIKE '%Gerb%' OR Genus LIKE '%Gerb%' OR Species LIKE '%Gerb%'
            //CommBloom.Parameters.AddWithValue("@Shape", row.Field<string>("Shape"));
            DataTable SelectListData = new DataTable();
            SelectListData.TableName = "SelectListData";
            SelectListData.Columns.Add("bloomid", typeof(string));
            SelectListData.Columns.Add("ltnname", typeof(string));
            SelectListData.Columns.Add("stnname", typeof(string));
            SelectListData.Columns.Add("lctnname", typeof(string));
            SelectListData.Columns.Add("sctnname", typeof(string));
            SelectListData.Columns.Add("name", typeof(string));
            SelectListData.Columns.Add("cultivar", typeof(string));
            SelectListData.Columns.Add("genus", typeof(string));
            SelectListData.Columns.Add("species", typeof(string));

            SQLiteConnection objConn = new SQLiteConnection(Connection_String, true);
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(sql, objConn);
            adapter.SelectCommand.Parameters.AddWithValue("@SearchTerm", "%" + SearchTerm + "%");
            SQLiteCommandBuilder command = new SQLiteCommandBuilder(adapter);
            
            adapter.Fill(SelectListData);

            return SelectListData;

        }

        static public DataTable GetDomainOfNames()
        {
            string sql = @"SELECT  Taxonomy.Rank AS Rank, Name, Name_ID, Taxonomy.Taxonomy_ID , Parent_ID
                      FROM Name  INNER JOIN Taxonomy on name.Taxonomy_ID = Taxonomy.Taxonomy_ID;";

            DataTable DomainOfNames = new DataTable();
            DomainOfNames.TableName = "DomainOfNames";
            DomainOfNames.Columns.Add("Rank", typeof(string));
            DomainOfNames.Columns.Add("Name", typeof(string));
            DomainOfNames.Columns.Add("Name_ID", typeof(int));
            DomainOfNames.Columns.Add("Taxonomy_ID", typeof(int));
            DomainOfNames.Columns.Add("Parent_ID", typeof(int));


            SQLiteConnection objConn = new SQLiteConnection(Connection_String, true);
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(sql, objConn);
            SQLiteCommandBuilder command = new SQLiteCommandBuilder(adapter);
            adapter.Fill(DomainOfNames);

            return DomainOfNames;


        }

        /// <summary>
        /// Returns a data table of all names that can be used to autofill a text box
        /// </summary>
        /// <returns></returns>
        static public DataTable GetNameAutoFill()
        {
            DataTable NameAutoFill = new DataTable();
            NameAutoFill.Columns.Add("Name", typeof(string));
            NameAutoFill.TableName = "NameAutoFill";
            SQLiteConnection objConn = new SQLiteConnection(Connection_String, true);
            string sql = "SELECT Name  FROM Name order by name;";
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(sql, objConn);
            SQLiteCommandBuilder command = new SQLiteCommandBuilder(adapter);
            adapter.Fill(NameAutoFill);
            return NameAutoFill;
        }


        // =========================================================================================================
        // ================================== Inport/Export Data ===================================================
        #region Inport/Export Data
        static public DataSet getExportData()
        {
            DataSet FlowerSpaceData = new DataSet("Flower Space Export Data");

            SQLiteConnection objConn = new SQLiteConnection(Connection_String, true);

            FlowerSpaceData.Tables.Add(CreateApplicationTable());

            // Fill List all tables
            SQLiteDataAdapter adapterTables = new SQLiteDataAdapter("SELECT  name FROM sqlite_master WHERE  type = 'table' AND  name NOT LIKE 'sqlite_%' order by name;", objConn);
            SQLiteCommandBuilder commandTables = new SQLiteCommandBuilder(adapterTables);
            adapterTables.Fill(FlowerSpaceData, "Tables");
            adapterTables.Dispose();


            // Fill Cultivar
            SQLiteDataAdapter adapterCultivar = new SQLiteDataAdapter("SELECT * FROM Cultivar order by Name;", objConn);
            SQLiteCommandBuilder commandCultivar = new SQLiteCommandBuilder(adapterCultivar);
            adapterCultivar.Fill(FlowerSpaceData, "Cultivar");
            adapterCultivar.Dispose();

            // Fill Genus
            SQLiteDataAdapter adapterGenus = new SQLiteDataAdapter("SELECT * FROM Genus order by Name;", objConn);
            SQLiteCommandBuilder commandGenus = new SQLiteCommandBuilder(adapterGenus);
            adapterGenus.Fill(FlowerSpaceData, "Genus");
            adapterGenus.Dispose();

            // Fill databaseversion
            SQLiteDataAdapter adapterdatabaseversion = new SQLiteDataAdapter("SELECT * FROM databaseversion;", objConn);
            SQLiteCommandBuilder commanddatabaseversion = new SQLiteCommandBuilder(adapterdatabaseversion);
            adapterdatabaseversion.Fill(FlowerSpaceData, "databaseversion");
            adapterdatabaseversion.Dispose();

            // Fill Bloom
            SQLiteDataAdapter adapterBloom = new SQLiteDataAdapter("SELECT * FROM Bloom;", objConn);
            SQLiteCommandBuilder commandBloom = new SQLiteCommandBuilder(adapterBloom);
            adapterBloom.Fill(FlowerSpaceData, "Bloom");
            adapterBloom.Dispose();

            // Fill Bloom_Photo
            SQLiteDataAdapter adapterBloom_Photo = new SQLiteDataAdapter("SELECT * FROM Bloom_Photo;", objConn);
            SQLiteCommandBuilder commandBloom_Photo = new SQLiteCommandBuilder(adapterBloom_Photo);
            adapterBloom_Photo.Fill(FlowerSpaceData, "Bloom_Photo");
            adapterBloom_Photo.Dispose();

            // Fill BloomPhotoView
            SQLiteDataAdapter adapterBloomPhotoView = new SQLiteDataAdapter("SELECT * FROM BloomPhotoView;", objConn);
            SQLiteCommandBuilder commandBloomPhotoView = new SQLiteCommandBuilder(adapterBloomPhotoView);
            adapterBloomPhotoView.Fill(FlowerSpaceData, "BloomPhotoView");
            adapterBloomPhotoView.Dispose();

            // Fill bloomview
            SQLiteDataAdapter adapterbloomview = new SQLiteDataAdapter("SELECT * FROM bloomview;", objConn);
            SQLiteCommandBuilder commandbloomview = new SQLiteCommandBuilder(adapterbloomview);
            adapterbloomview.Fill(FlowerSpaceData, "bloomview");
            adapterbloomview.Dispose();

            // Fill Common_Name
            SQLiteDataAdapter adapterCommon_Name = new SQLiteDataAdapter("SELECT * FROM Common_Name;", objConn);
            SQLiteCommandBuilder commandCommon_Name = new SQLiteCommandBuilder(adapterCommon_Name);
            adapterCommon_Name.Fill(FlowerSpaceData, "Common_Name");
            adapterCommon_Name.Dispose();

            // Fill Camera Angle
            SQLiteDataAdapter adapter = new SQLiteDataAdapter("SELECT * FROM Camera_Angle order by Name;", objConn);
            SQLiteCommandBuilder command = new SQLiteCommandBuilder(adapter);
            adapter.Fill(FlowerSpaceData, "Camera_Angle");
            adapter.Dispose();

            // Fill Shape
            SQLiteDataAdapter adapterShape = new SQLiteDataAdapter("SELECT * FROM Shape order by Name;", objConn);
            SQLiteCommandBuilder commandShape = new SQLiteCommandBuilder(adapterShape);
            adapterShape.Fill(FlowerSpaceData, "Shape");
            adapterShape.Dispose();

            // Fill Species
            SQLiteDataAdapter adapterSpecies = new SQLiteDataAdapter("SELECT * FROM Species order by Name;", objConn);
            SQLiteCommandBuilder commandSpecies = new SQLiteCommandBuilder(adapterSpecies);
            adapterSpecies.Fill(FlowerSpaceData, "Species");
            adapterSpecies.Dispose();

            // Fill Colour
            SQLiteDataAdapter adapterColour = new SQLiteDataAdapter("SELECT * FROM UPOV_Colour  Order by UPOV_Colour_ID;", objConn);
            SQLiteCommandBuilder commandColour = new SQLiteCommandBuilder(adapterColour);
            adapterColour.Fill(FlowerSpaceData, "UPOV_Colour");
            adapter.Dispose();

            return FlowerSpaceData;
        }
        #endregion
        // Inport/Export Data



        // =========================================================================================================
        // =========================================== Manage meta data ============================================
        #region Manage meta data



        /// <summary>
        /// Gets a Data Set with 8 tables containing the application meta data being:
        ///     Camera Angle
        ///     bloomview
        ///     Common_Name
        ///     Cultivar
        ///     Genus
        ///     Shape
        ///     Species
        ///     Colour
        ///     
        /// This is static within the application and available throughout
        /// </summary>
        /// <returns></returns>
        static public DataSet getMetaData()
        {
            DataSet MetaData = new DataSet("MetaData");

            SQLiteConnection objConn = new SQLiteConnection(Connection_String, true);


            // Fill Camera Angle
            SQLiteDataAdapter adapter = new SQLiteDataAdapter("SELECT * FROM Camera_Angle order by Name;", objConn);
            SQLiteCommandBuilder command = new SQLiteCommandBuilder(adapter);
            adapter.Fill(MetaData, "Camera_Angle");
            adapter.Dispose();

            // Fill Views
            SQLiteDataAdapter adapterview = new SQLiteDataAdapter("SELECT * FROM Views order by view_name;", objConn);
            SQLiteCommandBuilder commandbloomview = new SQLiteCommandBuilder(adapterview);
            adapterview.Fill(MetaData, "Views");
            adapterview.Dispose();

             // Fill Common_Name
            SQLiteDataAdapter adapterCommon_Name = new SQLiteDataAdapter("SELECT * FROM Common_Name order by Name;", objConn);
            SQLiteCommandBuilder commandCommon_Name = new SQLiteCommandBuilder(adapterCommon_Name);
            adapterCommon_Name.Fill(MetaData, "Common_Name");
            adapterCommon_Name.Dispose();

            // Fill Cultivar
            SQLiteDataAdapter adapterCultivar = new SQLiteDataAdapter("SELECT * FROM Cultivar order by Name;", objConn);
            SQLiteCommandBuilder commandCultivar = new SQLiteCommandBuilder(adapterCultivar);
            adapterCultivar.Fill(MetaData, "Cultivar");
            adapterCultivar.Dispose();

            // Fill Genus
            SQLiteDataAdapter adapterGenus = new SQLiteDataAdapter("SELECT * FROM Genus order by Name;", objConn);
            SQLiteCommandBuilder commandGenus = new SQLiteCommandBuilder(adapterGenus);
            adapterGenus.Fill(MetaData, "Genus");
            adapterGenus.Dispose();

            // Fill Species
            SQLiteDataAdapter adapterSpecies = new SQLiteDataAdapter("SELECT * FROM Species order by Name;", objConn);
            SQLiteCommandBuilder commandSpecies = new SQLiteCommandBuilder(adapterSpecies);
            adapterSpecies.Fill(MetaData, "Species");
            adapterSpecies.Dispose();

            // Fill Colour
            SQLiteDataAdapter adapterColour = new SQLiteDataAdapter("SELECT * FROM Colour  Order by id;", objConn);
            SQLiteCommandBuilder commandColour = new SQLiteCommandBuilder(adapterColour);
            adapterColour.Fill(MetaData, "Colour");
            adapter.Dispose();

            // Fill Inflorescence
            SQLiteDataAdapter adapterInflorescence = new SQLiteDataAdapter("SELECT * FROM Inflorescence  Order by Name;", objConn);
            SQLiteCommandBuilder commandInflorescence = new SQLiteCommandBuilder(adapterInflorescence);
            adapterInflorescence.Fill(MetaData, "Inflorescence");
            adapter.Dispose();

            // Fill Shape
            SQLiteDataAdapter adapterShape = new SQLiteDataAdapter("SELECT * FROM Shape order by Name;", objConn);
            SQLiteCommandBuilder commandShape = new SQLiteCommandBuilder(adapterShape);
            adapterShape.Fill(MetaData, "Shape");
            adapterShape.Dispose();

            return MetaData;
        }

        #region Camera Angle
        static public DataTable GetCamera_Angle_Table()
        {
            SQLiteConnection objConn;
            try
            {
                objConn = new SQLiteConnection(Connection_String, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fatel error \n" + ex.Message, "Can't open database", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }

            DataSet ds = new DataSet("ds");

            string sql = "SELECT * FROM Camera_Angle order by Name;";
            SQLiteDataAdapter adapts = new SQLiteDataAdapter(sql, objConn);
            adapts.Fill(ds, "Camera_Angle");
            adapts.Dispose();

            return ds.Tables["Camera_Angle"];
        }

        static public DataSet CreateCameraAngleMetaDataDataSet(bool includeData = true)
        {
            DataSet CameraAngleMetaData = new DataSet("Camera_Angle");

            CameraAngleMetaData.Tables.Add(CreateApplicationTable());
            CameraAngleMetaData.Tables.Add(CreateCameraAngleInstructionTable());
            CameraAngleMetaData.Tables.Add(CreateCameraAngleTable());

            if (includeData) LoadCamera_AngleTable(CameraAngleMetaData);

            return CameraAngleMetaData;
        }

        static private DataTable CreateCameraAngleInstructionTable()
        {
            DataTable Instruction = new DataTable("Instructions");
            Instruction.Columns.Add("Heading", typeof(string));
            Instruction.Columns.Add("Instruction", typeof(string));

            string inst = "\nINSTRUCTIONS\nRead the manual\n\n\t1. DO NOT change the name of existing camera angle. They look like this {<Name>Funnelform</Name>}. EVEN IF IT IS MISSPELT, this is an identifier.\n" +
                "\t2. The name must be unique.\n" +
                "\t3. Images must be in the same folder as this XML file.\n" +
                "\t4. The image name you enter into this file must match the image file name exactly.\n" +
                 "\nAll records in the data base will be replaced with the data below.\n";

            Instruction.Rows.Add("Instructions", inst);

            return Instruction;
        }

        static private DataTable CreateCameraAngleTable()
        {
            DataTable Camera_Angle = new DataTable("Camera_Angle");
            Camera_Angle.Columns.Add("Name", typeof(string));
            Camera_Angle.Columns.Add("Description", typeof(string));
            Camera_Angle.Columns.Add("ImageName", typeof(string));

            return Camera_Angle;

        }

        static private void LoadCamera_AngleTable(DataSet ds)
        {
            SQLiteConnection objConn;
            try
            {
                objConn = new SQLiteConnection(Connection_String, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fatel error \n" + ex.Message, "Can't open database", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }

            string sql = "SELECT * FROM Camera_Angle order by Name;";
            SQLiteDataAdapter adapts = new SQLiteDataAdapter(sql, objConn);
            adapts.Fill(ds, "Camera_Angle");
            adapts.Dispose();

        }

        static public bool InsertCamera_AngleMetaData(DataTable dt)
        {
            SQLiteConnection objConn = new SQLiteConnection(Connection_String, true);  // Connect to the data base
            objConn.Open();                                                            // Open the connection

            using (var transaction = objConn.BeginTransaction())
            {

                SQLiteCommand objDeleteCommand = objConn.CreateCommand();
                objDeleteCommand.CommandText = "DELETE FROM Camera_Angle";

                // Delete existing data from the Shape table
                try
                {
                    objDeleteCommand.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();   // If we fail
                    MessageBox.Show("Error message\n" + ex.Message, "Apply metta data aborted", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                // Now lets try filling it again
                SQLiteCommand objInsertCommand = objConn.CreateCommand();
                objInsertCommand.CommandText = @"INSERT INTO Camera_Angle (Name, Description, ImageName) VALUES (@Name, @Description, @ImageName);";

                foreach (DataRow row in dt.Rows)
                {
                    objInsertCommand.Parameters.Clear();
                    objInsertCommand.Parameters.AddWithValue("@Name", row.Field<string>("Name"));
                    objInsertCommand.Parameters.AddWithValue("@Description", row.Field<string>("Description"));
                    objInsertCommand.Parameters.AddWithValue("@ImageName", row.Field<string>("ImageName"));

                    try
                    {
                        objInsertCommand.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show("Error message\n" + ex.Message, "Apply metta data aborted", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }

                transaction.Commit();
            }

            return true;
        }
        #endregion  // Camera Angle

        #region bloomview  
        /// <summary>
        /// Create and populate a data set to be saved to XML
        /// The XML to be edited to add/change the colour meta data
        /// </summary>
        static public DataSet CreateEditbloomviewMetaDataDataSet()
        {
            DataSet viewsMetaData = new DataSet("bloomviewMetaData");

            viewsMetaData.Tables.Add(CreateApplicationTable());
            viewsMetaData.Tables.Add(CreatebloomviewInstructionTable());

            DataTable dt = getMetaData().Tables["Views"].Copy();
            viewsMetaData.Tables.Add(dt);


            return viewsMetaData;
        }

        /// <summary>
        /// Table data contains instructions for editing data
        /// </summary>
        /// <returns></returns>
        static private DataTable CreatebloomviewInstructionTable()
        {
            DataTable Instruction = new DataTable("Instructions");
            Instruction.Columns.Add("Heading", typeof(string));
            Instruction.Columns.Add("Instruction", typeof(string));

            string inst = "\nINSTRUCTIONS\nRead the manual\n\n" +
                "\t1. All columns must have a value.\n" +
                "\t2. BloomviewID must be unique.\n" +
                "\t3. Bloomviewname should be meaningfull in terms of what this view of bloom shows or is used for.\n" +
                "\t4. Description should describe the view such that the user can determin if a video stream is showing this view.\n" +
                "\tAll records in the data base will be replaced with the data below.\n";

            Instruction.Rows.Add("Instructions", inst);

            return Instruction;
        }

        /// <summary>
        /// Delete all data from the Genus table and replace it with the contents of the XML file
        /// This allows remote editing of metadata
        /// </summary>
        /// <param name="bloomviewMetaData"></param>
        /// <returns></returns>
        public static bool InsertbloomviewMetaData(DataTable viewsMetaData)
        {
            SQLiteConnection objConn = new SQLiteConnection(Connection_String, true);  // Connect to the data base
            objConn.Open();                                                            // Open the connection

            using (var transaction = objConn.BeginTransaction())                       // Start roolback transaction
            {
                SQLiteCommand objDeleteCommand = objConn.CreateCommand();
                objDeleteCommand.CommandText = "DELETE FROM views";

                // Delete existing data from the Shape table
                try
                {
                    objDeleteCommand.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();   // If we fail
                    MessageBox.Show("Error message\n" + ex.Message, "Apply metta data aborted", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                // Now lets try filling it again
                SQLiteCommand objInsertCommand = objConn.CreateCommand();
                objInsertCommand.CommandText = @"INSERT INTO views(view_name, view_description) VALUES(@view_name, @view_description);";
                
                foreach (DataRow row in viewsMetaData.Rows)
                {

                    try   // Inserting the data
                    {
                        objInsertCommand.Parameters.Clear();
                        objInsertCommand.Parameters.AddWithValue("@view_name", row.Field<string>("view_name"));
                        objInsertCommand.Parameters.AddWithValue("view_description", row.Field<string>("view_description"));
                        objInsertCommand.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();   // If we fail
                        MessageBox.Show("Error message\n" + ex.Message,"Apply metta data aborted",MessageBoxButtons.OK,MessageBoxIcon.Error);
                        return false;
                    }
                }

                transaction.Commit();
            }

            return true;
        }
        //InsertbloomviewMetaData

        #endregion  // bloomview
        // Bloomview

        #region Common_Name
        /// <summary>
        /// Create and populate a data set to be saved to XML
        /// The XML to be edited to add/change the colour meta data
        /// </summary>
        static public DataSet CreateEditCommon_NameMetaDataDataSet()
        {
            DataSet Common_NameMetaData = new DataSet("Common_NameMetaData");

            Common_NameMetaData.Tables.Add(CreateApplicationTable());
            Common_NameMetaData.Tables.Add(CreateCommon_NameInstructionTable());

            DataTable dt = getMetaData().Tables["Common_Name"].Copy();
            Common_NameMetaData.Tables.Add(dt);


            return Common_NameMetaData;
        }

        /// <summary>
        /// Table data contains instructions for editing data
        /// </summary>
        /// <returns></returns>
        static private DataTable CreateCommon_NameInstructionTable()
        {
            DataTable Instruction = new DataTable("Instructions");
            Instruction.Columns.Add("Heading", typeof(string));
            Instruction.Columns.Add("Instruction", typeof(string));

            string inst = "\nINSTRUCTIONS\nRead the manual\n\n" +
                "\t1. All columns must have a value.\n" +
                "\t2. Name must be the common name of a flower or other plant.\n" +
                "\nAll records in the data base will be replaced with the data below.\n";

            Instruction.Rows.Add("Instructions", inst);

            return Instruction;
        }

        /// <summary>
        /// Delete all data from the Genus table and replace it with the contents of the XML file
        /// This allows remote editing of metadata
        /// </summary>
        /// <param name="CultivarMetaData"></param>
        /// <returns></returns>
        public static bool InsertCommon_NameMetaData(DataTable CultivarMetaData)
        {
            SQLiteConnection objConn = new SQLiteConnection(Connection_String, true);  // Connect to the data base
            objConn.Open();                                                            // Open the connection

            using (var transaction = objConn.BeginTransaction())
            {

                SQLiteCommand objDeleteCommand = objConn.CreateCommand();
                objDeleteCommand.CommandText = "DELETE FROM Common_Name";

                // Delete existing data from the Common_Name table
                try
                {
                    objDeleteCommand.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();   // If we fail
                    MessageBox.Show("Error message\n" + ex.Message, "Apply metta data aborted", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                // Now lets try filling it again
                SQLiteCommand objInsertCommand = objConn.CreateCommand();
                objInsertCommand.CommandText = @"INSERT INTO Common_Name(Name) VALUES( @Name );";
                
                foreach (DataRow row in CultivarMetaData.Rows)
                {
                    try
                    {
                        objInsertCommand.Parameters.Clear();
                        objInsertCommand.Parameters.AddWithValue("@Name", row.Field<string>("Name"));
                        objInsertCommand.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();             // If we fail
                        MessageBox.Show("Error message\n" + ex.Message, "Apply metta data aborted", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        return false;
                    }
                }
                transaction.Commit();
            }

            return true;
        }

        #endregion
        // Common_Name

        #region Cultivar
        /// <summary>
        /// Create and populate a data set to be saved to XML
        /// The XML to be edited to add/change the colour meta data
        /// </summary>
        static public DataSet CreateEditCultivarMetaDataDataSet()
        {
            DataSet CultivarMetaData = new DataSet("CultivarMetaData");

            CultivarMetaData.Tables.Add(CreateApplicationTable());
            CultivarMetaData.Tables.Add(CreateCultivarInstructionTable());

            //DataSet metaData = getMetaData();
            DataTable dt = getMetaData().Tables["Cultivar"].Copy();
            CultivarMetaData.Tables.Add(dt);


            return CultivarMetaData;
        }

        /// <summary>
        /// Table data contains instructions for editing data
        /// </summary>
        /// <returns></returns>
        static private DataTable CreateCultivarInstructionTable()
        {
            DataTable Instruction = new DataTable("Instructions");
            Instruction.Columns.Add("Heading", typeof(string));
            Instruction.Columns.Add("Instruction", typeof(string));

            string inst = "\nINSTRUCTIONS\nRead the manual\n\n" +
                "\t1. All columns must have a value.\n" +
                "\t2. Name must be unique and be a Genus in the botanical taxonomy\n" +
                "\n\tIts recomended you review the Species data before amending the Species data.\n" +
                "\tAll records in the data base will be replaced with the data below.\n";

            Instruction.Rows.Add("Instructions", inst);

            return Instruction;
        }

        /// <summary>
        /// Delete all data from the Genus table and replace it with the contents of the XML file
        /// This allows remote editing of metadata
        /// </summary>
        /// <param name="CultivarMetaData"></param>
        /// <returns></returns>
        public static bool InsertCultivarMetaData(DataTable CultivarMetaData)
        {
            SQLiteConnection objConn = new SQLiteConnection(Connection_String, true);  // Connect to the data base
            objConn.Open();                                                            // Open the connection

            using (var transaction = objConn.BeginTransaction())
            {

                SQLiteCommand objDeleteCommand = objConn.CreateCommand();
                objDeleteCommand.CommandText = "DELETE FROM Cultivar";

                // Delete existing data from the Cultivar table
                try
                {
                    objDeleteCommand.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();   // If we fail
                    MessageBox.Show(ex.Message);    //TODO:  Inprove the message 
                    return false;
                }

                // Now lets try filling it again
                SQLiteCommand objInsertCommand = objConn.CreateCommand();
                objInsertCommand.CommandText = @"INSERT INTO Cultivar ( Name ) VALUES ( @Name );";

                foreach (DataRow row in CultivarMetaData.Rows)
                {

                    try
                    {
                        objInsertCommand.Parameters.Clear();
                        objInsertCommand.Parameters.AddWithValue("@Name", row.Field<string>("Name"));
                        objInsertCommand.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();   // If we fail
                        MessageBox.Show("Error message\n" + ex.Message, "Apply metta data aborted", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }

                transaction.Commit();
            }

            return true;
        }

        #endregion
        // Cultivar

        #region Genus
        /// <summary>
        /// Create and populate a data set to be saved to XML
        /// The XML to be edited to add/change the colour meta data
        /// </summary>
        static public DataSet CreateEditGenusMetaDataDataSet()
        {
            DataSet GenusMetaData = new DataSet("GenusMetaData");

            GenusMetaData.Tables.Add(CreateApplicationTable());
            GenusMetaData.Tables.Add(CreateGenusInstructionTable());

            //DataSet metaData = getMetaData();
            DataTable dt = getMetaData().Tables["Genus"].Copy();
            GenusMetaData.Tables.Add(dt);


            return GenusMetaData;
        }

        /// <summary>
        /// Table data contains instructions for editing data
        /// </summary>
        /// <returns></returns>
        static private DataTable CreateGenusInstructionTable()
        {
            DataTable Instruction = new DataTable("Instructions");
            Instruction.Columns.Add("Heading", typeof(string));
            Instruction.Columns.Add("Instruction", typeof(string));

            string inst = "\nINSTRUCTIONS\nRead the manual\n\n" +
                "\t1. All columns must have a value.\n" +
                "\t2. Name must be unique and be a Genus in the botanical taxonomy\n" +
                "\n\tIts recomended you review the Species data before amending the Species data.\n" +
                "\tAll records in the data base will be replaced with the data below.\n";

            Instruction.Rows.Add("Instructions", inst);

            return Instruction;
        }

        /// <summary>
        /// Delete all data from the Genus table and replace it with the contents of the XML file
        /// This allows remote editing of metadata
        /// </summary>
        /// <param name="SpeciesMetaData"></param>
        /// <returns></returns>
        public static bool InsertGenusMetaData(DataTable GenusMetaData)
        {
            SQLiteConnection objConn = new SQLiteConnection(Connection_String, true);  // Connect to the data base
            objConn.Open();                                                            // Open the connection

            using (var transaction = objConn.BeginTransaction())
            {

                SQLiteCommand objDeleteCommand = objConn.CreateCommand();
                objDeleteCommand.CommandText = "DELETE FROM Genus";

                // Delete existing data from the Genus table
                try
                {
                    objDeleteCommand.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();   // If we fail
                    MessageBox.Show("Error message\n" + ex.Message, "Apply metta data aborted", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                // Now lets try filling it again
                SQLiteCommand objInsertCommand = objConn.CreateCommand();
                objInsertCommand.CommandText = @"INSERT INTO Genus ( Name)  VALUES ( @Name );";

                foreach (DataRow row in GenusMetaData.Rows)
                {

                    try
                    {
                        objInsertCommand.Parameters.Clear();
                        objInsertCommand.Parameters.AddWithValue("@Name", row.Field<string>("Name"));
                        objInsertCommand.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();   // If we fail
                        MessageBox.Show("Error message\n" + ex.Message, "Apply metta data aborted", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }

                transaction.Commit();
            }

            return true;
        }          

        #endregion  
        // Genus

        #region Shape
        /// <summary>
        /// Create and populate a data table to be saved to XML
        /// The XML to be edited to add/change the shape meta data
        /// </summary>
        /// <param name="bid"></param>
        /// <returns></returns>
        static public DataSet CreateShapeMetaDataDataSet(bool includeData = true)
        {
            DataSet ShapeMetaData = new DataSet("ShapeMetaData");

            ShapeMetaData.Tables.Add(CreateApplicationTable());
            ShapeMetaData.Tables.Add(CreateShapeInstructionTable());
            ShapeMetaData.Tables.Add(CreateShapeTable());

            if ( includeData) LoadShapeTable(ShapeMetaData);

            return ShapeMetaData;
        }



        /// <summary>
        /// Table data contains instructions for editing data
        /// </summary>
        /// <returns></returns>
        static private DataTable CreateShapeInstructionTable()
        {
            DataTable Instruction = new DataTable("Instructions");
            Instruction.Columns.Add("Heading", typeof(string));
            Instruction.Columns.Add("Instruction", typeof(string));

            string inst = "\nINSTRUCTIONS\nRead the manual\n\n\t1. DO NOT change the name of existing shapes. They look like this {<Name>Funnelform</Name>}. EVEN IF IT IS MISSPELT, this is an identifier.\n" +
                "\t1. The name must be unique.\n" +
                "\t2. Image must be the file name of an image ilustrating the shapeyou enter into this file must match the image file name exactly.\n" +
                "\t\t2.1. The value must be file name exactly as it is in the source folder.\n" +
                "\t\t2.2. The Image named above must be in the same folder as this XML file.\n" +
                 "\n\nAll records in the data base will be replaced with the data below.\n";

            Instruction.Rows.Add("Instructions", inst);

            return Instruction;
        }

        /// <summary>
        /// Data data contains Shape data that will when imported replace all existing data
        /// </summary>
        /// <returns></returns>
        static private DataTable CreateShapeTable()
        {
            DataTable Shape = new DataTable("Shape");
            Shape.Columns.Add("Name", typeof(string));
            Shape.Columns.Add("Description", typeof(string));
            Shape.Columns.Add("Image", typeof(string));

            return Shape;
        }

        /// <summary>
        /// Create a populated Shape table
        /// </summary>
        /// <param name="ds"></param>
        static private void LoadShapeTable(DataSet ds)
        {
            SQLiteConnection objConn;
            try
            {
                objConn = new SQLiteConnection(Connection_String, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fatel error \n" + ex.Message, "Can't open database", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }

            string sql = "SELECT * FROM Shape order by Name;";
            SQLiteDataAdapter adapts = new SQLiteDataAdapter(sql, objConn);
            adapts.Fill(ds, "Shape");
            adapts.Dispose();
        }

        /// <summary>
        /// Delete all data from the Shape table and replace it with the contents of the XML file
        /// This allows remote editing of metadata
        /// </summary>
        /// <param name="shapeMetaData"></param>
        /// <returns></returns>
        public static bool InsertShapeMetaData(DataTable shapeMetaData)
        {
            SQLiteConnection objConn = new SQLiteConnection(Connection_String, true);  // Connect to the data base
            objConn.Open();                                                            // Open the connection

            using (var transaction = objConn.BeginTransaction())
            {

                SQLiteCommand objDeleteCommand = objConn.CreateCommand();
                objDeleteCommand.CommandText = "DELETE FROM Shape";

                // Delete existing data from the Shape table
                try
                {
                    objDeleteCommand.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();   // If we fail
                    MessageBox.Show(ex.Message);    //TODO:  Inprove the message 
                    return false;
                }

                // Now lets try filling it again
                SQLiteCommand objInsertCommand = objConn.CreateCommand();
                objInsertCommand.CommandText = @"INSERT INTO Shape (Name, Description, Image) VALUES (@Name, @Description, @Image);";

                foreach (DataRow row in shapeMetaData.Rows)
                {

                    try
                    {
                        objInsertCommand.Parameters.Clear();
                        objInsertCommand.Parameters.AddWithValue("@Name", row.Field<string>("Name"));
                        objInsertCommand.Parameters.AddWithValue("@Description", row.Field<string>("Description"));
                        objInsertCommand.Parameters.AddWithValue("@Image", row.Field<string>("Image"));
                        objInsertCommand.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();   // If we fail
                        MessageBox.Show("Error message\n" + ex.Message, "Apply metta data aborted", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
                transaction.Commit();
            }

            return true;
        }
        #endregion
        // Shape meta data

        #region Inflorescence
        /// <summary>
        /// Create and populate a data table to be saved to XML
        /// The XML to be edited to add/change the shape meta data
        /// </summary>
        /// <param name="bid"></param>
        /// <returns></returns>
        static public DataSet CreateInflorescenceMetaDataDataSet(bool includeData = true)
        {
            DataSet InflorescenceMetaData = new DataSet("InflorescenceMetaData");

            InflorescenceMetaData.Tables.Add(CreateApplicationTable());
            InflorescenceMetaData.Tables.Add(CreateInflorescenceInstructionTable());
            InflorescenceMetaData.Tables.Add(CreateInflorescenceTable());

            if (includeData) LoadInflorescenceTable(InflorescenceMetaData);

            return InflorescenceMetaData;
        }



        /// <summary>
        /// Table data contains instructions for editing data
        /// </summary>
        /// <returns></returns>
        static private DataTable CreateInflorescenceInstructionTable()
        {
            DataTable Instruction = new DataTable("Instructions");
            Instruction.Columns.Add("Heading", typeof(string));
            Instruction.Columns.Add("Instruction", typeof(string));

            string inst = "\nINSTRUCTIONS\nRead the manual\n\n\t1. DO NOT change the name of existing shapes. They look like this {<Name>Funnelform</Name>}. EVEN IF IT IS MISSPELT, this is an identifier.\n" +
                "\t1. The name must be unique.\n" +
                "\t2. Image must be the file name of an image ilustrating the shapeyou enter into this file must match the image file name exactly.\n" +
                "\t\t2.1. The value must be file name exactly as it is in the source folder.\n" +
                "\t\t2.2. The Image named above must be in the same folder as this XML file.\n" +
                 "\n\nAll records in the data base will be replaced with the data below.\n";

            Instruction.Rows.Add("Instructions", inst);

            return Instruction;
        }

        /// <summary>
        /// Data data contains Shape data that will when imported replace all existing data
        /// </summary>
        /// <returns></returns>
        static private DataTable CreateInflorescenceTable()
        {
            DataTable Inflorescence = new DataTable("Inflorescence");
            Inflorescence.Columns.Add("Name", typeof(string));
            Inflorescence.Columns.Add("Description", typeof(string));
            Inflorescence.Columns.Add("Image", typeof(string));

            return Inflorescence;
        }

        /// <summary>
        /// Create a populated Shape table
        /// </summary>
        /// <param name="ds"></param>
        static private void LoadInflorescenceTable(DataSet ds)
        {
            SQLiteConnection objConn;
            try
            {
                objConn = new SQLiteConnection(Connection_String, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fatel error \n" + ex.Message, "Can't open database", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }

            string sql = "SELECT * FROM Inflorescence Order by Name;";
            SQLiteDataAdapter adapts = new SQLiteDataAdapter(sql, objConn);
            adapts.Fill(ds, "Inflorescence");
            adapts.Dispose();
        }

        /// <summary>
        /// Delete all data from the Shape table and replace it with the contents of the XML file
        /// This allows remote editing of metadata
        /// </summary>
        /// <param name="shapeMetaData"></param>
        /// <returns></returns>
        public static bool InsertInflorescenceMetaData(DataTable shapeMetaData)
        {
            SQLiteConnection objConn = new SQLiteConnection(Connection_String, true);  // Connect to the data base
            objConn.Open();                                                            // Open the connection

            using (var transaction = objConn.BeginTransaction())
            {

                SQLiteCommand objDeleteCommand = objConn.CreateCommand();
                objDeleteCommand.CommandText = "DELETE FROM Inflorescence";

                // Delete existing data from the Inflorescence table
                try
                {
                    objDeleteCommand.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();   // If we fail
                    MessageBox.Show(ex.Message);    //TODO:  Inprove the message 
                    return false;
                }

                // Now lets try filling it again
                SQLiteCommand objInsertCommand = objConn.CreateCommand();
                objInsertCommand.CommandText = @"INSERT INTO Inflorescence (Name, Description, Image) VALUES (@Name, @Description, @Image);";

                foreach (DataRow row in shapeMetaData.Rows)
                {

                    try
                    {
                        objInsertCommand.Parameters.Clear();
                        objInsertCommand.Parameters.AddWithValue("@Name", row.Field<string>("Name"));
                        objInsertCommand.Parameters.AddWithValue("@Description", row.Field<string>("Description"));
                        objInsertCommand.Parameters.AddWithValue("@Image", row.Field<string>("Image"));
                        objInsertCommand.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();   // If we fail
                        MessageBox.Show("Error message\n" + ex.Message, "Apply metta data aborted", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
                transaction.Commit();
            }

            return true;
        }

        #endregion
        // Inflorescence


        #region Species
        /// <summary>
        /// Create and populate a data set to be saved to XML
        /// The XML to be edited to add/change the colour meta data
        /// </summary>
        static public DataSet CreateEditSpeciesMetaDataDataSet()
        {
            DataSet SpeciesMetaData = new DataSet("SpeciesMetaData");

            SpeciesMetaData.Tables.Add(CreateApplicationTable());
            SpeciesMetaData.Tables.Add(CreateSpeciesInstructionTable());

            //DataSet metaData = getMetaData();
            DataTable dt = getMetaData().Tables["Species"].Copy();
            SpeciesMetaData.Tables.Add(dt);


            return SpeciesMetaData;
        }

        /// <summary>
        /// Table data contains instructions for editing data
        /// </summary>
        /// <returns></returns>
        static private DataTable CreateSpeciesInstructionTable()
        {
            DataTable Instruction = new DataTable("Instructions");
            Instruction.Columns.Add("Heading", typeof(string));
            Instruction.Columns.Add("Instruction", typeof(string));

            string inst = "\nINSTRUCTIONS\nRead the manual\n\n" +
                "\t1. All columns must have a value.\n" +
                "\t2. Name must be unique and is a Species must be in the botanical taxonomy\n" +
                "\t3. Genus must exactly match an in entry in Genus meta data, it is used to auto populate the Genus and build the scientific name.\n" +
                "\n\tIts recomended you review or amend the Genus data before amending the Species data.\n" +
                "\tAll records in the data base will be replaced with the data below.\n";

            Instruction.Rows.Add("Instructions", inst);

            return Instruction;
        }

        /// <summary>
        /// Delete all data from the Species table and replace it with the contents of the XML file
        /// This allows remote editing of metadata
        /// </summary>
        /// <param name="SpeciesMetaData"></param>
        /// <returns></returns>
        public static bool InsertSpeciesMetaData(DataTable shapeMetaData)
        {
            SQLiteConnection objConn = new SQLiteConnection(Connection_String, true);  // Connect to the data base
            objConn.Open();                                                            // Open the connection

            using (var transaction = objConn.BeginTransaction())
            {

                SQLiteCommand objDeleteCommand = objConn.CreateCommand();
                objDeleteCommand.CommandText = "DELETE FROM Species";

                // Delete existing data from the Species table
                try
                {
                    objDeleteCommand.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();   // If we fail
                    MessageBox.Show("Error message\n" + ex.Message, "Apply metta data aborted", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                // Now lets try filling it again
                SQLiteCommand objInsertCommand = objConn.CreateCommand();
                objInsertCommand.CommandText = @"INSERT INTO Species ( Name,  Genus)  VALUES ( @Name, @Genus );";

                foreach (DataRow row in shapeMetaData.Rows)
                {

                    try
                    {
                        objInsertCommand.Parameters.Clear();
                        objInsertCommand.Parameters.AddWithValue("@Name", row.Field<string>("Name"));
                        objInsertCommand.Parameters.AddWithValue("@Genus", row.Field<string>("Genus"));
                        objInsertCommand.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();   // If we fail
                        MessageBox.Show("Error message\n" + ex.Message, "Apply metta data aborted", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }

                transaction.Commit();
            }

            return true;
        }
        #endregion  // Species

        #region Colour
        /// <summary>
        /// Create and populate a data set to be saved to XML
        /// The XML to be edited to add/change the colour meta data
        /// </summary>
        static public DataSet CreateEditColourMetaDataDataSet()
        {
            DataSet ColourMetaData = new DataSet("ColourMetaData");

            ColourMetaData.Tables.Add(CreateApplicationTable());
            ColourMetaData.Tables.Add(CreateColourInstructionTable());

            //DataSet metaData = getMetaData();
            DataTable dt = getMetaData().Tables["Colour"].Copy();
            ColourMetaData.Tables.Add(dt);

            return ColourMetaData;
        }
        // Colour

        /// <summary>
        /// Table data contains instructions for editing data
        /// </summary>
        /// <returns></returns>
        static private DataTable CreateColourInstructionTable()
        {
            DataTable Instruction = new DataTable("Instructions");
            Instruction.Columns.Add("Heading", typeof(string));
            Instruction.Columns.Add("Instruction", typeof(string));

            string inst = "\nINSTRUCTIONS\nRead the manual\n\n" +
                "\t1. All columns must have a value.\n" +
                "\t2. Sort_Key must be unique.\n" +
                "\t3. UPOV_Colour_ID (International Union for the Protection of New Varieties of Plants). Add none UPOV colours with an ID greater then 1000.\n" +
                "\t4. UPOV_Colour_Name. Use the official name. For none UPOV colours use your name.\n" +
                "\t5. Red_Value. An integer from 0 to 255,representing the Red in the RGB color system.\n" +
                "\t6. Green_Value. An integer from 0 to 255,representing the Green in the RGB color system.\n" +
                "\t7. Blue_Value. An integer from 0 to 255,representing the Blue in the RGB color system.\n" +
                "\n\tAll records in the data base will be replaced with the data below.\n";

            Instruction.Rows.Add("Instructions", inst);

            return Instruction;
        }

        /// <summary>
        /// Delete all data from the Shape table and replace it with the contents of the XML file
        /// This allows remote editing of metadata
        /// </summary>
        /// <param name="shapeMetaData"></param>
        /// <returns></returns>
        public static bool InsertColourMetaData(DataTable ColourMetaData)
        {
            SQLiteConnection objConn = new SQLiteConnection(Connection_String, true);  // Connect to the data base
            objConn.Open();                                                            // Open the connection

            using (var transaction = objConn.BeginTransaction())
            {

                SQLiteCommand objDeleteCommand = objConn.CreateCommand();
                objDeleteCommand.CommandText = "DELETE FROM Colour";

                // Delete existing data from the Genus table
                try
                {
                    objDeleteCommand.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();   // If we fail
                    MessageBox.Show("Error message\n" + ex.Message, "Apply metta data aborted", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                // Now lets try filling it again
                SQLiteCommand objInsertCommand = objConn.CreateCommand();
                objInsertCommand.CommandText = 
                    @"INSERT INTO Colour ( id, upov_id,  name, red, green, blue ) " +
                     "VALUES (@id, @upov_id, @name, @red, @green, @blue );";

                //SELECT id,  upov_id,  name,  red,   green,  blue  FROM Colour;

                foreach (DataRow row in ColourMetaData.Rows)
                {

                    try
                    {
                        objInsertCommand.Parameters.Clear();
                        objInsertCommand.Parameters.AddWithValue("@id", row.Field<int>("id"));
                        objInsertCommand.Parameters.AddWithValue("@upov_id", row.Field<int>("upov_id"));
                        objInsertCommand.Parameters.AddWithValue("@name", row.Field<string>("name"));
                        objInsertCommand.Parameters.AddWithValue("@red", row.Field<int>("red"));
                        objInsertCommand.Parameters.AddWithValue("@green", row.Field<int>("green"));
                        objInsertCommand.Parameters.AddWithValue("@blue", row.Field<int>("blue"));
                        objInsertCommand.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();   // If we fail
                        MessageBox.Show("Error message\n" + ex.Message, "Apply metta data aborted", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }

                transaction.Commit();
            }

            return true;
        }
        #endregion  // InsertColourMetaData

        /// <summary>
        /// Table data identifies:
        /// Application version target, Database version taget, Username, computer name and date created
        /// </summary>
        /// <returns></returns>
        static private DataTable CreateApplicationTable()
        {
            DataTable Appx = new DataTable("Application");
            //Appx.TableName = ;
            Appx.Columns.Add("Application", typeof(string));
            Appx.Columns.Add("Version", typeof(string));
            Appx.Columns.Add("Database_version", typeof(string));
            Appx.Columns.Add("User_name", typeof(string));
            Appx.Columns.Add("Computer_name", typeof(string));
            Appx.Columns.Add("Export_date", typeof(string));

            Appx.Rows.Add(Application.ProductName, Properties.Settings.Default.ApplicationVersion, BloomData.GetDBVersionDisplay(), Environment.UserName,
                        System.Environment.MachineName, DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));

            return Appx;
        }

        #endregion  // Manage meta data
        // ========================================= End manage meta data ===========================================

    }
}
