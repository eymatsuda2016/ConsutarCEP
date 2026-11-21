using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConsultaCEPs.Tests
{
    [TestClass]
    public class CepUtilTests
    {
        [TestMethod]
        public void SomenteNumeros_RemoveMascaraELetras()
        {
            Assert.AreEqual("01311921", ConsultaCEPs.CepUtil.SomenteNumeros("01311-921 abc"));
        }

        [TestMethod]
        public void EhValido_AceitaOitoDigitosComOuSemMascara()
        {
            Assert.IsTrue(ConsultaCEPs.CepUtil.EhValido("01311921"));
            Assert.IsTrue(ConsultaCEPs.CepUtil.EhValido("01311-921"));
        }

        [TestMethod]
        public void EhValido_RejeitaQuantidadeIncorreta()
        {
            Assert.IsFalse(ConsultaCEPs.CepUtil.EhValido("0131192"));
            Assert.IsFalse(ConsultaCEPs.CepUtil.EhValido("013119210"));
            Assert.IsFalse(ConsultaCEPs.CepUtil.EhValido(null));
        }

        [TestMethod]
        public void Formatar_AplicaMascaraNoCepValido()
        {
            Assert.AreEqual("01311-921", ConsultaCEPs.CepUtil.Formatar("01311921"));
        }

        [TestMethod]
        public void Formatar_NaoAlteraValorInvalido()
        {
            Assert.AreEqual("1234", ConsultaCEPs.CepUtil.Formatar("1234"));
        }
    }
}
