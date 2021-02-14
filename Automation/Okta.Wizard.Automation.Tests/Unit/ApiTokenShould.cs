// <copyright file="ApiTokenShould.cs" company="Okta, Inc">
// Copyright (c) 2020 - present Okta, Inc. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.
// </copyright>

using DevEx;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Okta.Wizard.Internal;
using System;
using System.IO;

namespace Okta.Wizard.Automation.Tests.Unit
{
    [TestClass]
    public class ApiTokenShould
    {
        [TestMethod]
        public void SerializeEncrypted()
        {
            string testTokenName = $"{nameof(SerializeEncrypted)}_Test";
            string testTokenValue = "test api token value";
            ApiToken testApiToken = new ApiToken() { Name = testTokenName, Value = testTokenValue };
            string json = Serialize.ToJson(testApiToken);
            FileInfo testFile = new FileInfo($"./{nameof(SerializeEncrypted)}_Test.json");
            testApiToken.ToJsonFile(testFile.FullName);

            string fileContent = File.ReadAllText(testFile.FullName);
            Console.WriteLine(json);
            Console.WriteLine(fileContent);
            fileContent.Should().NotBe(json);
            Assert.ThrowsException<JsonReaderException>(() => Deserialize.FromJson<ApiToken>(fileContent));

            SecureSerializable secureSerializable = new SecureSerializable();
            ApiToken reHydrated = secureSerializable.FromEncryptedJsonFile<ApiToken>(testFile.FullName);
            reHydrated.Name.Should().Be(testTokenName);
            reHydrated.Value.Should().Be(testTokenValue);
        }

        [TestMethod]
        public void SaveToDirectory()
        {
            string testTokenName = "test token";
            string testTokenValue = "test value";
            string tmpPath = $"{Environment.GetEnvironmentVariable("TMP") ?? "/tmp"}";
            DirectoryInfo tmpDir = new DirectoryInfo(tmpPath);

            string expectedFilePath = Path.Combine(tmpDir.FullName, ApiToken.DefaultFileName);
            if (File.Exists(expectedFilePath))
            {
                File.Delete(expectedFilePath);
            }
            try
            {
                ApiToken testApiToken = new ApiToken { Name = testTokenName, Value = testTokenValue };
                testApiToken.Save(tmpDir.FullName);
                File.Exists(Path.Combine(tmpDir.FullName, ApiToken.DefaultFileName)).Should().BeTrue();

                ApiToken reHydrated = ApiToken.Load(tmpDir.FullName);
                reHydrated.Name.Should().Be(testTokenName);
                reHydrated.Value.Should().Be(testTokenValue);
            }
            finally
            {
                if (File.Exists(expectedFilePath))
                {
                    File.Delete(expectedFilePath);
                }
            }
        }
    }
}
