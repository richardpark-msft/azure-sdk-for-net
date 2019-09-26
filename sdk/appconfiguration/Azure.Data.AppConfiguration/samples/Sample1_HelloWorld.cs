// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Azure.Data.AppConfiguration.Samples
{
    //[LiveOnly]
    public partial class ConfigurationSamples2
    {
        [Test]
        public void DoRevisionTest()
        {
            // the name isn't important, just want it to be unique per run
            string key = $"TestRevisionsTest-{Environment.TickCount}";

            // generate 200 revisions quickly
            string connectionString = File.ReadAllText(@"C:\dev\temp\.env").Split(new[] { '=' }, 2)[1];
            var client = new ConfigurationClient(connectionString);

            // store off those etags - they should be unique if the values are unique.
            var etagToValue = new Dictionary<string, string>();
            var addResp = client.Add(new ConfigurationSetting(key, "Value for 0"));

            ConfigurationSetting currentSetting = addResp.Value;

            // sanity check that no etags are being duplicated as we set these new values
            etagToValue.Add(addResp.Value.ETag.ToString(), addResp.Value.Value);

            for (int i = 1; i < 200; ++i)
            {
                // now let's generate 199 extra revisions
                currentSetting.Value = $"Value for {i}";
                var updateResp = client.Set(currentSetting);

                Assert.AreEqual(200, updateResp.GetRawResponse().Status);
                Assert.AreEqual(currentSetting.Value, updateResp.Value.Value);
                etagToValue.Add(updateResp.Value.ETag.ToString(), updateResp.Value.Value);

                currentSetting = updateResp.Value;
            }

            var revisions = client.GetRevisions(new SettingSelector(key)).ToArray();

            // this seems to not work quite right a few ways:
            // 1. For some reason the # of entries isn't 200 (I'm getting < 200 currently but
            //    I haven't clocked it to see if it's consistent)
            Assert.AreEqual(200, revisions.Length);

            // 2. The returned etags are duplicated - note that I tracked the insert's above with
            //     the dictionary so I should know I have 200 distinct etags.
            var distinctETags = revisions.Select(rev => rev.Value.ETag.ToString()).Distinct();
            Assert.AreEqual(200, distinctETags.Count());

            // 3. The values aren't unique either (but should have been)
            var distinctValues = revisions.Select(rev => rev.Value.Value).Distinct();
            Assert.AreEqual(200, distinctValues.Count());
        }

        [Test]
        public void HelloWorld()
        {
            // Retrieve the connection string from the configuration store.
            // You can get the string from your Azure portal.
            var connectionString = Environment.GetEnvironmentVariable("APPCONFIGURATION_CONNECTION_STRING");

            // Instantiate a client that will be used to call the service.
            var client = new ConfigurationClient(connectionString);

            // Create a Configuration Setting to be stored in the Configuration Store.
            var setting = new ConfigurationSetting("some_key", "some_value");

            // There are two ways to store a Configuration Setting:
            //   -AddAsync creates a setting only if the setting does not already exist in the store.
            //   -SetAsync creates a setting if it doesn't exist or overrides an existing setting
            client.Set(setting);

            // Retrieve a previously stored Configuration Setting by calling GetAsync.
            ConfigurationSetting gotSetting = client.Get("some_key");
            Debug.WriteLine(gotSetting.Value);

            // Delete the Configuration Setting from the Configuration Store when you don't need it anymore.
            client.Delete("some_key");
        }
    }
}
