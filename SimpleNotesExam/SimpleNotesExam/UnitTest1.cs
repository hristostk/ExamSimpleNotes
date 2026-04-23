using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using SimpleNotesExam.Models;
using System.Net;
using System.Text.Json;

namespace SimpleNotes
{
    [TestFixture]
    public class SimpleNotesTests
    {
        private RestClient client;
        private static string createdNoteId;
        private static string baseURL = "http://144.91.123.158:5005/api";

        [OneTimeSetUp]
        public void Setup()
        {
            // Get token for authentication
            string token = GetJwtToken("Ico129@gmail.com", "hristo123");

            // Create client with token
            var options = new RestClientOptions(baseURL)
            {
                Authenticator = new JwtAuthenticator(token)
            };

            client = new RestClient(options);
        }

        // Method to login and get JWT token
        private string GetJwtToken(string email, string password)
        {
            var loginClient = new RestClient(baseURL);

            var request = new RestRequest("/User/Authorization", Method.Post);
            request.AddJsonBody(new { email, password });

            var response = loginClient.Execute(request);

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            return json.GetProperty("accessToken").GetString();
        }

        [Test, Order(1)]
        public void CreateNote_WithoutRequiredFields_ShouldReturnBadRequest()
        {
            var note = new
            {
                Title = "",
                Description = "",
                Status = ""
            };

            var request = new RestRequest("/Note/Create", Method.Post);
            request.AddJsonBody(note);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test, Order(2)]
        public void CreateNote_WithRequiredFields_ShouldReturnOk()
        {
            var note = new
            {
                Title = "Test Note Title",
                Description = "This is a test note description with enough characters",
                Status = "New"
            };

            var request = new RestRequest("/Note/Create", Method.Post);
            request.AddJsonBody(note);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            string message = json.GetProperty("msg").GetString();
            Assert.That(message, Is.EqualTo("Note created successfully!"));
        }

        [Test, Order(3)]
        public void GetAllNotes_ShouldReturnOk()
        {
            var request = new RestRequest("/Note/AllNotes", Method.Get);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var notes = JsonSerializer.Deserialize<List<NoteDto>>(
                JsonDocument.Parse(response.Content).RootElement.GetProperty("allNotes").GetRawText()
            );

            Assert.That(notes, Is.Not.Empty);

            createdNoteId = notes.Last().Id;
            Assert.That(createdNoteId, Is.Not.Null.And.Not.Empty);
        }

        [Test, Order(4)]
        public void EditNote_ShouldReturnOk()
        {
            var noteChanges = new
            {
                Title = "Updated Note Title",
                Description = "This is an updated note description with enough characters",
                Status = "Done"
            };

            var request = new RestRequest($"/Note/Edit/{createdNoteId}", Method.Put);
            request.AddJsonBody(noteChanges);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            string message = json.GetProperty("msg").GetString();
            Assert.That(message, Is.EqualTo("Note edited successfully!"));
        }

        [Test, Order(5)]
        public void DeleteNote_ShouldReturnOk()
        {
            var request = new RestRequest($"/Note/Delete/{createdNoteId}", Method.Delete);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            string message = json.GetProperty("msg").GetString();
            Assert.That(message, Is.EqualTo("Note deleted successfully!"));
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            client?.Dispose();
        }
    }
}