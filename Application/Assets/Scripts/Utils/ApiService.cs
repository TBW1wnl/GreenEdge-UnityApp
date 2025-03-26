using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

public class ApiService
{
    private static readonly HttpClient client = new HttpClient();

    public static async Task Save(string json)
    {
        try
        {
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync("http://localhost:8000/save_sphere", content);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("JSON sent successfully!");
            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
    }
}
