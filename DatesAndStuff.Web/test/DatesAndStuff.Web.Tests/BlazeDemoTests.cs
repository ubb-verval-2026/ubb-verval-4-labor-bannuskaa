using System.Text;
using FluentAssertions;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace DatesAndStuff.Web.Tests;

[TestFixture]
public class BlazeDemoTests
{
    private IWebDriver driver;
    private StringBuilder verificationErrors;
    private const string BaseURL = "https://blazedemo.com";
    private const double PriceThreshold = 300.0;

    [SetUp]
    public void SetupTest()
    {
        driver = new ChromeDriver();
        verificationErrors = new StringBuilder();
    }

    [TearDown]
    public void TeardownTest()
    {
        try
        {
            driver.Quit();
            driver.Dispose();
        }
        catch (Exception)
        {
        }
        Assert.That(verificationErrors.ToString(), Is.EqualTo(""));
    }

    [Test]
    public void BlazDemo_MexicoCityToDublin_ShouldHaveAtLeastThreeFlights()
    {
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

        driver.Navigate().GoToUrl(BaseURL);

        var fromSelect = new SelectElement(wait.Until(ExpectedConditions.ElementToBeClickable(By.Name("fromPort"))));
        fromSelect.SelectByText("Mexico City");

        var toSelect = new SelectElement(driver.FindElement(By.Name("toPort")));
        toSelect.SelectByText("Dublin");

        driver.FindElement(By.XPath("//input[@value='Find Flights']")).Click();

        wait.Until(ExpectedConditions.ElementExists(By.XPath("//table/tbody/tr")));
        var flights = driver.FindElements(By.XPath("//table/tbody/tr"));

        flights.Count.Should().BeGreaterThanOrEqualTo(3);

        foreach (var row in flights)
        {
            var priceText = row.FindElement(By.XPath("td[6]")).Text.Replace("$", "").Trim();
            if (double.TryParse(priceText, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double price) && price < PriceThreshold)
            {
                var screenshot = ((ITakesScreenshot)driver).GetScreenshot();
                var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                screenshot.SaveAsFile(Path.Combine(desktopPath, "cheap_flight.png"));
                break;
            }
        }
    }
}
