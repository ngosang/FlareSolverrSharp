using FlareSolverrSharp.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlareSolverrSharp.Tests
{
    [TestClass]
    public class ConcurrencyAdvisorTests
    {
        [TestMethod]
        public void TestEstimateMemoryMb()
        {
            Assert.AreEqual(1250, ConcurrencyAdvisor.EstimateMemoryMb(5));
            Assert.AreEqual(2500, ConcurrencyAdvisor.EstimateMemoryMb(10));
            Assert.AreEqual(250, ConcurrencyAdvisor.EstimateMemoryMb(1));
            Assert.AreEqual(250, ConcurrencyAdvisor.EstimateMemoryMb(0));
        }

        [TestMethod]
        public void TestGetMemoryWarning()
        {
            Assert.IsNull(ConcurrencyAdvisor.GetMemoryWarning(4));
            Assert.IsNull(ConcurrencyAdvisor.GetMemoryWarning(7));

            var warning8 = ConcurrencyAdvisor.GetMemoryWarning(8);
            Assert.IsNotNull(warning8);
            Assert.IsTrue(warning8.Contains("ALERTA DE MEMÓRIA"));
            Assert.IsTrue(warning8.Contains("2000 MB"));

            var warning10 = ConcurrencyAdvisor.GetMemoryWarning(10);
            Assert.IsNotNull(warning10);
            Assert.IsTrue(warning10.Contains("2500 MB"));
        }

        [TestMethod]
        public void TestEvaluateDivergenceJackettBottleneck()
        {
            var msg = ConcurrencyAdvisor.EvaluateDivergence(3, 6);
            Assert.IsNotNull(msg);
            Assert.IsTrue(msg.Contains("limitado pelo Jackett"));
            Assert.IsTrue(msg.Contains("3 conexões"));
            Assert.IsTrue(msg.Contains("6 no Byparr"));
        }

        [TestMethod]
        public void TestEvaluateDivergenceByparrBottleneck()
        {
            var msg = ConcurrencyAdvisor.EvaluateDivergence(10, 5);
            Assert.IsNotNull(msg);
            Assert.IsTrue(msg.Contains("limitado pelo Byparr"));
            Assert.IsTrue(msg.Contains("5 sessões"));
            Assert.IsTrue(msg.Contains("10 no Jackett"));
        }

        [TestMethod]
        public void TestEvaluateDivergenceBalanced()
        {
            var msg = ConcurrencyAdvisor.EvaluateDivergence(5, 5);
            Assert.IsNull(msg);
        }

        [TestMethod]
        public void TestSemaphoreLockerDynamicConcurrency()
        {
            SemaphoreLocker.SetMaxConcurrency(8);
            Assert.AreEqual(8, SemaphoreLocker.MaxConcurrency);
            Assert.AreEqual(2000, SemaphoreLocker.EstimatedMemoryMb);
            Assert.IsNotNull(SemaphoreLocker.MemoryWarning);

            SemaphoreLocker.SetMaxConcurrency(4);
            Assert.AreEqual(4, SemaphoreLocker.MaxConcurrency);
            Assert.AreEqual(1000, SemaphoreLocker.EstimatedMemoryMb);
            Assert.IsNull(SemaphoreLocker.MemoryWarning);
        }
    }
}
