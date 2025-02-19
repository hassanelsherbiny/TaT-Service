﻿using System;
using System.Collections.Generic;
using System.Text;

namespace PowerStore.Plugin.ExternalAuth.Facebook
{
    class FacebookAuthConfig
    {
        /// <summary>
        /// Gets or sets OAuth2 client identifier
        /// </summary>
        public string ClientKeyIdentifier { get; set; }

        /// <summary>
        /// Gets or sets OAuth2 client secret
        /// </summary>
        public string ClientSecret { get; set; }
    }
}
