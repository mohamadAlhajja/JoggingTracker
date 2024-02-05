using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System;
using System.Net.Http;

namespace IntegrationTests
{
    public static class Helpers
    {
 
        public static StringContent ToJsonContent(this object content)
        {
            string content2 = JsonConvert.SerializeObject(content);
            StringContent stringContent = new StringContent(content2);
            stringContent.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json");
            return stringContent;
        }

        public static T Deserialize<T>(this HttpResponseMessage response)
        {
            string result = response.Content.ReadAsStringAsync().Result;
            T val = (T)((typeof(T) == typeof(string)) ? ((object)(T)Convert.ChangeType(result, typeof(T))) : ((object)JsonConvert.DeserializeObject<T>(result)!))!;
            return val;
        }

        public static T DeserializeAnonymous<T>(this HttpResponseMessage response, T anonymousTypeInstance)
        {
            string result = response.Content.ReadAsStringAsync().Result;

            object deserializedObject = JsonConvert.DeserializeAnonymousType(result, anonymousTypeInstance);

            T val = (T)deserializedObject;
            return val;
        }
    }
}
