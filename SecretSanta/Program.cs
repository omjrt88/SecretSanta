using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using RestSharp;

namespace SecretSanta
{
    class MainClass
    {
        //private static string sentBtn = "//span[@data-icon='send']";
        //private static string chromeProfile = @"user-data-dir=/Users/orodriguez/Library/Application Support/Google/Chrome/";
        public static void Main(string[] args)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            Console.WriteLine("Secret Santa!");
            var family = LoadJson();
            ComposeMessages(family);
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
                Name = (person.Split('=').FirstOrDefault()).Split('#').FirstOrDefault(), 
                Phone = (person.Split('=').FirstOrDefault()).Split('#').Length > 1
                                                            ? (person.Split('=').FirstOrDefault()).Split('#').LastOrDefault()
                                                            : null,
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
            List<string> amigoSecreto = new List<string>();
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
                amigoSecreto.Add($"{person.Name} => {person.Target}");
            }

            using (StreamWriter file = 
                   new StreamWriter(@"../../SortedFamily.txt"))
            {
                amigoSecreto.ForEach(x => file.WriteLine(x));
            }

            return family;
        }

        private static void ComposeMessages(List<Person> family)
        {
            foreach (var person in family)
            {
                string message = $"Hola nuevamente {person.Name}. Como hasta Santa se equivoca, volvi a sortear el amigo secreto. Para esta navidad, tu amigo secreto es: *{person.Target}*. Jo Jo Jo!";
                string phone = $"506{person.Phone}";
                ConnectToWhatsApp(phone, message);
            }
        }
        /*
        private static void ConnectToWhatsApp(string phone, string msg)
        {
            Console.WriteLine($"Number:{phone}\n Message:{msg}");
            IWebDriver driver;
            var options = new ChromeOptions();
            options.AddArguments(new List<string>() { 
                chromeProfile});
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
                Thread.Sleep(2000);
                button.Click();
            }
            finally
            {
                driver.Close();
            }
        }*/


        private static void ConnectToWhatsApp(string to, string text)
        {
            //Console.WriteLine($"Number:{to}\n Message:{text}");
            var token = "d926c26249e46e7b1d27368c4baee49b5dd402a1d831b";
            var uid = "50688237198";
            var custom_uid = $"omr - {Guid.NewGuid()}";

            var client = new RestClient($"https://www.waboxapp.com/ajax/sandbox/send_chat?strict=1&token={token}&uid={uid}&to={to}&custom_uid={custom_uid}&text={text}&api=1");
            var request = new RestRequest(Method.GET);
            //request.AddHeader("content-type", "application/x-www-form-urlencoded");
            //request.AddParameter("application/x-www-form-urlencoded", $"token=m{token}&uid={uid}&to={to}&custom_uid={custom_uid}&text={text}", ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            Console.WriteLine(response.StatusCode);
        }
    }
}
