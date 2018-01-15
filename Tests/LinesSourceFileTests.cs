using System;
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using LogParser.LinesSource;

namespace Tests
{
    [TestClass]
    public class LinesSourceTests
    {

        [TestMethod]
        public void PerfectFileTest()
        {
            var fileInfo = new FileInfo("..\\..\\tests_logs\\perfect_file.log.txt");
            ILinesSource linesSource = new LinesSourceFile(fileInfo, 1000000);
            using (var sr = new StreamReader(fileInfo.OpenRead()))
            {
                while (!sr.EndOfStream)
                {
                    var lineExpected = sr.ReadLine();
                    string line;
                    var mustContinue = linesSource.GetLine(out line);
                    Assert.AreEqual(true, mustContinue);
                    Assert.AreEqual(lineExpected, line);
                }
            }
            string emptyLine;
            var mustBeFalse = linesSource.GetLine(out emptyLine);
            Assert.AreEqual(false, mustBeFalse);
            Assert.AreEqual(string.Empty, emptyLine);
        }

        [TestMethod]
        public void EmptyFileTest()
        {
            var fileInfo = new FileInfo("..\\..\\tests_logs\\empty.log.txt");
            ILinesSource linesSource = new LinesSourceFile(fileInfo, 1000000);
            using (var sr = new StreamReader(fileInfo.OpenRead()))
            {
                while (!sr.EndOfStream)
                {
                    var lineExpected = sr.ReadLine();
                    string line;
                    var mustContinue = linesSource.GetLine(out line);
                    Assert.AreEqual(true, mustContinue);
                    Assert.AreEqual(lineExpected, line);
                }
            }
            string emptyLine;
            var mustBeFalse = linesSource.GetLine(out emptyLine);
            Assert.AreEqual(false, mustBeFalse);
            Assert.AreEqual(string.Empty, emptyLine);
        }

        [TestMethod]
        public void TruncatedFileTest()
        {
            var fileInfo = new FileInfo("..\\..\\tests_logs\\truncated_file.log.txt");
            ILinesSource linesSource = new LinesSourceFile(fileInfo, 1000000);
            using (var sr = new StreamReader(fileInfo.OpenRead()))
            {
                while (!sr.EndOfStream)
                {
                    var lineExpected = sr.ReadLine();
                    string line;
                    var mustContinue = linesSource.GetLine(out line);
                    Assert.AreEqual(true, mustContinue);
                    Assert.AreEqual(lineExpected, line);
                }
            }
            string emptyLine;
            var mustBeFalse = linesSource.GetLine(out emptyLine);
            Assert.AreEqual(false, mustBeFalse);
            Assert.AreEqual(string.Empty, emptyLine);
        }

    }
}
