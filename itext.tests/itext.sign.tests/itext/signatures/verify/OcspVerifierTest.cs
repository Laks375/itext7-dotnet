/*
This file is part of the iText (R) project.
Copyright (c) 1998-2019 iText Group NV
Authors: iText Software.

This program is free software; you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License version 3
as published by the Free Software Foundation with the addition of the
following permission added to Section 15 as permitted in Section 7(a):
FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY
ITEXT GROUP. ITEXT GROUP DISCLAIMS THE WARRANTY OF NON INFRINGEMENT
OF THIRD PARTY RIGHTS

This program is distributed in the hope that it will be useful, but
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
or FITNESS FOR A PARTICULAR PURPOSE.
See the GNU Affero General Public License for more details.
You should have received a copy of the GNU Affero General Public License
along with this program; if not, see http://www.gnu.org/licenses or write to
the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
Boston, MA, 02110-1301 USA, or download the license from the following URL:
http://itextpdf.com/terms-of-use/

The interactive user interfaces in modified source and object code versions
of this program must display Appropriate Legal Notices, as required under
Section 5 of the GNU Affero General Public License.

In accordance with Section 7(b) of the GNU Affero General Public License,
a covered work must retain the producer line in every PDF that is created
or manipulated using iText.

You can be released from the requirements of the license by purchasing
a commercial license. Buying such a license is mandatory as soon as you
develop commercial activities involving the iText software without
disclosing the source code of your own applications.
These activities include: offering paid services to customers as an ASP,
serving PDFs on the fly in a web application, shipping iText with a closed
source product.

For more information, please contact iText Software Corp. at this
address: sales@itextpdf.com
*/
using System;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Ocsp;
using Org.BouncyCastle.X509;
using iText.IO.Util;
using iText.Signatures;
using iText.Signatures.Testutils;
using iText.Signatures.Testutils.Builder;
using iText.Signatures.Testutils.Client;
using iText.Test;

namespace iText.Signatures.Verify {
    public class OcspVerifierTest : ExtendedITextTest {
        private static readonly String certsSrc = iText.Test.TestUtil.GetParentProjectDirectory(NUnit.Framework.TestContext
            .CurrentContext.TestDirectory) + "/resources/itext/signatures/certs/";

        private static readonly char[] password = "testpass".ToCharArray();

        [NUnit.Framework.OneTimeSetUp]
        public static void Before() {
        }

        /// <exception cref="Org.BouncyCastle.Security.GeneralSecurityException"/>
        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="Org.BouncyCastle.Ocsp.OcspException"/>
        [NUnit.Framework.Test]
        public virtual void ValidOcspTest01() {
            X509Certificate caCert = (X509Certificate)Pkcs12FileHelper.ReadFirstChain(certsSrc + "rootRsa.p12", password
                )[0];
            TestOcspResponseBuilder builder = new TestOcspResponseBuilder(caCert);
            NUnit.Framework.Assert.IsTrue(VerifyTest(builder));
        }

        /// <exception cref="Org.BouncyCastle.Security.GeneralSecurityException"/>
        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="Org.BouncyCastle.Ocsp.OcspException"/>
        [NUnit.Framework.Test]
        public virtual void InvalidRevokedOcspTest01() {
            X509Certificate caCert = (X509Certificate)Pkcs12FileHelper.ReadFirstChain(certsSrc + "rootRsa.p12", password
                )[0];
            TestOcspResponseBuilder builder = new TestOcspResponseBuilder(caCert);
            builder.SetCertificateStatus(new RevokedStatus(DateTimeUtil.GetCurrentUtcTime().AddDays(-20), Org.BouncyCastle.Asn1.X509.CrlReason.KeyCompromise
                ));
            NUnit.Framework.Assert.IsFalse(VerifyTest(builder));
        }

        /// <exception cref="Org.BouncyCastle.Security.GeneralSecurityException"/>
        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="Org.BouncyCastle.Ocsp.OcspException"/>
        [NUnit.Framework.Test]
        public virtual void InvalidUnknownOcspTest01() {
            X509Certificate caCert = (X509Certificate)Pkcs12FileHelper.ReadFirstChain(certsSrc + "rootRsa.p12", password
                )[0];
            TestOcspResponseBuilder builder = new TestOcspResponseBuilder(caCert);
            builder.SetCertificateStatus(new UnknownStatus());
            NUnit.Framework.Assert.IsFalse(VerifyTest(builder));
        }

        /// <exception cref="Org.BouncyCastle.Security.GeneralSecurityException"/>
        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="Org.BouncyCastle.Ocsp.OcspException"/>
        [NUnit.Framework.Test]
        public virtual void InvalidOutdatedOcspTest01() {
            X509Certificate caCert = (X509Certificate)Pkcs12FileHelper.ReadFirstChain(certsSrc + "rootRsa.p12", password
                )[0];
            TestOcspResponseBuilder builder = new TestOcspResponseBuilder(caCert);
            DateTime thisUpdate = DateTimeUtil.GetCurrentTime().AddDays(-30);
            DateTime nextUpdate = DateTimeUtil.GetCurrentTime().AddDays(-15);
            builder.SetThisUpdate(thisUpdate);
            builder.SetNextUpdate(nextUpdate);
            NUnit.Framework.Assert.IsFalse(VerifyTest(builder));
        }

        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="Org.BouncyCastle.Security.GeneralSecurityException"/>
        private bool VerifyTest(TestOcspResponseBuilder builder) {
            String caCertFileName = certsSrc + "rootRsa.p12";
            String checkCertFileName = certsSrc + "signCertRsa01.p12";
            X509Certificate caCert = (X509Certificate)Pkcs12FileHelper.ReadFirstChain(caCertFileName, password)[0];
            ICipherParameters caPrivateKey = Pkcs12FileHelper.ReadFirstKey(caCertFileName, password, password);
            X509Certificate checkCert = (X509Certificate)Pkcs12FileHelper.ReadFirstChain(checkCertFileName, password)[
                0];
            TestOcspClient ocspClient = new TestOcspClient(builder, caPrivateKey);
            byte[] basicOcspRespBytes = ocspClient.GetEncoded(checkCert, caCert, null);
            Asn1Object var2 = Asn1Object.FromByteArray(basicOcspRespBytes);
            BasicOcspResp basicOCSPResp = new BasicOcspResp(BasicOcspResponse.GetInstance(var2));
            OCSPVerifier ocspVerifier = new OCSPVerifier(null, null);
            return ocspVerifier.Verify(basicOCSPResp, checkCert, caCert, DateTimeUtil.GetCurrentUtcTime());
        }
    }
}
