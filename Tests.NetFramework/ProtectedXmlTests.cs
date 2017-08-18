using Axinom.Toolkit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml;

namespace Tests
{
    [TestClass]
    public sealed class ProtectedXmlTests : TestClass
    {
        #region Test data
        // Just some arbitrary XML document to play around with.
        private static string _xml;

        // Test certificates generated with the following command:
        // makecert.exe -pe -n CN=A -a sha512 -sky exchange -r -ss My

        private X509Certificate2 _aPublic;
        private X509Certificate2 _aPrivate;
        private X509Certificate2 _bPublic;
        private X509Certificate2 _bPrivate;
        private X509Certificate2 _cPublic;
        private X509Certificate2 _cPrivate;

        private X509Certificate2 _weak1Public;
        private X509Certificate2 _weak1Private;

        private X509Certificate2 _weak2Public;
        private X509Certificate2 _weak2Private;

        public ProtectedXmlTests()
        {
            using (var package = new EmbeddedPackage(_testsAssembly, "Tests.TestData.Certificates", "A.cer", "A.pfx", "B.cer", "B.pfx", "C.cer", "C.pfx", "Weak-Sha1.cer", "Weak-Sha1.pfx", "Weak-SmallKey.cer", "Weak-SmallKey.pfx"))
            {
                _aPublic = new X509Certificate2(Path.Combine(package.Path, "A.cer"));
                _bPublic = new X509Certificate2(Path.Combine(package.Path, "B.cer"));
                _cPublic = new X509Certificate2(Path.Combine(package.Path, "C.cer"));
                _weak1Public = new X509Certificate2(Path.Combine(package.Path, "Weak-Sha1.cer"));
                _weak2Public = new X509Certificate2(Path.Combine(package.Path, "Weak-SmallKey.cer"));

                _aPrivate = new X509Certificate2(Path.Combine(package.Path, "A.pfx"), "A");
                _bPrivate = new X509Certificate2(Path.Combine(package.Path, "B.pfx"), "B");
                _cPrivate = new X509Certificate2(Path.Combine(package.Path, "C.pfx"), "C");
                _weak1Private = new X509Certificate2(Path.Combine(package.Path, "Weak-Sha1.pfx"), "Weak-Sha1");
                _weak2Private = new X509Certificate2(Path.Combine(package.Path, "Weak-SmallKey.pfx"), "Weak-SmallKey");
            }
        }

        public override void Dispose()
        {
            _aPrivate.Dispose();
            _aPublic.Dispose();
            _aPrivate.Dispose();
            _aPublic.Dispose();
            _aPrivate.Dispose();
            _aPublic.Dispose();
            _weak1Private.Dispose();
            _weak1Public.Dispose();
            _weak2Private.Dispose();
            _weak2Public.Dispose();

            base.Dispose();
        }

        private static readonly Assembly _testsAssembly;

        static ProtectedXmlTests()
        {
            _testsAssembly = typeof(EmbeddedPackageTests).GetTypeInfo().Assembly;

            using (var package = new EmbeddedPackage(_testsAssembly, "Tests.TestData", "Gangster.xml"))
            {
                // We pass it through XmlDocument first to get "XmlDocument-style" formatting, so we can string compare later.
                var doc = new XmlDocument();
                doc.Load(Path.Combine(package.Path, "Gangster.xml"));
                _xml = doc.OuterXml;
            }
        }
        #endregion

        private static XmlDocument LoadDocument()
        {
            var document = new XmlDocument();
            document.LoadXml(_xml);

            return document;
        }

        [TestMethod]
        public void EncryptAndSign_AppearsToEncryptAndSign()
        {
            var document = LoadDocument();

            Helpers.ProtectedXml.EncryptAndSign(document, _aPublic, _bPrivate);

            // It looks encrypted.
            Assert.AreEqual("EncryptedData", document.DocumentElement.LocalName);

            // And it looks signed.
            Assert.AreEqual(1, document.DocumentElement.ChildNodes.Cast<XmlNode>().Count(n => n.LocalName == "Signature"));
        }

