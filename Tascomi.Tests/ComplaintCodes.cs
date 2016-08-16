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
    public class ComplaintCodes
    {
        private const string ApiUri = "/rest/v1/complaint_codes";

        [TestMethod]
        public async Task Create()
        {
            using (var hc = APIClient.Instance.GetHttpClient())
            {
                var codeData = new {
                    code = "T" + DateTime.Now.Millisecond.ToString(),
                    description = "Test " + DateTime.Now.ToString("r")
                };

                var content = new FormUrlEncodedContent(codeData.ToDictionary<string>().ToList());

                var response = await hc.PutAsync(ApiUri, content);

                Assert.IsFalse(response.IsSuccessStatusCode);
            }
        }

        [TestMethod]
        public async Task ReadOne()
        {
            using (var hc = APIClient.Instance.GetHttpClient())
            {
                // Find a code
                var getCodes = await hc.GetAsync(ApiUri);
                getCodes.EnsureSuccessStatusCode();
                var codes = await getCodes.Content.ReadAsAsync<List<dynamic>>();

                var codeId = (int)codes.First().id;

                var response = await hc.GetAsync(ApiUri + "/" + codeId);
                Assert.IsTrue(response.IsSuccessStatusCode, "Request wasn't successful");
                var code = await response.Content.ReadAsAsync<dynamic>();
                Assert.AreEqual<int>(codeId, (int)code.id, "IDs are not equal");

                Console.WriteLine(codeId);
            }
        }

        [TestMethod]
        public async Task ReadMany()
        {
            using (var hc = APIClient.Instance.GetHttpClient())
            {
                var response = await hc.GetAsync(ApiUri);
                Assert.IsTrue(response.IsSuccessStatusCode, "Request wasn't successful");
                var codes = await response.Content.ReadAsAsync<List<dynamic>>();
                Assert.IsTrue(codes.Count() > 0);
            }
        }

        [TestMethod]
        public async Task ReadManyWithFilter()
        {
            using (var hc = APIClient.Instance.GetHttpClient())
            {
                var response = await hc.GetAsync(ApiUri + "?code=EP");
                Assert.IsTrue(response.IsSuccessStatusCode, "Request wasn't successful");
                var codes = await response.Content.ReadAsAsync<List<dynamic>>();
                Assert.IsTrue(codes.Count() > 0);
                Assert.IsTrue(codes.Where(c => c.code == "EP").Count() > 0);
            }
        }

        [TestMethod]
        public async Task ReadManySorted()
        {
            using (var hc = APIClient.Instance.GetHttpClient())
            {
                var response = await hc.GetAsync(ApiUri + "?sort=-code");
                Assert.IsTrue(response.IsSuccessStatusCode, "Request wasn't successful");
                var codes = await response.Content.ReadAsAsync<List<dynamic>>();
                Assert.IsTrue(codes.Count() > 0);
                var expectedCodes = codes.OrderByDescending(c => c.code.ToString().ToLower()).ToList();
                Assert.IsTrue(expectedCodes.SequenceEqual(codes, new PropertyComparer<dynamic>("code")), "Lists are not ordered the same");
            }
        }

        [TestMethod]
        public async Task Update()
        {
            using (var hc = APIClient.Instance.GetHttpClient())
            {
                // Find a code to update
                var getCodes = await hc.GetAsync(ApiUri);
                getCodes.EnsureSuccessStatusCode();
                var codes = await getCodes.Content.ReadAsAsync<List<dynamic>>();

                var codeId = (int)codes.First().id;

                var codeData = new
                {
                    code = "T" + DateTime.Now.Millisecond
                };

                var content = new FormUrlEncodedContent(codeData.ToDictionary<string>().ToList());

                var updateCode = await hc.PostAsync(ApiUri + "/" + codeId, content);
                Assert.IsFalse(updateCode.IsSuccessStatusCode);
                var updatedCode = await updateCode.Content.ReadAsAsync<dynamic>();
                Assert.AreEqual<int>(codeId, (int)updatedCode.id, "IDs are not the same");

                var getContact = await hc.GetAsync(ApiUri + "/" + codeId);
                getContact.EnsureSuccessStatusCode();
                var code = await getContact.Content.ReadAsAsync<dynamic>();
                Assert.AreEqual<int>(codeId, (int)code.id, "IDs are not the same");
                Assert.AreEqual<string>(codeData.code, (string)code.code, "Code was not updated.");

                Console.WriteLine(codeId);
            }
        }

        [TestMethod]
        public async Task Delete()
        {
            using (var hc = APIClient.Instance.GetHttpClient())
            {
                // Find a contact to delete
                var getCodes = await hc.GetAsync(ApiUri);
                getCodes.EnsureSuccessStatusCode();
                var codes = await getCodes.Content.ReadAsAsync<List<dynamic>>();

                var codeId = (int)codes.First().id;

                // Now try to delete the first one in the collection
                var response = await hc.DeleteAsync(ApiUri + "/" + codeId);
                Assert.IsFalse(response.IsSuccessStatusCode);

                Console.WriteLine(codeId);
            }
        }
    }
}
