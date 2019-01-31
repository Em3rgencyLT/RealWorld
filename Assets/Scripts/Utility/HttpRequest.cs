using System;
using System.Collections.Generic;
using System.Net;
using System.IO;

namespace Utility {
    public static class HttpRequest
    {
        public static string Get(string url) {
            string data = String.Empty;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                data = reader.ReadToEnd();
            }

            return data;
        }
    }
}
