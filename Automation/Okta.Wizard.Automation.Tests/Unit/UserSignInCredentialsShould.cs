// <copyright file="UserSignInCredentialsShould.cs" company="Okta, Inc">
// Copyright (c) 2020 - present Okta, Inc. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.
// </copyright>

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Okta.Wizard.Internal;
using System;
using System.IO;

namespace Okta.Wizard.Automation.Tests.Unit
{
    [TestClass]
    public class UserSignInCredentialsShould
    {
        [TestMethod]
        public void SerializeEncrypted()
        {
            UserSignInCredentials userSignInCredentials = new UserSignInCredentials
            {
                SignInUrl = TestData.SignInUrl,
                UserName = TestData.UserName,
                Password = TestData.Password
            };
            FileInfo testCredentialsFile = new FileInfo($"./{nameof(UserSignInCredentials)}_{nameof(SerializeEncrypted)}_Test");
            userSignInCredentials.Save(testCredentialsFile.FullName);

            string fileContent = File.ReadAllText(testCredentialsFile.FullName);
            Console.WriteLine(fileContent);
            
            Assert.ThrowsException<JsonReaderException>(() => Deserialize.FromJson<UserSignInCredentials>(fileContent));

            SecureSerializable secureSerializable = new SecureSerializable();
            UserSignInCredentials reHydrated = secureSerializable.FromEncryptedJsonFile<UserSignInCredentials>(testCredentialsFile.FullName);
            reHydrated.SignInUrl.Should().Be(TestData.SignInUrl);
            reHydrated.UserName.Should().Be(TestData.UserName);
            reHydrated.Password.Should().Be(TestData.Password);
        }

        [TestMethod]
        public void SaveToAndLoadFromDefaultFilePath()
        {
            if(File.Exists(UserSignInCredentials.DefaultFilePath))
            {
                File.Delete(UserSignInCredentials.DefaultFilePath);
            }
            string testUserName = $"{TestData.UserName}_{nameof(SaveToAndLoadFromDefaultFilePath)}_Test";
            File.Exists(UserSignInCredentials.DefaultFilePath).Should().BeFalse();
            UserSignInCredentials userSignInCredentials = new UserSignInCredentials
            {
                SignInUrl = TestData.SignInUrl,
                UserName = testUserName,
                Password = TestData.Password
            };
            userSignInCredentials.Save();
            File.Exists(UserSignInCredentials.DefaultFilePath).Should().BeTrue();
            string fileContent = File.ReadAllText(UserSignInCredentials.DefaultFilePath);
            Assert.ThrowsException<JsonReaderException>(() => Deserialize.FromJson<UserSignInCredentials>(fileContent));
            
            UserSignInCredentials reHydrated = UserSignInCredentials.Load();
            reHydrated.SignInUrl.Should().Be(userSignInCredentials.SignInUrl);
            reHydrated.UserName.Should().Be(testUserName);
            reHydrated.Password.Should().Be(userSignInCredentials.Password);
        }

        [TestMethod]
        public void SaveToDefaultFileFromEnvironmentVariables()
        {
            if (File.Exists(UserSignInCredentials.DefaultFilePath))
            {
                File.Delete(UserSignInCredentials.DefaultFilePath);
            }
            UserSignInCredentials userSignInCredentials = UserSignInCredentials.FromEnvironmentVariables();
            Console.WriteLine(userSignInCredentials.ToJson(true));
            userSignInCredentials.Save();
            File.Exists(UserSignInCredentials.DefaultFilePath).Should().BeTrue();
            string fileContent = File.ReadAllText(UserSignInCredentials.DefaultFilePath);
            Assert.ThrowsException<JsonReaderException>(() => Deserialize.FromJson<UserSignInCredentials>(fileContent));

            UserSignInCredentials reHydrated = UserSignInCredentials.Load();
            reHydrated.SignInUrl.Should().Be(userSignInCredentials.SignInUrl);
            reHydrated.UserName.Should().Be(userSignInCredentials.UserName);
            reHydrated.Password.Should().Be(userSignInCredentials.Password);

            Console.WriteLine(reHydrated.ToJson(true));
        }

        [TestMethod]
        public void ReturnNullOnInvalidFile()
        {
            string text = "Some non json text that isn't encrypted";
            FileInfo testFile = new FileInfo($"./{nameof(ReturnNullOnInvalidFile)}_Test.txt");
            File.WriteAllText(testFile.FullName, text);
            UserSignInCredentials userSignInCredentials = UserSignInCredentials.TryLoad(testFile.FullName);
            userSignInCredentials.Should().BeNull();
        }
    }
}
