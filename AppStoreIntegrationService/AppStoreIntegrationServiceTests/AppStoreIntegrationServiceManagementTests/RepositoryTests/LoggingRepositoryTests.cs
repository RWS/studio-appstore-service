using AppStoreIntegrationServiceCore.Model;
using Xunit;
using AppStoreIntegrationServiceManagement.Repository;
using AppStoreIntegrationServiceTests.AppStoreIntegrationServiceManagementTests.Mock;

namespace AppStoreIntegrationServiceTests.AppStoreIntegrationServiceManagementTests.RepositoryTests
{
    public class LoggingRepositoryTests
    {
        [Fact]
        public async void LoggingRepositoryTest_GetAllPluginLogsFromNullOrEmptyStorage_ShouldReturnEmptyList()
        {
            var azurerepository = new AzureRepositoryMock(data: null);
            var logsRepository = new LoggingRepository(azurerepository);

            Assert.Empty(await logsRepository.GetPluginLogs(0));
        }

        [Fact]
        public async void LoggingRepositoryTest_GetAllPluginLogsFromStorage_ShouldReturnCorrespondingLogs()
        {
            var azurerepository = new AzureRepositoryMock(LoadPluginLogs());
            var logsRepository = new LoggingRepository(azurerepository);

            Assert.Equal(new List<Log>
            {
                new Log { Author = "Test author 1", Description = "Test log 1", Date = new DateTime(2015, 5, 15) },
                new Log { Author = "Test author 2", Description = "Test log 2", Date = new DateTime(2019, 9, 19) },
                new Log { Author = "Test author 3", Description = "Test log 3", Date = new DateTime(2005, 5, 5)  }
            }, await logsRepository.GetPluginLogs(0));
        }

        [Fact]
        public async void LoggingRepositoryTest_AddLogToStorage_TheLogShouldBeAdded()
        {
            var azurerepository = new AzureRepositoryMock(LoadPluginLogs());
            var logsRepository = new LoggingRepository(azurerepository);
            var currentDate = DateTime.Now;

            await logsRepository.Log(new Log { Author = "Test author 4", Description = "Test log 4", Date = currentDate, }, 0);

            Assert.Equal(new List<Log>
            {
                new Log { Author = "Test author 1", Description = "Test log 1", Date = new DateTime(2015, 5, 15) },
                new Log { Author = "Test author 2", Description = "Test log 2", Date = new DateTime(2019, 9, 19) },
                new Log { Author = "Test author 3", Description = "Test log 3", Date = new DateTime(2005, 5, 5)  },
                new Log { Author = "Test author 4", Description = "Test log 4", Date = currentDate },
            }, await logsRepository.GetPluginLogs(0));
        }

        [Fact]
        public async void LoggingRepositoryTest_AddNullLogToStorage_TheLogShouldNotBeAdded()
        {
            var azurerepository = new AzureRepositoryMock(LoadPluginLogs());
            var logsRepository = new LoggingRepository(azurerepository);
            await logsRepository.Log(null, 0);

            Assert.Equal(new List<Log>
            {
                new Log { Author = "Test author 1", Description = "Test log 1", Date = new DateTime(2015, 5, 15) },
                new Log { Author = "Test author 2", Description = "Test log 2", Date = new DateTime(2019, 9, 19) },
                new Log { Author = "Test author 3", Description = "Test log 3", Date = new DateTime(2005, 5, 5)  }
            }, await logsRepository.GetPluginLogs(0));
        }

        [Fact]
        public async void LoggingRepositoryTest_FilterLogsByDate_ShouldReturnMatchingLogs()
        {
            var azurerepository = new AzureRepositoryMock(LoadPluginLogs());
            var logsRepository = new LoggingRepository(azurerepository);
            var logs = await logsRepository.GetPluginLogs(0);

            Assert.Equal(new List<Log>
            {
                new Log { Author = "Test author 2", Description = "Test log 2", Date = new DateTime(2019, 9, 19) }
            }, logsRepository.SearchLogs(logs, new DateTime(2018, 1, 1), new DateTime(2020, 1, 1)));
        }

        [Fact]
        public async void LoggingRepositoryTest_FilterLogsByQuery_ShouldReturnMatchingLogs()
        {
            var azurerepository = new AzureRepositoryMock(LoadPluginLogs());
            var logsRepository = new LoggingRepository(azurerepository);
            var logs = await logsRepository.GetPluginLogs(0);

            Assert.Equal(new List<Log>
            {
                new Log { Author = "Test author 3", Description = "Test log 3", Date = new DateTime(2005, 5, 5) }
            }, logsRepository.SearchLogs(logs, DateTime.MinValue, DateTime.MaxValue, "log 3"));
        }

        private static PluginResponse<PluginDetails> LoadPluginLogs()
        {
            return new PluginResponse<PluginDetails>
            {
                Logs = new Dictionary<int, IEnumerable<Log>>
                {
                    [0] = new List<Log>
                    {
                        new Log { Author = "Test author 1", Description = "Test log 1", Date = new DateTime(2015, 5, 15) },
                        new Log { Author = "Test author 2", Description = "Test log 2", Date = new DateTime(2019, 9, 19) },
                        new Log { Author = "Test author 3", Description = "Test log 3", Date = new DateTime(2005, 5, 5) }
                    }
                }
            };
        }
    }
}
