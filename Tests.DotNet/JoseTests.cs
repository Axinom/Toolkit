using Axinom.Toolkit;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Xunit;

namespace Tests
{
	public sealed class JoseTests : TestClass
	{
		#region Test data
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

		public JoseTests()
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

		static JoseTests()
		{
			_testsAssembly = typeof(EmbeddedPackageTests).GetTypeInfo().Assembly;
		}
		#endregion

		[Fact]
		public void EncryptAndSign_DoesNotCatastrophicallyFail()
		{
			var data = Helpers.Random.GetBytes(123);

			var wrapped = Helpers.Jose.EncryptAndSign(data, _aPublic, _bPrivate);

			// Success? Well, the data can't at least get smaller so let's check that.
			Assert.True(data.Length <= wrapped.Length);

			// And it should contain two dots to be a valid JOSE object.
			Assert.Equal(2, wrapped.Count(c => c == '.'));
		}

		[Theory]
		[InlineData(1)]
		[InlineData(2)]
		[InlineData(8)]
		[InlineData(15)]
		[InlineData(16)]
		[InlineData(17)]
		[InlineData(127)]
		[InlineData(128)]
		[InlineData(100000)]
		[InlineData(1000000)]
		public void RoundTrip_AppearsToWork(int dataLength)
		{
			var data = Helpers.Random.GetBytes(dataLength);

			var wrapped = Helpers.Jose.EncryptAndSign(data, _aPublic, _bPrivate);

			X509Certificate2 signedBy;
			var unwrapped = Helpers.Jose.VerifyAndDecrypt(wrapped, out signedBy, _aPrivate);

			Assert.Equal(data, unwrapped);
			Assert.Equal(_bPrivate.Thumbprint, signedBy.Thumbprint);
		}

		[Fact]
		public void DoubleRoundTrip_AppearsToWork()
		{
			var data = Helpers.Random.GetBytes(56);

			// First encrypt for A and sign by B.
			// Then encrypt for C and sign by B.
			var wrapped1 = Helpers.Jose.EncryptAndSign(data, _aPublic, _bPrivate);
			var wrapped2 = Helpers.Jose.EncryptAndSign(Encoding.UTF8.GetBytes(wrapped1), _cPublic, _bPrivate);

			// First decrypt for C and verify signed by B.
			X509Certificate2 signedBy;
			var unwrapped1 = Helpers.Jose.VerifyAndDecrypt(wrapped2, out signedBy, _cPrivate);

			Assert.Equal(_bPrivate.Thumbprint, signedBy.Thumbprint);

			// Then decrypt for A and verify signed by B.
			var unwrapped2 = Helpers.Jose.VerifyAndDecrypt(Encoding.UTF8.GetString(unwrapped1), out signedBy, _aPrivate);

			Assert.Equal(data, unwrapped2);

			Assert.Equal(_bPrivate.Thumbprint, signedBy.Thumbprint);
		}

		[Fact]
		public void VerifyAndDecrypt_WithNoDecryptionKey_Fails()
		{
			var data = Helpers.Random.GetBytes(89);

			var wrapped = Helpers.Jose.EncryptAndSign(data, _aPublic, _bPrivate);

			X509Certificate2 signedBy;

			Assert.Throws<CryptographicException>(() => Helpers.Jose.VerifyAndDecrypt(wrapped, out signedBy, _cPrivate));
		}

		[Fact]
		public void EncryptAndSign_WithWeakEncryptionKey_Fails()
		{
			var data = Helpers.Random.GetBytes(853);

			Assert.Throws<ArgumentException>(() => Helpers.Jose.EncryptAndSign(data, _weak1Public, _aPrivate));
			Assert.Throws<ArgumentException>(() => Helpers.Jose.EncryptAndSign(data, _weak2Public, _aPrivate));
		}

		[Fact]
		public void EncryptAndSign_WithWeakSigningKey_Fails()
		{
			var data = Helpers.Random.GetBytes(255);

			Assert.Throws<ArgumentException>(() => Helpers.Jose.EncryptAndSign(data, _aPublic, _weak1Private));
			Assert.Throws<ArgumentException>(() => Helpers.Jose.EncryptAndSign(data, _aPublic, _weak2Private));
		}

		[Fact]
		public void VerifyAndDecrypt_WithWeakKeyJustTaggingAlong_Fails()
		{
			var data = Helpers.Random.GetBytes(256);

			var wrapped = Helpers.Jose.EncryptAndSign(data, _aPublic, _bPrivate);

			X509Certificate2 signedBy;

			// We also provide the right key, so the weak key is just tagging along.
			// Still, the expectation is that all the keys are verified for sanity first.

			Assert.Throws<ArgumentException>(() => Helpers.Jose.VerifyAndDecrypt(wrapped, out signedBy, _aPrivate, _weak1Private));
			Assert.Throws<ArgumentException>(() => Helpers.Jose.VerifyAndDecrypt(wrapped, out signedBy, _aPrivate, _weak2Private));
		}

		[Fact]
		public void VerifyAndDecrypt_WithMultipleKeysProvided_UsesCorrectKey()
		{
			var data = Helpers.Random.GetBytes(257);

			var wrapped = Helpers.Jose.EncryptAndSign(data, _bPublic, _cPrivate);

			X509Certificate2 signedBy;
			var unwrapped = Helpers.Jose.VerifyAndDecrypt(wrapped, out signedBy, _aPrivate, _bPrivate, _cPrivate);

			Assert.Equal(data, unwrapped);
		}
	}
}
