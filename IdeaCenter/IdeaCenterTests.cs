using IdeaCenter.Models;
using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Text.Json;


namespace IdeaCenter
{
    public class IdeaCenterTests
    {
        private RestClient client;
        private const string BaseUrl = "http://softuni-qa-loadbalancer-2137572849.eu-north-1.elb.amazonaws.com:84";

        private const string LoginEmail = "valee70@example.com";
        private const string LoginPassword = "qwe321";

        private static Random random = new Random();
        private static string? lastCreatedIdeaId;
        private const string nonExistedIdeaId = "-1";

        [OneTimeSetUp]
        public void Setup()
        {
            string jwtToken = GetJwtToken(LoginEmail, LoginPassword); 
            var options = new RestClientOptions(BaseUrl)
            {
                Authenticator = new JwtAuthenticator(jwtToken),
            };
            client = new RestClient(options);
        }

        private string GetJwtToken(string email, string password)
        {
            RestClient tmpClient = new RestClient(BaseUrl);
            RestRequest? request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { email, password });
            RestResponse? response = tmpClient.Post(request);
            
            if (response.IsSuccessStatusCode)
            {
                JsonElement content = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(response.Content);
                string token = content.GetProperty("accessToken").ToString();
                return token;
            }
            else
            {
                throw new InvalidOperationException($"Failed to authenticate. Status code: {response.StatusCode}, Content: {response.Content}");
            }
        }

        private static string GetRandomString(int length)
        {
            const string? chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            char[] stringChars = new char[length];

            for (int i = 0; i < length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return new string(stringChars);
        }

        //1.3. Create a New Idea with the Required Fields
        [Test, Order(1)]
        public void Test1_Create_a_New_Idea_with_the_Required_Fields()
        {
            //•	Create a test to send a POST request to add a new idea.
            IdeaDTO? newIdea = new IdeaDTO
            {
                Title = $"Test Idea {GetRandomString(6)}",
                Description = $"This is a test idea description.",
                Url = ""
            };
            RestRequest? request = new RestRequest("/api/Idea/Create", Method.Post);
            request.AddJsonBody(newIdea);
            RestResponse? response = client.Execute(request);

            ApiResponseDTO? ApiResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            //•	Assert that the response status code is OK(200).
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            //•	Assert that the response message indicates the idea was "Successfully created!".
            Assert.That(ApiResponse?.Msg, Is.EqualTo("Successfully created!"));
        }

        //1.4. Get All Ideas
        [Test, Order(2)]
        public void Test2_Get_All_Ideas()
        {
            //•	Create a test to send a GET request to list all ideas.
            RestRequest? request = new RestRequest("/api/Idea/All", Method.Get);
            RestResponse response = client.Execute(request);

            List<ApiResponseDTO>? ApiResponseItems = JsonSerializer.Deserialize<List<ApiResponseDTO>>(response.Content);

            //•	Assert that the response status code is OK(200).
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            //•	Assert that the response contains a non-empty array.
            Assert.That(ApiResponseItems, Is.Not.Empty);

            //•	Store the id of the last created idea in a static member of the test class to maintain its value between test runs.
            lastCreatedIdeaId = ApiResponseItems.LastOrDefault()?.IdeaId;
            //Console.WriteLine(ApiResponseItems.LastOrDefault().IdeaId);
            //Console.WriteLine(ApiResponseItems[ApiResponseItems.Count-1].IdeaId);
        }

        //1.5. Edit the Last Idea that you Created
        [Test, Order(3)]
        public void Test3_Edit_the_Last_Idea()
        {
            //•	Create a test that sends a PUT request to edit the idea.
            IdeaDTO? editIdea = new IdeaDTO
            {
                Title = $"Edited Idea {GetRandomString(10)}",
                Description = $"This is an edited idea {GetRandomString(6)} description. ",
                Url = ""
            };
            RestRequest? request = new RestRequest("/api/Idea/Edit", Method.Put);
            request.AddJsonBody(editIdea);
            //•	Use the id that you stored in the previous request as a query parameter.
            request.AddQueryParameter("ideaId", lastCreatedIdeaId);
            RestResponse response = client.Execute(request);

            ApiResponseDTO? ApiResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            //•	Assert that the response status code is OK(200).
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            //•	Assert that the response message indicates the idea was "Successfully edited".
            Assert.That(ApiResponse?.Msg, Is.EqualTo("Edited successfully"));
        }

        //1.6. Delete the Idea that you Edited
        [Test, Order(4)]
        public void Test4_Delete_the_Idea()
        {
            //•	Create a test that sends a DELETE request.
            RestRequest? request = new RestRequest("/api/Idea/Delete", Method.Delete);
            //•	Use the id that you stored in the "Get All Ideas" request as a query parameter.
            request.AddQueryParameter("ideaId", lastCreatedIdeaId);
            RestResponse response = client.Execute(request);

            //•	Assert that the response status code is OK(200).
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            //•	Confirm that the response contains "The idea is deleted!". 
            Assert.That(response.Content, Does.Contain("The idea is deleted!"));
        }

        //1.7. Try to Create an Idea without the Required Fields
        [Test, Order(5)]
        public void Test5_Try_to_Create_an_Idea_without_the_Required_Fields()
        {
            //•	Write a test that attempts to create a idea with missing required fields(Title, Description).
            IdeaDTO? newIdea = new IdeaDTO
            {
                Title = "",
                Description = "",
                Url = ""
            };
            //•	Send the POST request with the incomplete data.
            RestRequest request = new RestRequest("/api/Idea/Create", Method.Post);
            request.AddJsonBody(newIdea);
            RestResponse response = client.Execute(request);

            //•	Assert that the response status code is BadRequest (400).
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            Assert.That(response.Content, Does.Contain("The Title field is required."));
            Assert.That(response.Content, Does.Contain("The Description field is required."));
        }

        //1.8. Try to Edit a Non-existing Idea
        //* Keep in mind that the response in not a json object, but a string!
        [Test, Order(6)]
        public void Test6_Try_to_Edit_a_Non_existing_Idea()
        {
            //•	Create a test that sends a PUT request to edit the idea.
            IdeaDTO? editIdea = new IdeaDTO
            {
                Title = "Edited Non-Existing Idea",
                Description = "This is an updated test idea description for a non-existing idea.",
                Url = ""
            };
            RestRequest request = new RestRequest("/api/Idea/Edit", Method.Put);
            request.AddJsonBody(editIdea);
            
            //•	Write a test to send a PUT request to edit an Idea with a ideaId that does not exist.
            request.AddQueryParameter("ideaId", nonExistedIdeaId);
            RestResponse response = client.Execute(request);

            //•	Assert that the response status code is BadRequest(400).
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            //•	Assert that the response contains "There is no such idea!".
            Assert.That(response.Content, Does.Contain("There is no such idea!"));
        }

        //1.9. Try to Delete a Non-existing Idea
        //* Keep in mind that the response in not a json object, but a string!
        [Test, Order(7)]
        public void Test7_Try_to_Delete_a_NonExisting_Idea()
        {
            //•	Write a test to send a DELETE request to edit an Idea with a ideaId that does not exist.
            RestRequest request = new RestRequest("/api/Idea/Delete", Method.Delete);
            
            request.AddQueryParameter("ideaId", nonExistedIdeaId);
            RestResponse response = client.Execute(request);

            //•	Assert that the response status code is BadRequest(400).
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            //•	Assert that the response contains "There is no such idea!".
            Assert.That(response.Content, Does.Contain("There is no such idea!"));
        }


        [OneTimeTearDown]
        public void TearDown() 
        { 
            client.Dispose();
        }
    }
}