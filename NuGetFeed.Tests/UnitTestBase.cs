using Moq;

using NUnit.Framework;

using Ploeh.AutoFixture;

namespace NuGetFeed.Tests
{
    [TestFixture]
    [Category("Unit tests")]
    public class UnitTestBase
    {
        private Fixture _fixture;

        protected Fixture Fixture
        {
            get
            {
                return _fixture;
            }
        }

        [SetUp]
        public void SetUpFixture()
        {
            _fixture = new Fixture();
            _fixture.Inject('c');
        }

        protected Mock<T> Mock<T>() where T : class
        {
            return new Mock<T>();
        }

        protected T A<T>()
        {
            return _fixture.CreateAnonymous<T>();
        }
    }
}