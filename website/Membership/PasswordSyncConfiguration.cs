/*
 * Copyright 2012-2014 Matthew Cosand
 */
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Kcsara.Database.Web.Membership.Config
{
    public class PasswordSyncSection : ConfigurationSection
    {
        [ConfigurationProperty("", IsRequired=false, IsKey=false, IsDefaultCollection=true)]
        public SynchronizerCollection Items
        {
            get { return ((SynchronizerCollection)base[""]); }
            set { base[""] = value; }
        }
    }

    [ConfigurationCollection(typeof(SynchronizerElement), CollectionType=ConfigurationElementCollectionType.BasicMapAlternate)]
    public class SynchronizerCollection : ConfigurationElementCollection
    {
        internal const string PropertyName = "synchronizer";
        
        
        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMapAlternate; }
        }

        protected override string ElementName
        {
            get { return PropertyName; }
        }

        protected override bool IsElementName(string elementName)
        {
            return (elementName == PropertyName);
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new SynchronizerElement();
        }
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((SynchronizerElement)element).Type;
        }

        public override bool IsReadOnly()
        {
            return false;
        }
    }

    public class SynchronizerElement : ConfigurationElement
    {
        [ConfigurationProperty("type", IsRequired=true, IsKey=true)]
        public string Type
        {
            get { return (string)this["type"]; }
            set { this["type"] = value; }
        }

        [ConfigurationProperty("option")]
        public string Option
        {
            get { return (string)this["option"]; }
            set { this["option"] = value; }
        }
    }
}
