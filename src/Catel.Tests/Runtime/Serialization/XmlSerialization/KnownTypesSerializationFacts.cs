﻿namespace Catel.Tests.Runtime.Serialization.XmlSerialization
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Runtime.Serialization;
    using Catel.Collections;
    using Catel.Data;
    using Catel.Runtime.Serialization;
    using Catel.Runtime.Serialization.Xml;
    using Catel.Tests.Data;
    using NUnit.Framework;

    public interface IParams
    {

    }

    // Setting the known types on a base class (not interface) solves the issue
    // but is not feasible as the possible derived classes are implemented by
    // plugins which are not known in advance. Is there a way to do this dynamically,
    // by making a request to the plugin manager?
    [KnownType("KnownTypes")]
    public class ParamsBase : ComparableModelBase, IParams
    {
        // This method returns the array of known types.
        public static Type[] KnownTypes()
        {
            return new[] { typeof(PluginA.Params), typeof(PluginB.Params) };
        }
    }

    namespace PluginA
    {
        public class Params : ParamsBase
        {
            #region Property: SettingA
            public String SettingA
            {
                get { return GetValue<String>(SettingAProperty); }
                set { SetValue(SettingAProperty, value); }
            }

            public static readonly IPropertyData SettingAProperty = RegisterProperty<String>("SettingA");
            #endregion
        }
    }

    namespace PluginB
    {
        public class Params : ParamsBase
        {
            #region Property: SettingB
            public String SettingB
            {
                get { return GetValue<String>(SettingBProperty); }
                set { SetValue(SettingBProperty, value); }
            }

            public static readonly IPropertyData SettingBProperty = RegisterProperty<string>("SettingB");
            #endregion
        }
    }

    public class ContainerInterfaces : ComparableModelBase
    {
        #region Property: Parameters
        public ObservableCollection<IParams> Parameters
        {
            get { return GetValue<ObservableCollection<IParams>>(ParametersProperty); }
            set { SetValue(ParametersProperty, value); }
        }

        public static readonly IPropertyData ParametersProperty = RegisterProperty("Parameters", () => new ObservableCollection<IParams>());
        #endregion
    }

    public class ContainerAbstractClasses : ComparableModelBase
    {
        #region Property: Parameters
        public ObservableCollection<ParamsBase> Parameters
        {
            get { return GetValue<ObservableCollection<ParamsBase>>(ParametersProperty); }
            set { SetValue(ParametersProperty, value); }
        }

        public static readonly IPropertyData ParametersProperty = RegisterProperty("Parameters", () => new ObservableCollection<ParamsBase>());
        #endregion
    }

    [KnownType(nameof(DictionaryTestClass.GetKnownTypes))]
    public class DictionaryTestClass : ComparableModelBase
    {
        // This method returns the array of known types.
        private static Type[] GetKnownTypes()
        {
            return new[] { typeof(ModelBaseFacts.Person) };
        }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public Dictionary<string, object> Values
        {
            get { return GetValue<Dictionary<string, object>>(ValuesProperty); }
            set { SetValue(ValuesProperty, value); }
        }

        /// <summary>
        /// Register the Values property so it is known in the class.
        /// </summary>
        public static readonly IPropertyData ValuesProperty = RegisterProperty("Values", () => new Dictionary<string, object>());
    }

    [TestFixture]
    public class Serialization
    {
        //[TestCase, Explicit]
        //public void DictionaryWithKnownTypes()
        //{
        //    var dictionary = new DictionaryTestClass();

        //    dictionary.Values.Add("A", new ModelBaseFacts.Person
        //    {
        //        FirstName = "John",
        //        LastName = "Doe"
        //    });

        //    dictionary.Values.Add("B", new ModelBaseFacts.Person
        //    {
        //        FirstName = "Jane",
        //        LastName = "Doe"
        //    });

        //    var dictionary2 = SerializationTestHelper.SerializeAndDeserialize(dictionary, SerializationTestHelper.GetXmlSerializer());

        //    Assert.AreEqual(dictionary, dictionary2);

        //    var dic1Elem1 = dictionary.Values.ElementAt(0);
        //    var dic2Elem1 = dictionary2.Values.ElementAt(0);

        //    Assert.AreEqual(dic1Elem1, dic2Elem1);

        //    var dic1Elem2 = dictionary.Values.ElementAt(1);
        //    var dic2Elem2 = dictionary2.Values.ElementAt(1);

        //    Assert.AreEqual(dic1Elem2, dic2Elem2);
        //}

        [TestCase]
        public void EnumerableOfInterfacesViaKnownTypes_SameNameDifferentNamespaces_SaveLoadRoundTrip()
        {
            var c = new ContainerInterfaces();

            var pA = new PluginA.Params();
            pA.SettingA = "TestA";
            c.Parameters.Add(pA);

            var pB = new PluginB.Params();
            pB.SettingB = "TestB";
            c.Parameters.Add(pB);

            using (var memoryStream = new MemoryStream())
            {
                var serializer = SerializationFactory.GetXmlSerializer();

                c.Save(memoryStream, serializer);
                memoryStream.Position = 0L;

                var c2 = serializer.Deserialize<ContainerInterfaces>(memoryStream);

                Assert.AreEqual(c, c2);
            }
        }

        [TestCase]
        public void EnumerableOfAbstractClassesViaKnownTypes_SameNameDifferentNamespaces_SaveLoadRoundTrip()
        {
            var c = new ContainerAbstractClasses();

            var pA = new PluginA.Params();
            pA.SettingA = "TestA";
            c.Parameters.Add(pA);

            var pB = new PluginB.Params();
            pB.SettingB = "TestB";
            c.Parameters.Add(pB);

            using (var memoryStream = new MemoryStream())
            {
                var serializer = SerializationFactory.GetXmlSerializer();

                c.Save(memoryStream, serializer);
                memoryStream.Position = 0L;

                var c2 = serializer.Deserialize<ContainerAbstractClasses>(memoryStream);

                Assert.AreEqual(c, c2);
            }
        }


        [TestCase]
        public void DataContractSerializerFactory_NoInstanceTest()
        {
            var typeList = new[]
            {
                typeof (IEnumerable<IParams>), typeof (ICollection<IParams>),
                typeof (IDictionary<string, IParams>), typeof (IDictionary<IParams, string>),
                typeof (IList<IParams>), typeof (Collection<IParams>), typeof (FastObservableCollection<IParams>),
                typeof (IEnumerable<KeyValuePair<IParams, object>>), typeof (IEnumerable<Lazy<IParams>>),
            };

            foreach (var collectionType in typeList)
            {
                var serializer = new DataContractSerializerFactory().
                    GetDataContractSerializer(typeof(ContainerAbstractClasses), collectionType, "TestXmlName", null, null);

                Assert.IsTrue(serializer.KnownTypes.Contains(typeof(PluginA.Params)));
                Assert.IsTrue(serializer.KnownTypes.Contains(typeof(PluginB.Params)));
            }
        }

        [TestCase]
        public void DataContractSerializerFactory_InstanceTest()
        {
            var itemList = new IParams[] { new PluginA.Params { SettingA = "TestA" }, new PluginB.Params { SettingB = "TestB" } };


            var containerList = new ICollection[]
            {
                new Collection<IParams>(itemList), new List<IParams>(itemList),
                new FastObservableCollection<IParams>(itemList),
                new Collection<KeyValuePair<IParams, object>>(new[]
                {
                    new KeyValuePair<IParams, object>(itemList[0], new object()),
                    new KeyValuePair<IParams, object>(itemList[1], new object())
                }),
                new Collection<Lazy<IParams>>(new[]
                {
                    new Lazy<IParams>(() => itemList[0]),
                    new Lazy<IParams>(() => itemList[1])
                })
            };

            foreach (var collection in containerList)
            {
                var serializer = new DataContractSerializerFactory().
                    GetDataContractSerializer(typeof(object), collection.GetType(), "TestXmlName");

                Assert.IsTrue(serializer.KnownTypes.Contains(typeof(PluginA.Params)));
                Assert.IsTrue(serializer.KnownTypes.Contains(typeof(PluginB.Params)));
            }
        }
    }
}
