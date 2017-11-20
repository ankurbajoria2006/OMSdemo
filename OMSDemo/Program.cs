using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OMSDemo
{
    class Program
    {
        // An example JSON object, with key/value pairs
        static string json = @"[{""DemoField1"":""DemoValue1"",""DemoField2"":""DemoValue2""},{""DemoField3"":""DemoValue3"",""DemoField4"":""DemoValue4""}]";

        // Update customerId to your Operations Management Suite workspace ID
        static string customerId = "3a155103-91f3-40ca-ba4e-f1dca1fdb141";

        // For sharedKey, use either the primary or the secondary Connected Sources client authentication key   
        static string sharedKey = "l4dF18A0FVgB4WpcuTADs3nIrlIRfOsMV5nO7WT7XDTeCfyyPM345OsBiczYn0ZjKssJYhgEg+usESL22cOrkQ==";

        // LogName is name of the event type that is being submitted to Log Analytics
        static string LogName = "IITEST";

        // You can use an optional field to specify the timestamp from the data. If the time field is not specified, Log Analytics assumes the time is the message ingestion time
        static string TimeStampField = "";

        static void Main()
        {
            Guid guid;
            guid = Guid.NewGuid();
            //string abc = @"{ "" RequestId"" : {guid.ToString()},"" "" Message"" : ""Test Error"","" MemberName"" : ""Test Batch"","" SourceFilePath"" : ""abc"","" SourceLineNumber"" : 223, ""EventType"" : ""Error"" }";
            string abc = @"{ "" RequestId"" : ""572d7085-92a7-47ef-967a-0f9b4d046641"","" Message"" : ""Test Error"","" MemberName"" : ""Test Batch"","" SourceFilePath"" : ""abc"","" SourceLineNumber"" : 223, ""EventType"" : ""Error"" }";


            // Create a hash for the API signature
            var datestring = DateTime.UtcNow.ToString("r");
            var jsonBytes = Encoding.UTF8.GetBytes("Test");
            string stringToHash = "POST\n" + abc.Length + "\napplication/json\n" + "x-ms-date:" + datestring + "\n/api/logs";
            string hashedString = BuildSignature(stringToHash, sharedKey);
            string signature = "SharedKey " + customerId + ":" + hashedString;

            PostData(signature, datestring, abc);
        }

        // Build the API signature
        public static string BuildSignature(string message, string secret)
        {
            var encoding = new System.Text.ASCIIEncoding();
            byte[] keyByte = Convert.FromBase64String(secret);
            byte[] messageBytes = encoding.GetBytes(message);
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hash = hmacsha256.ComputeHash(messageBytes);
                return Convert.ToBase64String(hash);
            }
        }

        // Send a request to the POST API endpoint
        public static void PostData(string signature, string date, string json)
        {
            try
            {
                string url = "https://" + customerId + ".ods.opinsights.azure.us/api/logs?api-version=2016-04-01";

                System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("Log-Type", LogName);
                client.DefaultRequestHeaders.Add("Authorization", signature);
                client.DefaultRequestHeaders.Add("x-ms-date", date);
                client.DefaultRequestHeaders.Add("time-generated-field", TimeStampField);

                System.Net.Http.HttpContent httpContent = new StringContent(json, Encoding.UTF8);
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                Task<System.Net.Http.HttpResponseMessage> response = client.PostAsync(new Uri(url), httpContent);

                System.Net.Http.HttpContent responseContent = response.Result.Content;
                string result = responseContent.ReadAsStringAsync().Result;
                Console.WriteLine("Return Result: " + result);
            }
            catch (Exception excep)
            {
                Console.WriteLine("API Post Exception: " + excep.Message);
            }
        }
    }
}
