using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Net.Http.Formatting;

namespace Tascomi.Tests
{
    [TestClass]
    public class Users
    {
        private const string ApiUri = "/rest/v1/users";

        [TestMethod]
        public async Task Create()
        {
            using (var hc = APIClient.Instance.GetHttpClient())
            {
                var userData = new {
                    firstname = "Test " + DateTime.Now.ToString("r"),
                    username = "Test" + DateTime.Now.Second.ToString() };

                var content = new FormUrlEncodedContent(userData.ToDictionary<string>().ToList());

                var response = await hc.PutAsync(ApiUri, content);

                Assert.IsFalse(response.IsSuccessStatusCode);
            }
        }

        [TestMethod]
        public async Task ReadOne()
        {
            using (var hc = APIClient.Instance.GetHttpClient())
            {
                // Find a user
                var getUsers = await hc.GetAsync(ApiUri);
                getUsers.EnsureSuccessStatusCode();
                var users = await getUsers.Content.ReadAsAsync<List<dynamic>>();

                var userId = (int)users.Where(u => (int)u.id > 100).First().id;

                var response = await hc.GetAsync(ApiUri + "/" + userId);
                Assert.IsTrue(response.IsSuccessStatusCode, "Request wasn't successful");
                var user = await response.Content.ReadAsAsync<dynamic>();
                Assert.AreEqual<int>(userId, (int)user.id, "IDs are not equal");

                Console.WriteLine(userId);
            }
        }

        [TestMethod]
        public async Task ReadMany()
        {
            using (var hc = APIClient.Instance.GetHttpClient())
            {
                var response = await hc.GetAsync(ApiUri);
                Assert.IsTrue(response.IsSuccessStatusCode, "Request wasn't successful");
                var users = await response.Content.ReadAsAsync<List<dynamic>>();
                Assert.IsTrue(users.Count() > 0);
            }
        }

        [TestMethod]
        public async Task ReadManyWithFilter()
        {
            using (var hc = APIClient.Instance.GetHttpClient())
            {
                var response = await hc.GetAsync(ApiUri + "?firstname=Alex");
                Assert.IsTrue(response.IsSuccessStatusCode, "Request wasn't successful");
                var users = await response.Content.ReadAsAsync<List<dynamic>>();
                Assert.IsTrue(users.Count() > 0);
                Assert.IsTrue(users.Where(c => c.firstname == "Alex").Count() > 0);
            }
        }

        [TestMethod]
        public async Task ReadManySorted()
        {
            using (var hc = APIClient.Instance.GetHttpClient())
            {
                var response = await hc.GetAsync(ApiUri + "?sort=-surname");
                Assert.IsTrue(response.IsSuccessStatusCode, "Request wasn't successful");
                var users = await response.Content.ReadAsAsync<List<dynamic>>();
                Assert.IsTrue(users.Count() > 0);
                var expectedUsers = users.OrderByDescending(c => c.surname.ToString().ToLower()).ToList();
                Assert.IsTrue(expectedUsers.SequenceEqual(users, new PropertyComparer<dynamic>("surname")), "Lists are not ordered the same");
            }
        }

        [TestMethod]
        public async Task Update()
        {
            using (var hc = APIClient.Instance.GetHttpClient())
            {
                // Find a user to update
                var getUsers = await hc.GetAsync(ApiUri);
                getUsers.EnsureSuccessStatusCode();
                var users = await getUsers.Content.ReadAsAsync<List<dynamic>>();

                var userId = (int)users.Where(u => (int)u.id > 100).First().id;

                var userData = new
                {
                    firstname = "Test " + DateTime.Now.ToString("r")
                };

                var content = new FormUrlEncodedContent(userData.ToDictionary<string>().ToList());

                var updateContact = await hc.PostAsync(ApiUri + "/" + userId, content);
                Assert.IsFalse(updateContact.IsSuccessStatusCode);
                var updatedContact = await updateContact.Content.ReadAsAsync<dynamic>();
                Assert.AreEqual<int>(userId, (int)updatedContact.id, "IDs are not the same");

                var getContact = await hc.GetAsync(ApiUri + "/" + userId);
                getContact.EnsureSuccessStatusCode();
                var user = await getContact.Content.ReadAsAsync<dynamic>();
                Assert.AreEqual<int>(userId, (int)user.id, "IDs are not the same");
                Assert.AreEqual<string>(userData.firstname, (string)user.firstname, "Firstname was not updated.");

                Console.WriteLine(userId);
            }
        }

        [TestMethod]
        public async Task Delete()
        {
            using (var hc = APIClient.Instance.GetHttpClient())
            {
                // Find a contact to delete
                var getUsers = await hc.GetAsync(ApiUri);
                getUsers.EnsureSuccessStatusCode();
                var users = await getUsers.Content.ReadAsAsync<List<dynamic>>();

                var userId = (int)users.Where(u => (int)u.id > 100).First().id;

                // Now try to delete the first one in the collection
                var response = await hc.DeleteAsync(ApiUri + "/" + userId);
                Assert.IsFalse(response.IsSuccessStatusCode);

                Console.WriteLine(userId);
            }
        }
    }
}
