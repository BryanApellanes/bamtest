// <copyright file="OktaApiTokenShould.cs" company="Okta, Inc">
// Copyright (c) 2020 - present Okta, Inc. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.
// </copyright>

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Okta.Wizard.Automation.Okta;
using Okta.Wizard.Internal;
using System;
using System.IO;
using FluentAssertions;

namespace Okta.Wizard.Automation.Tests.Unit
{
    [TestClass]
    public class OktaApiTokenShould
    {
        [TestMethod]
        public void SerializeEncrypted()
        {
            OktaApiToken oktaApiToken = new OktaApiToken("TestToken") { Value = "TestTokenValue", SignInUrl = "TestSignInUrl" };
            FileInfo testOktaApiTokenFile = new FileInfo($"./{nameof(OktaApiToken)}_{nameof(SerializeEncrypted)}_Test");
            oktaApiToken.Save(testOktaApiTokenFile.FullName);

            string fileContent = File.ReadAllText(testOktaApiTokenFile.FullName);
            Console.WriteLine(fileContent);
            Console.WriteLine(oktaApiToken.ToJson(true));

            Assert.ThrowsException<JsonReaderException>(() => Deserialize.FromJson<OktaApiToken>(fileContent));

            SecureSerializable secureSerializable = new SecureSerializable();
            OktaApiToken reHydrated = secureSerializable.FromEncryptedJsonFile<OktaApiToken>(testOktaApiTokenFile.FullName);
            reHydrated.SignInUrl.Should().Be(oktaApiToken.SignInUrl);
            reHydrated.Name.Should().Be(oktaApiToken.Name);
            reHydrated.Value.Should().Be(oktaApiToken.Value);            
        }

        [TestMethod]
        public void SaveToAndLoadFromDefaultFilePath()
        {
            if(File.Exists(OktaApiToken.DefaultFilePath))
            {
                File.Delete(OktaApiToken.DefaultFilePath);
            }
            File.Exists(OktaApiToken.DefaultFilePath).Should().BeFalse();
            OktaApiToken oktaApiToken = new OktaApiToken("TestToken") { Value = "TestTokenValue", SignInUrl = "TestSignInUrl" };
            oktaApiToken.Save();
            File.Exists(OktaApiToken.DefaultFilePath).Should().BeTrue();
            string fileContent = File.ReadAllText(OktaApiToken.DefaultFilePath);
            Assert.ThrowsException<JsonReaderException>(() => Deserialize.FromJson<OktaApiToken>(fileContent));

            OktaApiToken reHydrated = OktaApiToken.Load();
            reHydrated.Name.Should().Be(oktaApiToken.Name);
            reHydrated.SignInUrl.Should().Be(oktaApiToken.SignInUrl);
            reHydrated.Value.Should().Be(oktaApiToken.Value);
        }

        [TestMethod]
        public void ReturnNullOnInvalidFile()
        {
            string text = "Some non json text that isn't encrypted";
            FileInfo testFile = new FileInfo($"./{nameof(ReturnNullOnInvalidFile)}_Test.txt");
            File.WriteAllText(testFile.FullName, text);
            OktaApiToken oktaApiToken = OktaApiToken.TryLoad(testFile.FullName);
            oktaApiToken.Should().BeNull();
        }
    }
}
