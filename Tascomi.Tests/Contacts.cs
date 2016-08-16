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
    public class Contacts
    {
        private const string ApiUri = "/rest/v1/contacts";

        [TestMethod]
        public async Task Create()
        {
            using (var hc = APIClient.Instance.GetHttpClient())
            {
                var contactData = new {
                                      firstname = "Unit",
                                      surname = "Test " + DateTime.Now.ToString("r")
                                  };

                var content = new FormUrlEncodedContent(contactData.ToDictionary<string>().ToList());

                var response = await hc.PutAsync(ApiUri, content);

                Assert.IsTrue(response.IsSuccessStatusCode);

                var contact = await response.Content.ReadAsAsync<dynamic>();
                
                Assert.IsNotNull(contact.id);

                Assert.AreEqual<string>(contactData.firstname, (string)contact.firstname, "Firstnames don't match");
                Assert.AreEqual<string>(contactData.surname, (string)contact.surname, "Surnames don't match");

                Console.WriteLine(contact.id);
            }
        }

        [TestMethod]
        public async Task ReadOne()
        {
            using (var hc = APIClient.Instance.GetHttpClient())
            {
                // Find a contact
                var getContacts = await hc.GetAsync(ApiUri);
                getContacts.EnsureSuccessStatusCode();
                var contacts = await getContacts.Content.ReadAsAsync<List<dynamic>>();

                var contactId = (int)contacts.First().id;

                var response = await hc.GetAsync(ApiUri + "/" + contactId);
                Assert.IsTrue(response.IsSuccessStatusCode, "Request wasn't successful");
                var contact = await response.Content.ReadAsAsync<dynamic>();
                Assert.AreEqual<int>(contactId, (int)contact.id, "IDs are not equal");

                Console.WriteLine(contactId);
            }
        }

        [TestMethod]
        public async Task ReadMany()
        {
            using (var hc = APIClient.Instance.GetHttpClient())
            {
                var response = await hc.GetAsync(ApiUri);
                Assert.IsTrue(response.IsSuccessStatusCode, "Request wasn't successful");
                var contacts = await response.Content.ReadAsAsync<List<dynamic>>();
                Assert.IsTrue(contacts.Count() > 0);
            }
        }

        [TestMethod]
        public async Task ReadManyWithFilter()
        {
            using (var hc = APIClient.Instance.GetHttpClient())
            {
                var response = await hc.GetAsync(ApiUri + "?firstname=Mohammed");
                Assert.IsTrue(response.IsSuccessStatusCode, "Request wasn't successful");
                var contacts = await response.Content.ReadAsAsync<List<dynamic>>();
                Assert.IsTrue(contacts.Count() > 0);
                Assert.IsTrue(contacts.Where(c => c.firstname == "Mohammed").Count() > 0);
            }
        }

        [TestMethod]
        public async Task ReadManySorted()
        {
            using (var hc = APIClient.Instance.GetHttpClient())
            {
                var response = await hc.GetAsync(ApiUri + "?sort=-surname");
                Assert.IsTrue(response.IsSuccessStatusCode, "Request wasn't successful");
                var contacts = await response.Content.ReadAsAsync<List<dynamic>>();
                Assert.IsTrue(contacts.Count() > 0);
                var expectedContacts = contacts.OrderByDescending(c => c.surname.ToString().ToLower()).ToList();
                Assert.IsTrue(expectedContacts.SequenceEqual(contacts, new PropertyComparer<dynamic>("surname")), "Lists are not ordered the same");
            }
        }

        [TestMethod]
        public async Task Update()
        {
            using (var hc = APIClient.Instance.GetHttpClient())
            {
                // Find a contact to update
                var getContacts = await hc.GetAsync(ApiUri);
                getContacts.EnsureSuccessStatusCode();
                var contacts = await getContacts.Content.ReadAsAsync<List<dynamic>>();

                var contactId = (int)contacts.First().id;

                var contactData = new
                {
                    surname = "Test " + DateTime.Now.ToString("r")
                };

                var content = new FormUrlEncodedContent(contactData.ToDictionary<string>().ToList());

                var updateContact = await hc.PostAsync(ApiUri + "/" + contactId, content);
                Assert.IsTrue(updateContact.IsSuccessStatusCode);
                var updatedContact = await updateContact.Content.ReadAsAsync<dynamic>();
                Assert.AreEqual<int>(contactId, (int)updatedContact.id, "IDs are not the same");

                var getContact = await hc.GetAsync(ApiUri + "/" + contactId);
                getContact.EnsureSuccessStatusCode();
                var contact = await getContact.Content.ReadAsAsync<dynamic>();
                Assert.AreEqual<int>(contactId, (int)contact.id, "IDs are not the same");
                Assert.AreEqual<string>(contactData.surname, (string)contact.surname, "Surname was not updated.");

                Console.WriteLine(contactId);
            }
        }

        [TestMethod]
        public async Task Delete()
        {
            using (var hc = APIClient.Instance.GetHttpClient())
            {
                // Find a contact to delete
                var getContacts = await hc.GetAsync(ApiUri);
                getContacts.EnsureSuccessStatusCode();
                var contacts = await getContacts.Content.ReadAsAsync<List<dynamic>>();

                var contactId = (int)contacts.First().id;

                // Now try to delete the first one in the collection
                var response = await hc.DeleteAsync(ApiUri + "/" + contactId);
                Assert.IsTrue(response.IsSuccessStatusCode);

                Console.WriteLine(contactId);
            }
        }
    }
}
