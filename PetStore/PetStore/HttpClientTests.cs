using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PetStore
{
    [TestClass]
    public class HttpClientTests
    {
        private static HttpClient httpClient;

        private static readonly string BaseURL = "https://petstore.swagger.io/v2/";

        private static readonly string UsersEndpoint = "pet";

        private static string GetURL(string enpoint) => $"{BaseURL}{enpoint}";

        private static Uri GetURI(string endpoint) => new Uri(GetURL(endpoint));

        private readonly List<UserModel> cleanUpList = new List<UserModel>();

        [TestInitialize]
        public void TestInitialize()
        {
            httpClient = new HttpClient();
        }

        [TestCleanup]
        public async Task TestCleanUp()
        {
            foreach (var data in cleanUpList)
            {
                var httpResponse = await httpClient.DeleteAsync(GetURL($"{UsersEndpoint}/{data.Id}"));
            }
        }

        [TestMethod]
        public async Task PutMethod()
        {
            #region create data

            // Create Json Object
            UserModel userData = new UserModel()
            {
                Id = 987,
                Name = "Jappy",
                Status = "available"
            };

            // Serialize Content
            var request = JsonConvert.SerializeObject(userData);
            var postRequest = new StringContent(request, Encoding.UTF8, "application/json");

            // Send Post Request
            await httpClient.PostAsync(GetURL(UsersEndpoint), postRequest);

            #endregion

            #region get Username of the created data

            // Get Request
            var getResponse = await httpClient.GetAsync(GetURI($"{UsersEndpoint}/{userData.Id}"));

            // Deserialize Content
            var listUserData = JsonConvert.DeserializeObject<UserModel>(getResponse.Content.ReadAsStringAsync().Result);

            // filter created data
            var createdUserData = listUserData.Id;

            #endregion

            #region send put request to update data

            // Update value of userData
            userData = new UserModel()
            {
                Id = listUserData.Id,
                Name = "Jappy Spitz",
                Status = "unavailable"
            };

            // Serialize Content
            request = JsonConvert.SerializeObject(userData);
            postRequest = new StringContent(request, Encoding.UTF8, "application/json");

            // Send Put Request
            var httpResponse = await httpClient.PutAsync(GetURL($"{UsersEndpoint}"), postRequest);

            // Get Status Code
            var statusCode = httpResponse.StatusCode;

            #endregion

            #region get updated data

            // Get Request
            getResponse = await httpClient.GetAsync(GetURI($"{UsersEndpoint}/{userData.Id}"));

            // Deserialize Content
            listUserData = JsonConvert.DeserializeObject<UserModel>(getResponse.Content.ReadAsStringAsync().Result);

            // filter created data
            createdUserData = listUserData.Id;
            var updatedName = listUserData.Name;
            var updatedStatus = listUserData.Status;

            #endregion

            #region cleanup data

            // Add data to cleanup list
            cleanUpList.Add(listUserData);

            #endregion

            #region assertion

            // Assertion
            Assert.AreEqual(HttpStatusCode.OK, statusCode, "Status code is not equal to 200");
            Assert.AreEqual(userData.Id, createdUserData, "Id not matching");
            Assert.AreEqual(userData.Name, updatedName, "Name not matching");
            Assert.AreEqual(userData.Status, updatedStatus, "Status not matching");

            #endregion
        }
    }
}