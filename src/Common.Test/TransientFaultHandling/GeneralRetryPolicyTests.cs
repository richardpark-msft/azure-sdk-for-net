﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Common.TransientFaultHandling;
using Xunit;

namespace Microsoft.WindowsAzure.Common.Test.TransientFaultHandling
{
    /// <summary>
    /// Implements general test cases for retry policies.
    /// </summary>
    public class GeneralRetryPolicyTests : IDisposable
    {
        public GeneralRetryPolicyTests()
        {
            //RetryPolicyFactory.CreateDefault();
        }

        public void Dispose()
        {
            //RetryPolicyFactory.SetRetryManager(null, false);
        }

        [Fact]
        public void TestNegativeRetryCount()
        {
            try
            {
                // First, instantiate a policy directly bypassing the configuration data validation.
                var retryPolicy = new RetryPolicy<DefaultHttpErrorDetectionStrategy>(-1);
                Assert.True(false, "When the RetryCount is negative, the retry policy should throw an exception.");
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.Equal("retryCount", ex.ParamName);
            }
        }

        [Fact]
        public void TestNegativeRetryInterval()
        {
            try
            {
                // First, instantiate a policy directly bypassing the configuration data validation.
                var retryPolicy = new RetryPolicy<DefaultHttpErrorDetectionStrategy>(3, TimeSpan.FromMilliseconds(-2));
                Assert.True(false, "When the RetryInterval is negative, the retry policy should throw an exception.");
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.Equal("retryInterval", ex.ParamName);
            }
        }
      
        [Fact]
        public void TestNegativeMinBackoff()
        {
            try
            {
                // First, instantiate a policy directly bypassing the configuration data validation.
                new RetryPolicy<DefaultHttpErrorDetectionStrategy>(3, TimeSpan.FromMilliseconds(-1), TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(100));
                Assert.True(false, "When the MinBackoff is negative, the retry policy should throw an exception.");
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.Equal("minBackoff", ex.ParamName);
            }
        }

        [Fact]
        public void TestNegativeMaxBackoff()
        {
            try
            {
                // First, instantiate a policy directly bypassing the configuration data validation.
                new RetryPolicy<DefaultHttpErrorDetectionStrategy>(3, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(-2), TimeSpan.FromMilliseconds(100));
                Assert.True(false, "When the MaxBackoff is negative, the retry policy should throw an exception.");
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.Equal("maxBackoff", ex.ParamName);
            }
        }

        [Fact]
        public void TestNegativeDeltaBackoff()
        {
            try
            {
                // First, instantiate a policy directly bypassing the configuration data validation.
                new RetryPolicy<DefaultHttpErrorDetectionStrategy>(3, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(-1));
                Assert.True(false, "When the DeltaBackoff is negative, the retry policy should throw an exception.");
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.Equal("deltaBackoff", ex.ParamName);
            }
        }

        [Fact]
        public void TestMinBackoffGreaterThanMax()
        {
            try
            {
                // First, instantiate a policy directly bypassing the configuration data validation.
                new RetryPolicy<DefaultHttpErrorDetectionStrategy>(3, TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100));
                Assert.True(false, "When the MinBackoff greater than MaxBackoff, the retry policy should throw an exception.");
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.Equal("minBackoff", ex.ParamName);
            }
        }

        [Fact]
        public void TestLargeDeltaBackoff()
        {
            int retryCount = 0;
            TimeSpan totalDelay;

            // First, instantiate a policy directly bypassing the configuration data validation.
            var retryPolicy = new RetryPolicy<DefaultHttpErrorDetectionStrategy>(3, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(100000000000000));

            TestRetryPolicy(retryPolicy, out retryCount, out totalDelay);
            Assert.Equal<int>(3, retryCount);
        }

        
        #region Private methods
        internal static void TestRetryPolicy(RetryPolicy retryPolicy, out int retryCount, out TimeSpan totalDelay)
        {
            int callbackCount = 0;
            double totalDelayInMs = 0;

            retryPolicy.Retrying += (sender, args) =>
            {
                callbackCount++;
                totalDelayInMs += args.Delay.TotalMilliseconds;
            };

            try
            {
                retryPolicy.ExecuteAction(() =>
                {
                    throw new TimeoutException("Forced Exception");
                });
            }
            catch (TimeoutException ex)
            {
                Assert.Equal("Forced Exception", ex.Message);
            }

            retryCount = callbackCount;
            totalDelay = TimeSpan.FromMilliseconds(totalDelayInMs);
        }
        #endregion
    }
}
