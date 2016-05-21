using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;

namespace iBoardBot.Helpers {
    public static class UploadHelper {
        public static string HttpUploadFile(string url, byte[] fileBytes , string paramName, string contentType, NameValueCollection nvc) {
            var boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            var boundarybytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
            var wr = (HttpWebRequest) WebRequest.Create(url);

            wr.ContentType = "multipart/form-data; boundary=" + boundary;
            wr.Method = "POST";
            wr.KeepAlive = true;
            wr.Credentials = CredentialCache.DefaultCredentials;

            var rs = wr.GetRequestStream();
            var formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";

            foreach (string key in nvc.Keys) {
                rs.Write(boundarybytes, 0, boundarybytes.Length);
                var formitem = string.Format(formdataTemplate, key, nvc[key]);
                var formitembytes = Encoding.UTF8.GetBytes(formitem);
                rs.Write(formitembytes, 0, formitembytes.Length);
            }

            rs.Write(boundarybytes, 0, boundarybytes.Length);

            var headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
            var header = string.Format(headerTemplate, paramName, "image.png", contentType);
            var headerbytes = Encoding.UTF8.GetBytes(header);

            rs.Write(headerbytes, 0, headerbytes.Length);
            rs.Write(fileBytes, 0, fileBytes.Length);

            var trailer = Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            rs.Write(trailer, 0, trailer.Length);
            rs.Close();
            WebResponse wresp = null;

            wresp = wr.GetResponse();

            var stream2 = wresp.GetResponseStream();
            var reader2 = new StreamReader(stream2);

            return reader2.ReadToEnd();
        }
    }
}
