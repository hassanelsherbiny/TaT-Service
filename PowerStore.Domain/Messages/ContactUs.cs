﻿using PowerStore.Domain.Common;
using System;
using System.Collections.Generic;

namespace PowerStore.Domain.Messages
{
    /// <summary>
    /// Search term record (for statistics)
    /// </summary>
    public partial class ContactUs : BaseEntity
    {
        /// <summary>
        /// Gets or sets the customer identifier
        /// </summary>
        public string CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the store identifier
        /// </summary>
        public string StoreId { get; set; }

        /// <summary>
        /// Gets or sets the IP address
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// Gets or sets the date and time of entity creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }


        /// <summary>
        /// Gets or sets the email
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets full name
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets the subject
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets enquiry
        /// </summary>
        public string Enquiry { get; set; }

        /// <summary>
        /// Gets or sets the email account identifier
        /// </summary>
        public string EmailAccountId { get; set; }

        /// <summary>
        /// Gets or sets the vendor identifier
        /// </summary>
        public string VendorId { get; set; }

        /// <summary>
        /// Gets or sets the contact attribute description
        /// </summary>
        public string ContactAttributeDescription { get; set; }

        /// <summary>
        /// Gets or sets the contact attributes in XML format
        /// </summary>
        [Obsolete("Will be removed in version 5.0.0 - this field was replaced by Attributes")]
        public string ContactAttributesXml { get; set; }

        /// <summary>
        /// Gets or sets the custom attributes (see "AddressAttribute" entity for more info)
        /// </summary>
        public IList<CustomAttribute> Attributes { get; set; } = new List<CustomAttribute>();


    }
}
