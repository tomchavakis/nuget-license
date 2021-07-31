using System;
using System.IO;
using NUnit.Framework;

namespace NugetUtility.Tests {

    [TestFixture]
    public class UtilityTests {

        [Test]
        public void ExtractTextFromHTML_Should_Return_Text_Output_From_HTML_Input () {
            try {
                string input = File.ReadAllText ("data/SPDX.html");
                string text = Utilties.ExtractTextFromHTML (input);
                Assert.NotNull (text);
            } catch (System.Exception ex) {
                Assert.Fail (ex.Message);
            }
        }
    }
}