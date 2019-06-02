using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CoreBot.DataService
{
    public class DataAPI
    {
        public static string DataAPIURL = "";

        public static string GetOnSpotPerformanceData(string Geo, string Date)
        {
            try
            {
                string URL = DataAPIURL + string.Format("/MDX?Geo={0}&Date={1}", Geo, Date);
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(URL);
                    client.DefaultRequestHeaders.Clear();
                    HttpResponseMessage resMessage = client.GetAsync(URL).Result;
                    return resMessage.Content.ReadAsStringAsync().Result;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "Exception";
            }
        }

    }
}
