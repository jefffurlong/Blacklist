using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using Amazon.Runtime.Internal.Settings;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace crm_blacklist
{
    public static class Blacklist
    {
        public static string url;
        public static string key;
        public static string username;

        public static byte[] domainBuffer;
        public static byte[] contactsBuffer;

        public static string TotalDomainEntries;
        public static string TotalContactEntries;

        public static bool bDomainExist = false;
        public static bool bContactExist = false;

        public static string GetJsonStream(string url, string username, string password)
        {
            string myResponse = null;
            
            WebRequest req;

            try
            {
                req = WebRequest.Create(@url);
            }
            catch (WebException ex)
            {
                Log.Error(ex.ToString());
                return null;
            }
            
            req.Method = "GET";
            req.Headers["Authorization"] =
                "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(username + ":" + password));

            try
            {
                WebResponse response = req.GetResponse();
                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.UTF8);
                    myResponse = reader.ReadToEnd();
                    
                    response.Close();
                    
                    Log.Information("Response from GetJsonStream: " + myResponse);
                    return myResponse;
                }
            }
            catch (WebException ex)
            {
                WebResponse errorResponse = ex.Response;
                String errorText;
                
                using (Stream responseStream = errorResponse.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.GetEncoding("utf-8"));
                    errorText = reader.ReadToEnd();
                    
                    Log.Error(errorText);

                }

                return null;
            }

        }

        public static string PostJsonStream(string url, string username, string password, string token, string blob,
            string uniqueid)
        {
            string myResponse = null;

            //Create JSON

            string diag = "{\"token\": \"" + token + "\"" + ", \"blob\": \"" + blob + "\"" + ", \"client_type\": \"BTF-VOIP\"" + ", \"uniquifier\": \"" +
                          uniqueid + "\"}";

            Log.Information(diag);
            
            WebRequest req;
            
            try
            {
                req = WebRequest.Create(@url);
            }
            catch (WebException ex)
            {
                Log.Error(ex.ToString());
                return null;
            }

            req.Method = "POST";
            
            // turn our request string into a byte stream
            byte[] postBytes = Encoding.UTF8.GetBytes(diag);

            // this is important - make sure you specify type this way
            req.ContentType = "application/json; charset=UTF-8";
            req.ContentLength = postBytes.Length;
            Stream requestStream = req.GetRequestStream();

            try
            {

                requestStream.Write(postBytes, 0, postBytes.Length);
                requestStream.Close();

                WebResponse response = req.GetResponse();

                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.UTF8);
                    myResponse = reader.ReadToEnd();

                    response.Close();

                    Log.Information("Response: " + myResponse);
                    return myResponse;
                }
            }
            catch (WebException ex)
            {
                WebResponse errorResponse = ex.Response;
                String errorText;
                
                using (Stream responseStream = errorResponse.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.GetEncoding("utf-8"));
                    errorText = reader.ReadToEnd();
                    
                    //Console.WriteLine(errorText);
                    Log.Error(errorText);

                }

                return null;
            }

        }
        
        public static void ParseBlacklistDomainJson()
        {
            
            var json = System.IO.File.ReadAllText(@"btf_domains.txt");
            
            var objects = JObject.Parse(json);
            
            //Get total number of entries
            TotalDomainEntries = (String) objects.GetValue("total_entries");

            //We know there's at least one entry.
            var entries = JsonConvert.DeserializeObject<JArray>(objects.GetValue("entries").ToString()).ToObject<List<JObject>>().FirstOrDefault();
            
            string domain = (String)entries.GetValue("domain");

            Log.Information("Domain: " + domain);

            //Check for more entries.
            for( int i = 1; i < Convert.ToInt32(TotalDomainEntries); i++ )
            {
                entries = JsonConvert.DeserializeObject<JArray>(objects.GetValue("entries").ToString())
                    .ToObject<List<JObject>>()[i];

                domain = (String)entries.GetValue("domain");
                
                Log.Information("Domain: " + domain);
                
            }
        }
        
        public static void ParseBlacklistContactsJson()
        {
            var json = System.IO.File.ReadAllText(@"btf_contacts.txt");
            
            var objects = JObject.Parse(json);
            
            //Get total number of entries
            TotalContactEntries = (String) objects.GetValue("total_entries");

            //We know there's at least one entry.
            var entries = JsonConvert.DeserializeObject<JArray>(objects.GetValue("entries").ToString()).ToObject<List<JObject>>().FirstOrDefault();
            
            string domain = (String)entries.GetValue("domain");

            Log.Information("Domain: " + domain);

            //Check for more entries.
            for( int i = 1; i < Convert.ToInt32(TotalContactEntries); i++ )
            {
                entries = JsonConvert.DeserializeObject<JArray>(objects.GetValue("entries").ToString())
                    .ToObject<List<JObject>>()[i];

                domain = (String)entries.GetValue("domain");
                
                Log.Information("Domain: " + domain);
                
            }
        }
        
        public static void WriteFile(bool bContacts, string fileName, string jsonString)
        {
            byte[] buffer = Encoding.ASCII.GetBytes((jsonString));

            MemoryStream ms = new MemoryStream(buffer);
            
            if (bContacts)
            {
                contactsBuffer = buffer.ToArray();
                
            }
            else
            {
                domainBuffer = buffer.ToArray();
                
            }


            //write to file
            FileStream file = new FileStream(fileName, FileMode.Create, FileAccess.Write);

            ms.WriteTo(file);
            file.Close();
            ms.Close();
        }
    }
}