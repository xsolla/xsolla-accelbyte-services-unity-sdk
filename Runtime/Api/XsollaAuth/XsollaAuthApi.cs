// Copyright (c) 2025 Xsolla Inc. All Rights Reserved.
// This is licensed software from Xsolla Inc. Powered by AccelByte.
// For limitation and restriction, contact your company contract manager.

using AccelByte.Core;
using AccelByte.Models;

namespace Xsolla.GamingService
{
    [UnityEngine.Scripting.Preserve]
    public class XsollaAuthApi : ApiBase
    {
        [UnityEngine.Scripting.Preserve]
        public XsollaAuthApi(IHttpClient httpClient
            , Config config
            , ISession session)
            : this(httpClient, config, session, null)
        {
        }

        [UnityEngine.Scripting.Preserve]
        public XsollaAuthApi( IHttpClient httpClient
            , Config config
            , ISession session
            , HttpOperator httpOperator) 
            : base( httpClient, config, config.IamServerUrl, session , httpOperator)
        {
        }
    }
}