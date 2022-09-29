using Microsoft.VisualStudio.TestTools.UnitTesting;
using RestSharp;
using System.Net;
using Xunit.Sdk;

namespace AppStoreIntegrationLoadTests
{
	[TestClass]
	public class PluginsTests
	{
        [TestMethod]
        public void GetPlugins()
        {
            var client = new RestClient("https://studio-appstore-api.azurewebsites.net/");
            var request = new RestRequest("/plugins", Method.Get);
            request.AddHeader("Content-Type", "application/json");

            var response = client.Execute(request);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
