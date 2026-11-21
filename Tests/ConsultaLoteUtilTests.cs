using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConsultaCEPs.Tests
{
    [TestClass]
    public class ConsultaLoteUtilTests
    {
        [TestMethod]
        public void ExtrairCepsValidos_NormalizaMascaraEDescartaInvalidos()
        {
            var ceps = ConsultaLoteUtil.ExtrairCepsValidos(
                new[] { "01311-921", "01001-000", "1234", "abcdefgh" }, 100);

            CollectionAssert.AreEqual(new[] { "01311921", "01001000" }, ceps);
        }

        [TestMethod]
        public void ExtrairCepsValidos_RemoveDuplicados()
        {
            var ceps = ConsultaLoteUtil.ExtrairCepsValidos(
                new[] { "01311-921", "01311921", "01311 921" }, 100);

            Assert.AreEqual(1, ceps.Count);
            Assert.AreEqual("01311921", ceps[0]);
        }

        [TestMethod]
        public void ExtrairCepsValidos_RespeitaLimite()
        {
            var linhas = Enumerable.Range(0, 105)
                .Select(indice => (10000000 + indice).ToString());

            var ceps = ConsultaLoteUtil.ExtrairCepsValidos(linhas, 100);

            Assert.AreEqual(100, ceps.Count);
            Assert.AreEqual("10000000", ceps[0]);
            Assert.AreEqual("10000099", ceps[99]);
        }

        [TestMethod]
        public void ExtrairCepsValidos_EntradaNulaOuLimiteInvalidoRetornaVazia()
        {
            Assert.AreEqual(0, ConsultaLoteUtil.ExtrairCepsValidos(null, 100).Count);
            Assert.AreEqual(0, ConsultaLoteUtil.ExtrairCepsValidos(new[] { "01001000" }, 0).Count);
        }
    }
}
