﻿using PowerStore.Core.TypeConverters.Converter;
using PowerStore.Domain.Common;
using PowerStore.Domain.Shipping;
using System.Collections.Generic;
using System.ComponentModel;

namespace PowerStore.Core.Infrastructure
{
    public class WebTypeConverter : ITypeConverter
    {
        public void Register()
        {
            TypeDescriptor.AddAttributes(typeof(List<int>), new TypeConverterAttribute(typeof(GenericListTypeConverter<int>)));
            TypeDescriptor.AddAttributes(typeof(List<decimal>), new TypeConverterAttribute(typeof(GenericListTypeConverter<decimal>)));
            TypeDescriptor.AddAttributes(typeof(List<string>), new TypeConverterAttribute(typeof(GenericListTypeConverter<string>)));
            TypeDescriptor.AddAttributes(typeof(bool), new TypeConverterAttribute(typeof(BoolTypeConverter)));

            //dictionaries
            TypeDescriptor.AddAttributes(typeof(Dictionary<int, int>), new TypeConverterAttribute(typeof(GenericDictionaryTypeConverter<int, int>)));
            TypeDescriptor.AddAttributes(typeof(Dictionary<string, bool>), new TypeConverterAttribute(typeof(GenericDictionaryTypeConverter<string, bool>)));

            //shipping option
            TypeDescriptor.AddAttributes(typeof(ShippingOption), new TypeConverterAttribute(typeof(ShippingOptionTypeConverter)));
            TypeDescriptor.AddAttributes(typeof(List<ShippingOption>), new TypeConverterAttribute(typeof(ShippingOptionListTypeConverter)));
            TypeDescriptor.AddAttributes(typeof(IList<ShippingOption>), new TypeConverterAttribute(typeof(ShippingOptionListTypeConverter)));

            //custom attributes
            TypeDescriptor.AddAttributes(typeof(List<CustomAttribute>), new TypeConverterAttribute(typeof(CustomAttributeListTypeConverter)));
            TypeDescriptor.AddAttributes(typeof(IList<CustomAttribute>), new TypeConverterAttribute(typeof(CustomAttributeListTypeConverter)));

        }

        public int Order => 0;
    }
}
