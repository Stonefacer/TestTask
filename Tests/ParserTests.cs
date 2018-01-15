using System;
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Models;
using LogParser;
using LogParser.LinesSource;

namespace Tests
{
    [TestClass]
    public class ParserTests
    {

        private void CheckDataEquality(RequestData requestDataExpected, RequestData requestDataActual)
        {
            Assert.AreEqual(requestDataExpected.ClientHostname, requestDataActual.ClientHostname);
            Assert.AreEqual(requestDataExpected.DataSize, requestDataActual.DataSize);
            Assert.AreEqual(requestDataExpected.Datetime, requestDataActual.Datetime);
            Assert.AreEqual(requestDataExpected.QueryString, requestDataActual.QueryString);
            Assert.AreEqual(requestDataExpected.StatusCode, requestDataActual.StatusCode);
            Assert.AreEqual(requestDataExpected.Route, requestDataActual.Route);
        }

        /// <summary>
        /// Compare parsed from file data with given expected data and return parser for further tests
        /// </summary>
        private void CheckFile(string filename, RequestData[] dataExpected)
        {
            var fileInfo = new FileInfo(filename);
            ILinesSource linesSource = LinesSourceMemory.CreateFromFile(fileInfo);
            IParser parser = new Parser(linesSource, new string[] { "css", "map", "jpg", "jpeg", "png", "gif", "bmp", "tiff", "js", "xbm" });
            RequestData requestData;
            for (var i = 0; i < dataExpected.Length; i++)
            {
                bool keepAlive;
                var save = parser.ProcessOneLine(out requestData, out keepAlive);
                Assert.AreEqual(true, keepAlive);
                // check objects equality
                CheckDataEquality(dataExpected[i], requestData);
            }
            // check result when end of data reached
            bool mustBeFalse;
            var mustBeFalseToo = parser.ProcessOneLine(out requestData, out mustBeFalse);
            Assert.AreEqual(false, mustBeFalse);
            Assert.AreEqual(false, mustBeFalseToo);
            Assert.AreEqual(requestData, null);
        }

        [TestMethod]
        public void PerfectFileTest()
        {
            // data expected in file
            var dataExpected = new RequestData[]
            {
                new RequestData()
                {
                    ClientHostname = "199.72.81.55",
                    Datetime = new DateTime(1995, 7, 1, 4, 0, 1, DateTimeKind.Utc),
                    DataSize = 6245,
                    StatusCode = 200,
                    Route = "/history/apollo/",
                    QueryString = ""
                },
                new RequestData()
                {
                    ClientHostname = "64.242.88.10",
                    Datetime = new DateTime(2004, 3, 8, 00, 6, 51, DateTimeKind.Utc),
                    DataSize = 4523,
                    StatusCode = 200,
                    Route = "/twiki/bin/rdiff/TWiki/NewUserTemplate",
                    QueryString = "rev1=1.3&rev2=1.2"
                },
                new RequestData()
                {
                    ClientHostname = "129.94.144.152",
                    Datetime = new DateTime(1995, 7, 1, 4, 0, 13, DateTimeKind.Utc),
                    DataSize = 7074,
                    StatusCode = 200,
                    Route = "/",
                    QueryString = ""
                }
            };
            CheckFile("..\\..\\tests_logs\\perfect_file.log", dataExpected);
        }

        [TestMethod]
        public void EmptyFileTest()
        {
            var dataExpected = new RequestData[0];
            CheckFile("..\\..\\tests_logs\\empty.log", dataExpected);
        }

        [TestMethod]
        public void TruncatedFileTest()
        {
            var dataExpected = new RequestData[]
            {
                new RequestData()
                {
                    ClientHostname = "64.242.88.10",
                    DataSize = 46373,
                    Datetime = new DateTime(2004, 3, 8, 00, 36, 22, DateTimeKind.Utc),
                    QueryString = "rev1=1.2&rev2=1.1",
                    StatusCode = 200,
                    Route = "/twiki/bin/rdiff/Main/WebIndex"
                },
                new RequestData()
                {
                    ClientHostname = "dyn1-039.cc.umanitoba.ca",
                    DataSize = 7124,
                    Datetime = new DateTime(1995, 7, 1, 5, 42, 28, DateTimeKind.Utc),
                    QueryString = "SAREX-II",
                    StatusCode = 200,
                    Route = "/htbin/wais.pl"
                },
                new RequestData()
                {
                    ClientHostname = "lj1125.inktomisearch.com",
                    DataSize = 209,
                    Datetime = new DateTime(2004, 3, 8, 02, 6, 14, DateTimeKind.Utc),
                    QueryString = string.Empty,
                    StatusCode = 200,
                    Route = "/twiki/bin/oops/Sandbox/WebChanges"
                },
                new RequestData()
                {
                    ClientHostname = "lj1125.inktomisearch.com",
                    DataSize = 209,
                    Datetime = new DateTime(2004, 3, 8, 02, 6, 14, DateTimeKind.Utc),
                    QueryString = string.Empty,
                    StatusCode = 200,
                    Route = "/twiki/bin/oops/Sandbox/WebChanges "
                },
                new RequestData()
                {
                    ClientHostname = "knuth.mtsu.edu",
                    DataSize = -1,
                    Datetime = new DateTime(1995, 7, 22, 5, 49, 32, DateTimeKind.Utc),
                    QueryString = string.Empty,
                    StatusCode = 404,
                    Route = "/images/\">index of /images"
                }
            };
            CheckFile("..\\..\\tests_logs\\truncated_file.log", dataExpected);
        }

    }
}
