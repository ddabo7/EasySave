using Xunit;
using EasySave.Logging;
using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

namespace EasySave.Tests
{
    public class JsonLoggerTests
    {
        private readonly string _testLogsDir;
        private readonly JsonLogManager _logger;

        public JsonLoggerTests()
        {
            _testLogsDir = Path.Combine(Path.GetTempPath(), "EasySaveTests", "Logs");
            if (Directory.Exists(_testLogsDir))
                Directory.Delete(_testLogsDir, true);
            Directory.CreateDirectory(_testLogsDir);
            _logger = new JsonLogManager(_testLogsDir);
        }

        [Fact]
        public void LogTransfer_ValidInfo_CreatesLogFile()
        {
            // Arrange
            var info = new TransferInfo
            {
                Name = "TestBackup",
                SourceFile = "source/test.txt",
                TargetFile = "dest/test.txt",
                FileSize = 1000,
                TransferTime = 100,
                BackupType = "FULL",
                Status = "Success"
            };

            // Act
            _logger.LogTransfer(info);

            // Assert
            var logFiles = Directory.GetFiles(_testLogsDir, "*.json");
            Assert.Single(logFiles);
            
            var logContent = File.ReadAllText(logFiles[0]);
            var loggedInfos = JsonSerializer.Deserialize<List<TransferInfo>>(logContent);
            
            Assert.NotNull(loggedInfos);
            Assert.Single(loggedInfos);
            var loggedInfo = loggedInfos[0];
            Assert.Equal(info.Name, loggedInfo.Name);
            Assert.Equal(info.SourceFile, loggedInfo.SourceFile);
            Assert.Equal(info.TargetFile, loggedInfo.TargetFile);
        }

        [Fact]
        public void LogTransfer_MultipleFiles_AppendToLog()
        {
            // Arrange
            var info1 = new TransferInfo
            {
                Name = "TestBackup1",
                SourceFile = "source/test1.txt",
                TargetFile = "dest/test1.txt",
                FileSize = 1000,
                TransferTime = 100,
                BackupType = "FULL"
            };

            var info2 = new TransferInfo
            {
                Name = "TestBackup2",
                SourceFile = "source/test2.txt",
                TargetFile = "dest/test2.txt",
                FileSize = 2000,
                TransferTime = 200,
                BackupType = "DIFFERENTIAL"
            };

            // Act
            _logger.LogTransfer(info1);
            _logger.LogTransfer(info2);

            // Assert
            var logFiles = Directory.GetFiles(_testLogsDir, "*.json");
            Assert.Single(logFiles);

            var logContent = File.ReadAllText(logFiles[0]);
            Assert.Contains(info1.SourceFile, logContent);
            Assert.Contains(info2.SourceFile, logContent);
        }
    }
}
