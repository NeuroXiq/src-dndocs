using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.IntegrationTests.Tests
{
    [TestFixture]
    internal class HomeController
    {
        [Test]
        public void FullCreateNugetProject_Success() { throw new Exception();  }

        [Test]
        public void CreateNugetProject_FailOnDuplicated() { throw new Exception(); }

        [Test]
        public void WillCreateMultipleParallelProjects() { throw new Exception(); }

        [Test]
        public void WillReturnImmediatelyIfProjectAlreadyExists() { throw new Exception(); }
    }
}
