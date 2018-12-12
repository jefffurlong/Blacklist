using System;
using Xunit;
using crm_blacklist;

namespace voip_btf.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            String response = Blacklist.GetJsonStream("http://localhost:3000/api/blacklist_domains_list.json", "poweruser2@power.com",
                "M5iecZ7ddkn7dWmJC_aHJAYkxzRvkWJYEMeW4StS");
            
           Assert.NotNull(response);
        }
        
        [Fact]
        public void Test2()
        {
            string response = Blacklist.GetJsonStream("http://localhost:3000/api/blacklist_domains_list.json", "poweruser2@power.com",
                "no_valid_key");
            
            Assert.Null(response);
        }
        
        [Fact]
        public void Test3()
        {
            string response = Blacklist.GetJsonStream("http://localhost:3000/api/blacklist_domains_list.json", "no_valid_user",
                "M5iecZ7ddkn7dWmJC_aHJAYkxzRvkWJYEMeW4StS");
            
            Assert.Null(response);
        }
        
        [Fact]
        public void Test4()
        {
            String response = Blacklist.GetJsonStream("http://localhost:3000/api/blacklist_contacts_list.json", "poweruser2@power.com",
                "M5iecZ7ddkn7dWmJC_aHJAYkxzRvkWJYEMeW4StS");
            
            Assert.NotNull(response);
        }
        
        [Fact]
        public void Test5()
        {
            string response = Blacklist.GetJsonStream("http://localhost:3000/api/blacklist_contacts_list.json", "poweruser2@power.com",
                "no_valid_key");
            
            Assert.Null(response);
        }
        
        [Fact]
        public void Test6()
        {
            string response = Blacklist.GetJsonStream("http://localhost:3000/api/blacklist_contacts_list.json", "no_valid_user",
                "M5iecZ7ddkn7dWmJC_aHJAYkxzRvkWJYEMeW4StS");
            
            Assert.Null(response);
        }
    }
}