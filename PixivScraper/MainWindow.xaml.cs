using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Net.Http;
using static System.Net.Mime.MediaTypeNames;
using System.Windows.Documents.DocumentStructures;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.DevTools.V123.FedCm;
using System.IO;
using System.Net;  
using System.Runtime.InteropServices;
using System.Printing;


namespace PixivScraper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    // Additional: qol improvement so program automatically goes to illustrations page if user inputs profile or artwork link
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        
        private void scrapeButton_Click(object sender, RoutedEventArgs e)
        {
            string url = linkTextBox.Text.Trim();

            if (validateURL(url, out _, logRichBox) == true)
            {
                var options = new ChromeOptions();
                options.AddArgument("--headless=new");
                var driver = new ChromeDriver();

                driver.Navigate().GoToUrl(url);
                logRichBox.AppendText(Environment.NewLine + "Valid Url. Going to: " + url);

                scrapeLoop(driver, url, logRichBox);
                //test(driver, url, logRichBox);
            }
        }

        public static void login(ChromeDriver driver, string input, string type)
        {
            // LOGIN put in mainpage for implementation
            //string mainpage = "https://www.pixiv.net/en/";
            //string username = usernameTextBox.Text.Trim();
            //string password = passwordTextBox.Text.Trim();
            //driver.Navigate().GoToUrl(mainpage);
            //logRichBox.AppendText(Environment.NewLine + "Logging in PIXIV account...");
            //IWebElement loginForm = driver.FindElement(By.XPath("//a[@class='signup-form__submit--login']"));
            //wait.Until(d => loginForm.Displayed);
            //loginForm.Click();
            //login(driver, username, "text");
            //login(driver, password, "password");
            //IWebElement loginButton = driver.FindElement(By.CssSelector(".sc-aXZVg.fSnEpf.sc-eqUAAy.hhGKQA.sc-2o1uwj-10.ldVSLT.sc-2o1uwj-10.ldVSLT"));
            //wait.Until(d => loginButton.Enabled);
            //loginButton.Click();

            string xpathString = String.Format("//input[@type='{0}']", type);
            IWebElement textBox = driver.FindElement(By.XPath(xpathString));
            textBox.Clear();
            textBox.SendKeys(input);
        }

        private static void scrapeLoop(ChromeDriver driver, string url, RichTextBox logRichBox)
        {
            // Test user w/ illust: https://www.pixiv.net/en/users/8024586/illustrations
            // Test user w/ multi-paged illust: https://www.pixiv.net/en/users/16183476
            // Test user w/o illust: https://www.pixiv.net/en/users/35244745
            // Test user w/ private illust: https://www.pixiv.net/en/users/72477613/illustrations
            // Click illust post, click image, download image, go back to illust page, repeat until done

            // Find all illust entries
            string illustrationSelector = ".sc-iasfms-5.liyNwX";
            string showButtonSelector = ".sc-emr523-0.guczbC";
            string imageSelectors = ".sc-1qpw8k9-3.ilIMcK.gtm-expand-full-size-illust";


            IList<IWebElement> illustrationsElement = driver.FindElements(By.CssSelector(illustrationSelector));
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            if (illustrationsElement.Count > 0)
            {
                logRichBox.AppendText(Environment.NewLine + illustrationsElement.Count);

                for (int i=0; i< illustrationsElement.Count; ++i) // i<1
                {
                    // Redeclare to refresh element to avoid stale error
                    IList<IWebElement> illustrations = driver.FindElements(By.CssSelector(illustrationSelector));
                    if (illustrations.Count > 0)
                    {
                        clickLoop(illustrations, wait, i);
                    }
                    else
                    {
                        logRichBox.AppendText(Environment.NewLine + "singleEntry was empty. Either driver wasn't in illust page, or illust page didn't load properly. Entry Count: " + illustrations.Count);
                        driver.Quit();
                    }

                    Task.Delay(1000).Wait();
                    logRichBox.AppendText(Environment.NewLine + driver.Title);

                    // Check entry if it has multiple images
                    IList<IWebElement> showElement = driver.FindElements(By.CssSelector(showButtonSelector));
                    if (showElement.Count > 0)
                    {
                        clickCssSelector(driver, wait, showButtonSelector);
                    }

                    Task.Delay(1000).Wait();

                    // Find all instances of the image
                    IList<IWebElement> images = driver.FindElements(By.CssSelector(imageSelectors));
                    for (int j = 0; j < images.Count; ++j)
                    {
                        clickLoop(images, wait, j);
                        Task.Delay(1000).Wait();
                    }

                    // Download Image
                    //SaveImageUsingJS(driver, imageSelectors);

                    // Go back to main url
                    driver.Navigate().GoToUrl(url);
                }
                driver.Quit();
            }
            else {
                logRichBox.AppendText(Environment.NewLine + "User does not have any posts.");
                driver.Quit();
            }
        }

        // Check url if valid using Uri
        private static bool validateURL(string url, out Uri? uri, RichTextBox logRichBox)
        {
            string domain = "pixiv";
            string userPath = "users";
            var valid = Uri.TryCreate(url, UriKind.Absolute, out uri);

            if (url.ToLower().Contains(domain) == true && url.ToLower().Contains(userPath) == true)
            {
                return valid;
            }
            else
            {
                logRichBox.AppendText(Environment.NewLine + "Url is invalid or does not direct to a PIXIV profile.");
            }

            return false;
        }

        private static void clickLoop(IList<IWebElement> list, WebDriverWait wait, int i)
        {
            IWebElement element = list[i];
            wait.Until(d => list[i].Displayed);
            element.Click();
        }

        private static void clickCssSelector(ChromeDriver driver, WebDriverWait wait, string selector)
        {
            IWebElement webElement = driver.FindElement(By.CssSelector(selector));
            wait.Until(d => webElement.Displayed);
            webElement.Click();
        }





         
        private void quitButton_Click(object sender, RoutedEventArgs e)
        {
            string test = "https://en.wikipedia.org/wiki/White#/media/File:Delphinapterus_leucas_2.jpg";
            string test2 = "https://i.pximg.net/img-original/img/2024/06/01/06/00/08/119232545_p0.jpg/";
            string path = "C:\\Users\\tile\\Downloads\\WHAT.jpg";
        }

        public static void SaveImageUsingJS(IWebDriver driver, string imageSelector) //async Task
        {
            IWebElement imageElement = driver.FindElement(By.CssSelector(imageSelector)); // Replace with your selector

            string script = @"
                var img = arguments[0];
                var a = document.createElement('a');
                a.href = img.src;
                a.download = 'downloaded_image.jpg'; // Modify as needed for filename
                a.click();";

            ((IJavaScriptExecutor)driver).ExecuteScript(script, imageElement);
        }

    }
}