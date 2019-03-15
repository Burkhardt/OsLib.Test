using System;
using OperatingSystemCore;
using Xunit;
using System.Collections;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OsLibCore;
using System.IO;
//using System.Management;
using HDitem.Persist;
//using HDitem.Server.Configuration;
using System.Diagnostics;
using RaiUtilsCore;
using NyokaServerConfiguration;

namespace OsLibCore.Tests
{
    #region TODO: get all the OsLib tests and insert them here
    #endregion

    public class RaiFileTest
    {
        static internal string TestDir => GeneralTestSettings.Tests.TestDir;
        public RaiFileTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }
        // private TestContext testContextInstance;
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        // public TestContext TestContext
        // {
        // 	get
        // 	{
        // 		return testContextInstance;
        // 	}
        // 	set
        // 	{
        // 		testContextInstance = value;
        // 	}
        // }
        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion
        [Fact]
        public void TestRaiFileEnsure()
        {   // this test does not really challange the delay caused by massive parallel change operations in Dropbox
            var f0 = new RaiFile($"{TestDir}/Data/EnsureTests/kill.txt");
            f0.mkdir();
            var f = new TextFile($"{TestDir}/Data/EnsureTests/kill.txt");
            for (int i = 0; i < 10000; i++)
                f.Append("Test line " + i.ToString());
            f.Save();
            var f2 = new RaiFile($"{TestDir}/Data/EnsureTests/kill2.txt");
            var start = DateTimeOffset.UtcNow;
            var result1 = f2.cp(f);
            Assert.True(File.Exists(f2.FullName), "File is supposed to have materialized by now.");
            var cpDuration = DateTimeOffset.UtcNow - start;
            var f3 = new TmpFile();
            var result2 = f3.mv(f2);
            Assert.True(File.Exists(f3.FullName), "File is supposed to have materialized by now.");
            Assert.False(File.Exists(f2.FullName), "File is supposed to have vanished by now.");
            Assert.Equal(0, result1 + result2);
        }
        [Fact]
        public void TestZip()
        {
            var temp = new RaiFile($"{TestDir}/Data/pic3/d/iserv/testpic/325/325859/325859_00.tiff");
            var original = new RaiFile($"{TestDir}/Data/pic3/d/iserv/testpic/AB5/AB5678/AB5678_03.tiff");
            Assert.True(File.Exists(original.FullName));
            temp.cp(original);
            var zipFile = temp.Zip();
            var t1 = DateTime.Now;
            Assert.True(File.Exists(zipFile.FullName));
            var zipSize = new FileInfo(zipFile.FullName).Length;
            zipFile.rm();
            temp.cp(original);
            var t2 = DateTime.Now;
            // var zip7 = temp.Zip7();
            // var t3 = DateTime.Now;
            // Assert.True(zip7.Exists());
            // var zip7Size = new FileInfo(zip7.FullName).Length;
            // zip7.rm();
            temp.cp(original);
            var t4 = DateTime.Now;
            // var ultra = temp.ZipUltra();
            // var t5 = DateTime.Now;
            // Assert.True(ultra.Exists());
            // var ultraSize = new FileInfo(ultra.FullName).Length;
            // Debug.WriteLine("zip:/t/t" + (t1 - t0) + "/t" + zipSize);
            // Debug.WriteLine("7zip:/t" + (t3 - t2) + "/t" + zip7Size);
            // Debug.WriteLine("ultra:/t" + (t5 - t4) + "/t" + ultraSize);
        }
        [Fact]
        public void TestRaiFile1()
        {
            var temp = new RaiFile($"{TestDir}/Data/pic3/d/iserv/testpic/325/325859/325859_11.tiff");
            var original = new RaiFile($"{TestDir}/Data/pic3/d/iserv/testpic/325/325859/325859_03.tiff");
            Assert.True(File.Exists(original.FullName));
            temp.rm();
            Assert.False(File.Exists(temp.FullName));
            temp.cp(original);
            Assert.True(File.Exists(temp.FullName));
            var dest = new RaiFile($"{TestDir}/Data/pic3/d/iserv/testpic/325/325859/325859_01.tiff");
            dest.rm();
            dest.mv(temp);
            Assert.True(File.Exists(dest.FullName));
            Assert.False(File.Exists(temp.FullName));
        }
        [Fact]
        public void TestTextFile()
        {
            var f = new TextFile($"{TestDir}/Data/temp/kill/308024_11.text");
            f.mkdir();  // <= doesn't seem to work
            f.Append("1: first line");
            f.Save();
            f.Insert(0, "2: second line");
            f.Insert(1, "3: third line");
            f.Save();
            f.Sort();
            f.Save(true);
        }
        [Fact]
        public void TestCsvFile()
        {
            var csv = new CsvFile($"{TestDir}/Data/CONLEYde.csv");
            int count = csv.Read();
            var item = csv[1000]; //was: var item = csv.Item(1000);
            string MerchantProductNumber = item[nameof(MerchantProductNumber)];
            string ProductManufacturerBrand = item[nameof(ProductManufacturerBrand)];
            string ProductName = item[nameof(ProductName)];
            var FieldNames = csv.FieldNames();
        }
        /// <summary>
        /// Replacement Concept Iserv/Subscriber 
        /// </summary>
        /// <remarks>
        /// Iserv.xml defined variables for every server like iserv["Pro1997"].FastRootDir
        /// Subscribers.xml (iserv.Subscribers) uses this variables to define SubscriberSettings
        /// that are generalized enough to work for all servers.
        /// subscriberFile["pic"].CacheDir: "{FastRootDir}Cache/pic/"
        /// subscriberFile["ipic"].CacheDir: "{FastRootDir}Cache/ipic/"
        /// Only if one server is so special that it's settings cannot be described using the variables, 
        /// a special subscriber setting needs to be defined in the subscriberFile:
        /// subscriberFile["Pro1997.pic"], subscriberFile["Pro1997.ipic"], ...
        /// Use HDitem.Persist's XmlFile<T> if no variables allowed or HDitem.Persist's MemSettingsFile<T> if variables can be used;
        /// if so, use the parameter variables to pass-in Variables and values; use the Variables constructor to create a set of variables and
        /// values from an XmlSetting; example: var Pro1997Vars = new Variables(iserv["PRO1997"]);
        /// </remarks>
        [Fact]
        public void TestVariableReplacement()
        {
            // #region make sure we are not affecting any live settings file
            // var oldServerSettingDefault = XmlFile<ServerSetting>.ConfigDirDefault;
            // var oldSubscriberSettingDefault = JsonFile<SubscriberItem>.ConfigDirDefault;
            // XmlFile<ServerSetting>.ConfigDirDefault = Os.DropboxRoot + @"TestData/U17138031";
            // JsonFile<SubscriberItem>.ConfigDirDefault = Os.DropboxRoot + @"TestData/U17138031";
            // #endregion
            // var iSet = new JsonFile<ServerItem>()["U17138031"];
            // var dr = Os.DropboxRoot;
            // var fileName = dr + "TestData/pic3/c/Config/3.3.3/" + Environment.MachineName + "/Subscribers.xml";
            // var vars = new Variables(iSet);
            // var xsf = new MemItemsFile<SubscriberItem>(fileName, variables: vars);
            // Assert.Equal(xsf.InfosResolved["ipict"].CacheDir.ToLower(), "c:/cache/ipict/", "error: check test method and " + fileName);
            // Assert.Equal(xsf.InfosResolved["ipict"].DownloadUrl.ToLower(), "http://pic8.hditem.com/download/ipict/", "error: check test method and " + fileName);
            // Assert.NotNull(xsf.InfosResolved["ipict"].BackupDir.ToLower(), "error: check test method and " + fileName);
            // Assert.False(xsf.InfosResolved["ipict"].BackupDir.Contains("{"), "all variables should be resolved");
            // #region switch back to old ConfigDirDefault
            // XmlFile<ServerSetting>.ConfigDirDefault = oldServerSettingDefault;
            // JsonFile<SubscriberItem>.ConfigDirDefault = oldSubscriberSettingDefault;
            // #endregion
        }
        [Fact]
        public void TestOsType()
        {
            Assert.True(OsType.UNIX == Os.Type || OsType.Windows == Os.Type);
        }
        [Fact]
        public void TestSearchExpression2()
        {
            // var se = new SearchExpression("Email=Rainer*");
            // var user1 = new UserSetting()
            // {
            //     Email = "Rainer@Burkhardt.com"
            // };
            // var user2 = new UserSetting()
            // {
            //     Email = "Rainer77@HDitem.com"
            // };
            // var user3 = new UserSetting()
            // {
            //     Email = "MeinRainer77@HDitem.com"
            // };
            // var b1 = se.IsMatch(user1);
            // var b2 = se.IsMatch(user2);
            // var b3 = se.IsMatch(user3);
            // Assert.True(b1 && b2 && !b3);
            // se = new SearchExpression("HDitem+ein");
            // b1 = se.IsMatch(user1);
            // b2 = se.IsMatch(user2);
            // b3 = se.IsMatch(user3);
            // Assert.True(!b1 && !b2 && b3);
        }
        // [Fact]
        // public void TestFindFile()
        // {
        //     var rootDirectory = @"D:/iserv/pic/";
        //     var filePattern = "*";
        //     var itd = new ImageTreeDirectory(rootDirectory, filePattern, new string[] { "tiff", "png" });
        //     var t0 = DateTimeOffset.UtcNow;
        //     var ok = itd.FindImages();
        //     Assert.True(ok);
        //     var skuDict = itd.SkuDict;
        //     var t1 = DateTimeOffset.UtcNow;
        //     Debug.WriteLine("Create SkuDict using FindImages: found {0:n0} skus with all their images in {1:n2} seconds.", skuDict.Count(), (t1 - t0).TotalSeconds);
        //     var t2 = DateTimeOffset.UtcNow;
        //     var ImageList = itd.GetImageList().OrderBy(x => x.Name).ToList();
        //     var t3 = DateTimeOffset.UtcNow;
        //     Debug.WriteLine("Enumerated {0:n0} skus with {1:n0} images in {2:n3} seconds.", skuDict.Count(), ImageList.Count(), (t3 - t2).TotalSeconds);
        // }
        // [Fact]
        // public void TestFindFile2()
        // {
        // 	var rootDirectory = @"D:/iserv/pic/";
        // 	#region *
        // 	string filePattern = "*";
        // 	var itd = new ImageTreeDirectory(rootDirectory, filePattern, new string[] { "tiff", "png" });
        // 	var t0 = DateTimeOffset.UtcNow;
        // 	bool ok = itd.FindImages();
        // 	Assert.True(ok);
        // 	var skuDict = itd.SkuDict;
        // 	#endregion
        // 	#region *1
        // 	filePattern = "*1";
        // 	itd = new ImageTreeDirectory(rootDirectory, filePattern, new string[] { "tiff", "png" });
        // 	var t1 = DateTimeOffset.UtcNow;
        // 	ok = itd.FindImages();
        // 	Assert.True(ok);
        // 	skuDict = itd.SkuDict;
        // 	#endregion
        // 	#region *12
        // 	filePattern = "*12";
        // 	itd = new ImageTreeDirectory(rootDirectory, filePattern, new string[] { "tiff", "png" });
        // 	var t2 = DateTimeOffset.UtcNow;
        // 	ok = itd.FindImages();
        // 	Assert.True(ok);
        // 	skuDict = itd.SkuDict;
        // 	#endregion
        // 	#region *123
        // 	filePattern = "*123";
        // 	itd = new ImageTreeDirectory(rootDirectory, filePattern, new string[] { "tiff", "png" });
        // 	var t3 = DateTimeOffset.UtcNow;
        // 	ok = itd.FindImages();
        // 	Assert.True(ok);
        // 	skuDict = itd.SkuDict;
        // 	#endregion
        // 	#region *1234
        // 	filePattern = "*1234";
        // 	itd = new ImageTreeDirectory(rootDirectory, filePattern, new string[] { "tiff", "png" });
        // 	var t4 = DateTimeOffset.UtcNow;
        // 	ok = itd.FindImages();
        // 	Assert.True(ok);
        // 	skuDict = itd.SkuDict;
        // 	#endregion
        // 	#region *12345
        // 	filePattern = "*12345";
        // 	itd = new ImageTreeDirectory(rootDirectory, filePattern, new string[] { "tiff", "png" });
        // 	var t5 = DateTimeOffset.UtcNow;
        // 	ok = itd.FindImages();
        // 	Assert.True(ok);
        // 	skuDict = itd.SkuDict;
        // 	#endregion
        // 	#region 30145*
        // 	filePattern = "30145*";
        // 	itd = new ImageTreeDirectory(rootDirectory, filePattern, new string[] { "tiff", "png" });
        // 	var t6 = DateTimeOffset.UtcNow;
        // 	ok = itd.FindImages();
        // 	Assert.True(ok);
        // 	skuDict = itd.SkuDict;
        // 	#endregion
        // 	#region 32*45
        // 	filePattern = "32*45";
        // 	itd = new ImageTreeDirectory(rootDirectory, filePattern, new string[] { "tiff", "png" });
        // 	var t7 = DateTimeOffset.UtcNow;
        // 	ok = itd.FindImages();
        // 	Assert.True(ok);
        // 	skuDict = itd.SkuDict;
        // 	#endregion
        // 	#region 32*5
        // 	filePattern = "32*5";
        // 	itd = new ImageTreeDirectory(rootDirectory, filePattern, new string[] { "tiff", "png" });
        // 	var t8 = DateTimeOffset.UtcNow;
        // 	ok = itd.FindImages();
        // 	Assert.True(ok);
        // 	skuDict = itd.SkuDict;
        // 	#endregion
        // 	#region 2*
        // 	filePattern = "2*";
        // 	itd = new ImageTreeDirectory(rootDirectory, filePattern, new string[] { "tiff", "png" });
        // 	var t9 = DateTimeOffset.UtcNow;
        // 	ok = itd.FindImages();
        // 	Assert.True(ok);
        // 	skuDict = itd.SkuDict;
        // 	#endregion
        // 	#region 32*
        // 	filePattern = "32*";
        // 	itd = new ImageTreeDirectory(rootDirectory, filePattern, new string[] { "tiff", "png" });
        // 	var t10 = DateTimeOffset.UtcNow;
        // 	ok = itd.FindImages();
        // 	Assert.True(ok);
        // 	skuDict = itd.SkuDict;
        // 	#endregion
        // 	var tEnd = DateTimeOffset.UtcNow;
        // 	Debug.WriteLine("dt0: {0} - dt1: {1} - dt2: {2} - dt3: {3} - dt4: {4} - dt5: {5} - dt6: {6} - dt7: {7} - dt8: {8} - dt9: {9}/ntotal: {10}", t1 - t0, t2 - t1, t3 - t2, t4 - t3, t5 - t4, t6 - t5, t7 - t6, t8 - t7, t9 - t8, t10-t9, tEnd - t0);
        // }
    }

    public class TestOsLibCore
    {
        static internal string TestDir => GeneralTestSettings.Tests.TestDir;
        // [Fact]
        // public void WhereIsTheCurrentDropbox()
        // {
        //     if (Os.Type == OsType.Windows)
        //     {
        //         var dbPath = System.IO.Path.Combine(
        //             Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dropbox//host.db");
        //         string[] lines = System.IO.File.ReadAllLines(dbPath);
        //         byte[] dbBase64Text = Convert.FromBase64String(lines[1]);
        //         string folderPath = System.Text.ASCIIEncoding.ASCII.GetString(dbBase64Text);
        //         Assert.False(string.IsNullOrEmpty(folderPath));
        //         var iSet = new JsonFile<ServerItem>(Os.DropboxRoot + @"TestData/pic3/c/Config/Iserv.xml")[Environment.MachineName];
        //         Assert.Equal(Os.NormSeperator(Os.DropboxRoot), Os.NormSeperator(iSet.SyncRootDir).ToLower()); //, "DropboxRootDir not found"
        //         Assert.False(string.IsNullOrEmpty(Os.DropboxRoot), "DropboxRootDir is not supposed to be empty - is Dropbox installed?");
        //         #region get it from the running process (dropbox should obviously be running)
        //         var processes = Process.GetProcessesByName("Dropbox");
        //         var processId = processes[0].Id;
        //         using (ManagementObjectSearcher mos = new ManagementObjectSearcher("SELECT * FROM Win32_Process WHERE Name = 'Dropbox.exe'"))
        //         {
        //             foreach (var mo in mos.Get())
        //             {
        //                 string x = (string)mo["CommandLine"];
        //                 if (x != null && x.Contains("Dropbox"))
        //                     Debug.WriteLine("Name={0}/nCommandLine={1}/nDescription={2}", mo["Name"], mo["CommandLine"], mo["Description"]);
        //             }
        //         }
        //         #endregion
        //     }
        //     #region get Os.DropboxRootDir
        //     string root = Os.DropboxRoot;
        //     Assert.False(string.IsNullOrWhiteSpace(root), "Dropbox not installed or not running on this machine");
        //     #endregion
        // }
        [Fact]
        public void TestDropboxRoot()
        {
            try
            {
                var dropboxRoot = Os.DropboxRoot;
            }
            catch (Exception)
            {
                Assert.True(false, "Os.DropboxRoot not working - file permission?");
            }
        }
        [Fact]
        public void TestJsonFileNyokaRemote()
        {

        }
    }
    public class NyokaTest
    {
        static internal string TestDir => GeneralTestSettings.Tests.TestDir;
        public NyokaTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }
        [Fact]
        public void TestNyokaRemote()
        {
            const string nyokaremoteFileName = ".nyokaremotetest.json";
            var dir = Directory.GetCurrentDirectory();
            var nrFileNameWithPath = $"{dir}{Os.DIRSEPERATOR}{nyokaremoteFileName}";
            var info = new NyokaRemoteInfo
            {
                RepositoryServer = null,
                ZementisServer = null,
                ZementisModeler = "http://localhost:7007"
            };
            #region write nyokaremote file in current directory
            var nrFile = new JsonFile<NyokaRemoteInfo>(nyokaremoteFileName, readOnly: false);
            nrFile["default"] = info;
            nrFile.Save(force: true);
            #endregion
            Assert.True(File.Exists(nrFileNameWithPath));
            #region read nyokaremote file in current directory
            var nrFile0 = new JsonFile<NyokaRemoteInfo>(nyokaremoteFileName, readOnly: false);
            var info0 = nrFile0["default"];
            #endregion
            Assert.Equal(info.ZementisModeler, info0.ZementisModeler);
            #region set new values and save it back to the file
            info0.ZementisServer = "https://zserver.zmod.org";
            nrFile0["default"] = info0;
            nrFile0.Save(force: true);   // should not be neccessary to force
            #endregion
            Assert.True(File.Exists(nrFileNameWithPath));
            #region readonly test for nyokaremote file
            var nrFileReadOnly = new JsonFile<NyokaRemoteInfo>(nyokaremoteFileName);    // readonly is default
            var info2 = nrFileReadOnly["default"];
            Assert.Equal(info.ZementisModeler, info2.ZementisModeler);
            info2.RepositoryServer = "https://dlexp.zmod.org";
            nrFileReadOnly["default"] = info2;
            try {
                nrFileReadOnly.Save();
                Assert.True(false, "IOException was supposed to get thrown");
            }
            catch (IOException ex)
            {
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.True(false, $"wrong exception type {ex.GetType()} was thrown: {ex.Message}");
            }
            #endregion
            #region clean up test
            //nrFile.rm()   // not implemented
            File.Delete(nrFileNameWithPath);
            #endregion
            Assert.False(File.Exists(nrFileNameWithPath));
        }
        [Fact]
        public void TestCreateTextFileHome()
        {
            // this test does not really challange the delay caused by massive parallel change operations in Dropbox
            var tf = new TextFile("~/TestTextFileHome.txt");
            tf.Append("Hallo");
            tf.Append("World");
            tf[6] = "Seventh line";   // should not throw an exception but extend the size of the collection
            tf[999] = "Line 1000";
            tf.Save(backup: true);
            Assert.True(File.Exists(tf.FullName));
            tf.rm();
            Assert.False(File.Exists(tf.FullName), "File is supposed to have vanished by now - not in dropbox => no delay.");
        }
        [Fact]
        public void TestCreateTextFileCurrent()
        {
            var tf = new TextFile("./TestCreateTextFileCurrent.txt");
            tf[6] = "Seventh line";   // should not throw an exception but extend the size of the collection
            tf[999] = "Line 1000";
            tf.Save(backup: false);
            Assert.True(File.Exists(tf.FullName));
            tf.rm();
            Assert.False(File.Exists(tf.FullName), "File is supposed to have vanished by now - not in dropbox => no delay.");
        }
        [Fact]
        public void TestCreateTextFileParent()
        {
            var tf = new TextFile("../TestCreateTextFileParent.txt");
            tf[4] = "Line 5";   // should not throw an exception but extend the size of the collection
            tf[799] = "Line 800";
            tf.Save(backup: false);
            Assert.True(File.Exists(tf.FullName));
            tf.rm();
            Assert.False(File.Exists(tf.FullName), "File is supposed to have vanished by now - not in dropbox => no delay.");
        }
    }
}
