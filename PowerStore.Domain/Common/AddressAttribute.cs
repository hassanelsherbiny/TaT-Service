using PowerStore.Domain.Catalog;
using PowerStore.Domain.Localization;
using System.Collections.Generic;

namespace PowerStore.Domain.Common
{
    /// <summary>
    /// Represents an address attribute
    /// </summary>
    public partial class AddressAttribute : BaseEntity, ILocalizedEntity
    {
        private ICollection<AddressAttributeValue> _addressAttributeValues;

        public AddressAttribute()
        {
            Locales = new List<LocalizedProperty>();
        }
        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the attribute is required
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Gets or sets the attribute control type identifier
        /// </summary>
        public int AttributeControlTypeId { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public int DisplayOrder { get; set; }


        /// <summary>
        /// Gets or sets the collection of locales
        /// </summary>
        public IList<LocalizedProperty> Locales { get; set; }

        /// <summary>
        /// Gets the attribute control type
        /// </summary>
        public AttributeControlType AttributeControlType
        {
            get
            {
                return (AttributeControlType)AttributeControlTypeId;
            }
            set
            {
                AttributeControlTypeId = (int)value;
            }
        }
        /// <summary>
        /// Gets the address attribute values
        /// </summary>
        public virtual ICollection<AddressAttributeValue> AddressAttributeValues
        {
            get { return _addressAttributeValues ??= new List<AddressAttributeValue>(); }
            protected set { _addressAttributeValues = value; }
        }
    }

}