        [TestMethod]
        public void RoundTrip_AppearsToWork()
        {
            var document = LoadDocument();

            Helpers.ProtectedXml.EncryptAndSign(document, _aPublic, _bPrivate);

            X509Certificate2 signedBy;
            Helpers.ProtectedXml.VerifyAndDecrypt(document, out signedBy, _aPrivate);

            var xml = document.DocumentElement.OuterXml;
            Assert.AreEqual(_xml, xml);

            Assert.AreEqual(_bPrivate.Thumbprint, signedBy.Thumbprint);
        }

        [TestMethod]
        public void DoubleRoundTrip_AppearsToWork()
        {
            var document = LoadDocument();

            // First encrypt for A and sign by B.
            // Then encrypt for C and sign by B.
            Helpers.ProtectedXml.EncryptAndSign(document, _aPublic, _bPrivate);
            Helpers.ProtectedXml.EncryptAndSign(document, _cPublic, _bPrivate);

            // First decrypt for C and verify signed by B.
            X509Certificate2 signedBy;
            Helpers.ProtectedXml.VerifyAndDecrypt(document, out signedBy, _cPrivate);

            Assert.AreEqual(_bPrivate.Thumbprint, signedBy.Thumbprint);

            // Then decrypt for A and verify signed by B.
            Helpers.ProtectedXml.VerifyAndDecrypt(document, out signedBy, _aPrivate);

            var xml = document.DocumentElement.OuterXml;
            Assert.AreEqual(_xml, xml);

            Assert.AreEqual(_bPrivate.Thumbprint, signedBy.Thumbprint);
        }

        [TestMethod]
        public void VerifyAndDecrypt_WithNoDecryptionKey_Fails()
        {
            var document = LoadDocument();

            Helpers.ProtectedXml.EncryptAndSign(document, _aPublic, _bPrivate);

            X509Certificate2 signedBy;

            Assert.ThrowsException<CryptographicException>(() => Helpers.ProtectedXml.VerifyAndDecrypt(document, out signedBy, _cPrivate));
        }

        [TestMethod]
        public void EncryptAndSign_WithWeakEncryptionKey_Fails()
        {
            var document = LoadDocument();

            Assert.ThrowsException<ArgumentException>(() => Helpers.ProtectedXml.EncryptAndSign(document, _weak1Public, _aPrivate));

            document = LoadDocument();

            Assert.ThrowsException<ArgumentException>(() => Helpers.ProtectedXml.EncryptAndSign(document, _weak2Public, _aPrivate));
        }

        [TestMethod]
        public void EncryptAndSign_WithWeakSigningKey_Fails()
        {
            var document = LoadDocument();

            Assert.ThrowsException<ArgumentException>(() => Helpers.ProtectedXml.EncryptAndSign(document, _aPublic, _weak1Private));

            document = LoadDocument();

            Assert.ThrowsException<ArgumentException>(() => Helpers.ProtectedXml.EncryptAndSign(document, _aPublic, _weak2Private));
        }

        [TestMethod]
        public void VerifyAndDecrypt_WithWeakKeyJustTaggingAlong_Fails()
        {
            var document = LoadDocument();

            Helpers.ProtectedXml.EncryptAndSign(document, _aPublic, _bPrivate);

            X509Certificate2 signedBy;

            // We also provide the right key, so the weak key is just tagging along.
            // Still, the expectation is that all the keys are verified for sanity first.

            Assert.ThrowsException<ArgumentException>(() => Helpers.ProtectedXml.VerifyAndDecrypt(document, out signedBy, _aPrivate, _weak1Private));

            Assert.ThrowsException<ArgumentException>(() => Helpers.ProtectedXml.VerifyAndDecrypt(document, out signedBy, _aPrivate, _weak2Private));
        }

        [TestMethod]
        public void VerifyAndDecrypt_WithMultipleKeysProvided_UsesCorrectKey()
        {
            var document = LoadDocument();

            Helpers.ProtectedXml.EncryptAndSign(document, _bPublic, _cPrivate);

            X509Certificate2 signedBy;
            Helpers.ProtectedXml.VerifyAndDecrypt(document, out signedBy, _aPrivate, _bPrivate, _cPrivate);

            var xml = document.DocumentElement.OuterXml;
            Assert.AreEqual(_xml, xml);
        }
    }
}
