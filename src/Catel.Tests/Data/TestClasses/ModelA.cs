﻿namespace Catel.Tests.Data
{
    using System;
    using System.Runtime.Serialization;
    using Catel.Data;

    /// <summary>
    /// ModelA Data object class which fully supports serialization, property changed notifications,
    /// backwards compatibility and error checking.
    /// </summary>
    [Serializable]
    public class ModelA : Model
    {
        /// <summary>
        /// Initializes a new object from scratch.
        /// </summary>
        public ModelA() { }

        /// <summary>
        /// Gets or sets property B.
        /// </summary>
        public string B
        {
            get { return GetValue<string>(BProperty); }
            set { SetValue(BProperty, value); }
        }

        /// <summary>
        /// Register the B property so it is known in the class.
        /// </summary>
        public static readonly IPropertyData BProperty = RegisterProperty("B", string.Empty);
    }
}
