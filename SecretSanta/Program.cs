using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;

namespace SecretSanta
{
    class MainClass
    {
        private static string sentBtn = "//span[@data-icon='send']";
        public static void Main(string[] args)
        {
            Console.WriteLine("Secret Santa!");
            //LoadJson();
            ConnectToWhatsApp("50688237198", "Secret Santa");
        }


        public static List<Person> LoadJson()
        {
            using (StreamReader r = new StreamReader("../../Family.txt"))
            {
                string json = r.ReadToEnd();
                List<Person> family = GetFamilyPeople(json.Split(','));
                return SortSecretSanta(family);
            }
        }

        private static List<Person> GetFamilyPeople(string[] family)
        {

            return family.Select(person => new Person() 
            { 
                Name = person.Split('=').FirstOrDefault(), 
                Exclusions = person.Split('=').Length > 1 
                                   ? GetExclusions(person.Split('=').LastOrDefault()) 
                                   : null 
            }).ToList();
        }

        private static List<string> GetExclusions(string exclusionList) 
        {
            return exclusionList.Split('-').ToList();
        }

        private static List<Person> SortSecretSanta(List<Person> family)
        {
            foreach (Person person in family)
            {
                var allowed = family.Where(x => x.Name != person.Name && !x.Targeted).ToList();
                if (person.Exclusions?.Count > 0)
                {
                    allowed = allowed.Where(x => !person.Exclusions.Contains(x.Name)).ToList();
                }
                
                var target = allowed.OrderBy(emp => Guid.NewGuid()).ToList().First();
                person.Target = target.Name;
                target.Targeted = true;
                Console.WriteLine($"{person.Name} => {person.Target}");
            }

            return family;
        }

        private static void ConnectToWhatsApp(string phone, string msg)
        {
            IWebDriver driver;
            var options = new ChromeOptions();
            options.AddArguments(@"user-data-dir=/Users/orodriguez/Library/Application Support/Google/Chrome/");
            driver = new ChromeDriver(options);
            try
            {
                driver.Url = $"https://web.whatsapp.com/send?phone={phone}&text={msg}&source&data";

                By locator = By.XPath(sentBtn);
                WebDriverWait wait = new WebDriverWait(driver,
                   System.TimeSpan.FromSeconds(15));
                var button = wait.Until(d => 
                           d.FindElement(locator));
                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.XPath(sentBtn)));
                button.Click();
            }
            finally
            {
                driver.Close();
            }
        }

        private static void SendMessage()
        {
            string yourId = "BL1udPQscUCKUg6VQ7vOBG9tanJ0ODhfYXRfZ21haWxfZG90X2NvbQ==";
            string yourMobile = "+50687835836";
            string yourMessage = "Probando Secret Santa desde c#. Por cierto, te amo preciosa! :*";

            try
            {
                string url = "https://NiceApi.net/API";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.Headers.Add("X-APIId", yourId);
                request.Headers.Add("X-APIMobile", yourMobile);
                using (StreamWriter streamOut = new StreamWriter(request.GetRequestStream()))
                {
                    streamOut.Write(yourMessage);
                }
                using (StreamReader streamIn = new StreamReader(request.GetResponse().GetResponseStream()))
                {
                    Console.WriteLine(streamIn.ReadToEnd());
                }
            }
            catch (SystemException se)
            {
                Console.WriteLine(se.Message);
            }
        }
    }
}
