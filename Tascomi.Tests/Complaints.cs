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
    public class Complaints
    {
        private const string ApiUri = "/rest/v1/complaints";

        [TestMethod]
        public async Task Create()
        {
            using (var hc = APIClient.Instance.GetHttpClient())
            {
                var complaintData = new {
                    remarks = "Test " + DateTime.Now.ToString("r"),
                    asbo = "true" };

                var content = new FormUrlEncodedContent(complaintData.ToDictionary<string>().ToList());

                var response = await hc.PutAsync(ApiUri, content);

                Assert.IsTrue(response.IsSuccessStatusCode);

                var complaint = await response.Content.ReadAsAsync<dynamic>();

                Assert.IsNotNull(complaint.id);

                Assert.AreEqual<string>(complaintData.remarks, (string)complaint.remarks, "Remarks don't match");

                Console.WriteLine(complaint.id);
            }
        }

        [TestMethod]
        public async Task ReadOne()
        {
            using (var hc = APIClient.Instance.GetHttpClient())
            {
                // Find a contact
                var getComplaints = await hc.GetAsync(ApiUri);
                getComplaints.EnsureSuccessStatusCode();
                var complaints = await getComplaints.Content.ReadAsAsync<List<dynamic>>();

                var complaintId = (int)complaints.First().id;

                var response = await hc.GetAsync(ApiUri + "/" + complaintId);
                Assert.IsTrue(response.IsSuccessStatusCode, "Request wasn't successful");
                var complaint = await response.Content.ReadAsAsync<dynamic>();
                Assert.AreEqual<int>(complaintId, (int)complaint.id, "IDs are not equal");

                Console.WriteLine(complaintId);
            }
        }

        [TestMethod]
        public async Task ReadMany()
        {
            using (var hc = APIClient.Instance.GetHttpClient())
            {
                var response = await hc.GetAsync(ApiUri);
                Assert.IsTrue(response.IsSuccessStatusCode, "Request wasn't successful");
                var complaints = await response.Content.ReadAsAsync<List<dynamic>>();
                Assert.IsTrue(complaints.Count() > 0);
            }
        }

        //[TestMethod]
        //public async Task ReadManyWithFilter()
        //{
        //    using (var hc = APIClient.Instance.GetHttpClient())
        //    {
        //        var response = await hc.GetAsync(ApiUri + "?firstname=Mohammed");
        //        Assert.IsTrue(response.IsSuccessStatusCode, "Request wasn't successful");
        //        var complaints = await response.Content.ReadAsAsync<List<dynamic>>();
        //        Assert.IsTrue(complaints.Count() > 0);
        //        Assert.IsTrue(complaints.Where(c => c.firstname == "Mohammed").Count() > 0);
        //    }
        //}

        [TestMethod]
        public async Task ReadManySorted()
        {
            using (var hc = APIClient.Instance.GetHttpClient())
            {
                var response = await hc.GetAsync(ApiUri + "?sort=-submit_date");
                Assert.IsTrue(response.IsSuccessStatusCode, "Request wasn't successful");
                var complaints = await response.Content.ReadAsAsync<List<dynamic>>();
                Assert.IsTrue(complaints.Count() > 0);
                var expectedComplaints = complaints.OrderByDescending(c => (DateTime)c.submit_date).ToList();
                Assert.IsTrue(expectedComplaints.SequenceEqual(complaints, new PropertyComparer<dynamic>("submit_date")), "Lists are not ordered the same");
            }
        }

        [TestMethod]
        public async Task Update()
        {
            using (var hc = APIClient.Instance.GetHttpClient())
            {
                // Find a contact to update
                var getComplaints = await hc.GetAsync(ApiUri);
                getComplaints.EnsureSuccessStatusCode();
                var complaints = await getComplaints.Content.ReadAsAsync<List<dynamic>>();

                var complaintId = (int)complaints.First().id;

                var complaintData = new
                {
                    remarks = "Test " + DateTime.Now.ToString("r")
                };

                var content = new FormUrlEncodedContent(complaintData.ToDictionary<string>().ToList());

                var updateContact = await hc.PostAsync(ApiUri + "/" + complaintId, content);
                Assert.IsTrue(updateContact.IsSuccessStatusCode);
                var updatedContact = await updateContact.Content.ReadAsAsync<dynamic>();
                Assert.AreEqual<int>(complaintId, (int)updatedContact.id, "IDs are not the same");

                var getContact = await hc.GetAsync(ApiUri + "/" + complaintId);
                getContact.EnsureSuccessStatusCode();
                var complaint = await getContact.Content.ReadAsAsync<dynamic>();
                Assert.AreEqual<int>(complaintId, (int)complaint.id, "IDs are not the same");
                Assert.AreEqual<string>(complaintData.remarks, (string)complaint.remarks, "Remarks was not updated.");

                Console.WriteLine(complaintId);
            }
        }

        [TestMethod]
        public async Task Delete()
        {
            using (var hc = APIClient.Instance.GetHttpClient())
            {
                // Find a contact to delete
                var getComplaints = await hc.GetAsync(ApiUri);
                getComplaints.EnsureSuccessStatusCode();
                var complaints = await getComplaints.Content.ReadAsAsync<List<dynamic>>();

                var complaintId = (int)complaints.First().id;

                // Now try to delete the first one in the collection
                var response = await hc.DeleteAsync(ApiUri + "/" + complaintId);
                Assert.IsTrue(response.IsSuccessStatusCode);

                Console.WriteLine(complaintId);
            }
        }
    }
}
