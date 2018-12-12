
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Xml;
using Newtonsoft.Json;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using Serilog.Sinks.SystemConsole.Themes;


//using Salesforce.Force;

namespace crm_blacklist
{
    public class Program
    {
        public static void Main(string[] args)
        {
         
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}", theme: AnsiConsoleTheme.Code)
                .CreateLogger();

            string uniqueid = "BTF-VOIP:" + DateTime.Now.ToString() + DateTime.Now.Ticks.ToString();
            
            //Parse Configuration File
            if ( ParseParameterFile() == false )
                return;
        
            //Retrieve Blacklisted Domains
            String pingpong = Blacklist.PostJsonStream(Blacklist.url + "diag/ping.json",
                Blacklist.username, Blacklist.key, "Main Process Start", "Parsing parameter list for accessing Blacklist endpoints", uniqueid);

            //Print Process Start Time
            Log.Information("Main Process Start: ");

            //Print Process Start Retrieving Blacklists
            Log.Information("Blacklist Start: ");
            
            //Retrieve Blacklisted Domains
            pingpong = Blacklist.PostJsonStream(Blacklist.url + "diag/ping.json",
                Blacklist.username, Blacklist.key, "Start retrieving blacklist domain list", "Calling api/blacklist_domains_list endpoint", uniqueid);

            //Retrieve Blacklisted Domains
            String FetchDomainList = Blacklist.GetJsonStream(Blacklist.url + "api/blacklist_domains_list.json",
                Blacklist.username, Blacklist.key);

            if (FetchDomainList != null)
            {
                Log.Information(FetchDomainList);

                Blacklist.WriteFile(false, "btf_domains.txt", FetchDomainList);

                Blacklist.bDomainExist = true;
            }

            //Retrieve Blacklisted Contacts
            pingpong = Blacklist.PostJsonStream(Blacklist.url + "diag/ping.json",
                Blacklist.username, Blacklist.key, "Start retrieving blacklist contact list", "Calling api/blacklist_contacts_list endpoint", uniqueid);
            
            //Retrieve Blacklisted Contacts.
            String FetchContactList = Blacklist.GetJsonStream(Blacklist.url + "api/blacklist_contacts_list.json",
                Blacklist.username, Blacklist.key);

            if (FetchContactList != null)
            {
                Log.Information(FetchContactList);
 
                Blacklist.WriteFile(true, "btf_contacts.txt", FetchContactList);

                Blacklist.bContactExist = true;
            }

            //Parse JSON files for required information.
            if (Blacklist.bDomainExist)
            {
                Blacklist.ParseBlacklistDomainJson();
                Log.Information("Total Blacklisted Domain Records: " + Blacklist.TotalDomainEntries);
                
                //Retrieve Blacklisted Domains
                pingpong = Blacklist.PostJsonStream(Blacklist.url + "diag/ping.json",
                    Blacklist.username, Blacklist.key, "Finished retrieving blacklist domain list", "Found " + Blacklist.TotalDomainEntries + " Blacklist domain entries", uniqueid);

            }

            if (Blacklist.bContactExist)
            {
                Blacklist.ParseBlacklistContactsJson();                
                Log.Information("Total Blacklisted Contact Records: " + Blacklist.TotalContactEntries);

                pingpong = Blacklist.PostJsonStream(Blacklist.url + "diag/ping.json",
                    Blacklist.username, Blacklist.key, "Finished retrieving blacklist contact list", "Found " + Blacklist.TotalContactEntries + " Blacklist contact entries", uniqueid);

            }
            
            
            //Print Process Finished Retrieving Blacklists
            Log.Information("Blacklist End: ");

            //Print Process End Time
            pingpong = Blacklist.PostJsonStream(Blacklist.url + "diag/ping.json",
                Blacklist.username, Blacklist.key, "Exiting Application", "Completed retrieving blacklist domains and contacts", uniqueid);
            
            Log.Information("Process Finished:");
            
        }

        private static bool ParseParameterFile()
        {
            string node = null;
            string param = null;
            
            //Read in XML file
            XmlTextReader reader = new XmlTextReader("Parameters.xml");

            //Loop through nodes until we get to App Settings
            while (reader.Read())
            {
                node = reader.Name;

                if (node == "add")
                {
                    reader.MoveToNextAttribute();
                    param = reader.Value;
                    
                    //Move to Value
                    reader.MoveToNextAttribute();

                    //Assign proper settings
                    switch (param.ToUpper())
                    {
                        case "USERNAME":
                            Blacklist.username = reader.Value;
                            break;
                        
                        case "URL":
                            Blacklist.url = reader.Value;
                            break;
                        
                        case "KEY":
                            Blacklist.key = reader.Value;
                            break;
                    }

                    
                }

            }

            Log.Information(Blacklist.username + " " + Blacklist.url + " " + Blacklist.key);

            if (Blacklist.username == null || Blacklist.url == null || Blacklist.key == null)
                return false;
            
            return true;
        }

        
    }
}