using BBBSBackend.DBModels;

namespace BBBSTests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
            Console.WriteLine("Setting up tests...");
        }

        [Test]
        public void AssertTrue()
        {
            Assert.IsTrue(true);
        }
    }
}