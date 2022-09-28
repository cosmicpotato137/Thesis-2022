using System.Web;
using NUnit.Framework;

namespace UnityTests
{
    [TestFixture]
    public class HttpUtilityTests
    {
        [Test]
        public void HtmlDecodeWorks()
        {
            var x = HttpUtility.HtmlDecode("foo&#32;bar");
            Assert.AreEqual("foo bar", x);
        }

        [Test]
        public void UrlDecodeWorks()
        {
            var x = HttpUtility.UrlDecode("foo%20bar");
            Assert.AreEqual("foo bar", x);
        }
    }
}
