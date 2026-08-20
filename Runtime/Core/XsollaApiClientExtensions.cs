// Copyright (c) 2025-2026 Xsolla Inc. All Rights Reserved.
// This is licensed software from Xsolla Inc. Powered by AccelByte.
// For limitation and restriction, contact your company contract manager.

using AccelByte.Core;
using System;
using System.Runtime.CompilerServices;

namespace Xsolla.AccelByte
{
    public static class XsollaApiClientExtensions
    {
        private static readonly ConditionalWeakTable<ApiClient, IXsollaAccelByteAuth> XsollaAuthByApiClient = new();

        public static IXsollaAccelByteAuth GetXsollaAuth(this ApiClient apiClient)
        {
            if (apiClient == null)
            {
                throw new ArgumentNullException(nameof(apiClient));
            }

            return XsollaAuthByApiClient.GetValue(
                apiClient,
                client => new XsollaAuth(client.GetUser(autoCreate: true)));
        }
    }
}
