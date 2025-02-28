using Xunit;
using EasySave.Core;
using EasySave.Models;
using System;
using System.IO;
using System.Text.Json;

namespace EasySave.Tests
{
    public class StateManagerTests : IDisposable
    {
        private readonly StateManager _stateManager;

        public StateManagerTests()
        {
            _stateManager = new StateManager();
        }

        [Fact]
        public void SaveState_ValidState_SavesToFile()
        {
            // Arrange
            var state = new JobState
            {
                JobName = "TestJob",
                TotalFiles = 10,
                TotalSize = 1000,
                FilesProcessed = 5,
                SizeProcessed = 500,
                Status = BackupStatus.Running,
                Progress = 50,
                Message = "Processing files...",
                LastUpdate = DateTime.Now
            };

            // Act
            _stateManager.SaveState(state);

            // Assert
            var loadedState = _stateManager.GetState("TestJob");
            Assert.NotNull(loadedState);
            Assert.Equal(state.JobName, loadedState.JobName);
            Assert.Equal(state.TotalFiles, loadedState.TotalFiles);
            Assert.Equal(state.Status, loadedState.Status);
        }

        [Fact]
        public void GetState_ExistingJob_ReturnsState()
        {
            // Arrange
            var state = new JobState
            {
                JobName = "TestJob",
                TotalFiles = 10,
                TotalSize = 1000,
                FilesProcessed = 5,
                SizeProcessed = 500,
                Status = BackupStatus.Running,
                Progress = 50,
                Message = "Processing files...",
                LastUpdate = DateTime.Now
            };
            _stateManager.SaveState(state);

            // Act
            var retrievedState = _stateManager.GetState("TestJob");

            // Assert
            Assert.NotNull(retrievedState);
            Assert.Equal(state.JobName, retrievedState.JobName);
            Assert.Equal(state.TotalFiles, retrievedState.TotalFiles);
            Assert.Equal(state.Status, retrievedState.Status);
        }

        [Fact]
        public void GetState_NonExistingJob_ReturnsNull()
        {
            // Act
            var state = _stateManager.GetState("NonExistingJob");

            // Assert
            Assert.Null(state);
        }

        [Fact]
        public void SaveState_MultipleJobs_SavesAllStates()
        {
            // Arrange
            var state1 = new JobState
            {
                JobName = "Job1",
                Status = BackupStatus.Running,
                Progress = 50
            };

            var state2 = new JobState
            {
                JobName = "Job2",
                Status = BackupStatus.Completed,
                Progress = 100
            };

            // Act
            _stateManager.SaveState(state1);
            _stateManager.SaveState(state2);

            // Assert
            var states = _stateManager.States;
            Assert.Equal(2, states.Count);
            Assert.Equal(BackupStatus.Running, states["Job1"].Status);
            Assert.Equal(BackupStatus.Completed, states["Job2"].Status);
        }

        public void Dispose()
        {
            // Cleanup
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var projectRoot = Directory.GetParent(baseDirectory)?.Parent?.Parent?.Parent?.FullName;
            if (projectRoot != null)
            {
                var stateDirectory = Path.Combine(projectRoot, "Logs", "State");
                if (Directory.Exists(stateDirectory))
                    Directory.Delete(stateDirectory, true);
            }
        }
    }
}
