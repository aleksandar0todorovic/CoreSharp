﻿using System;
using System.Linq;
using System.Text.RegularExpressions;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Inspections;
using FluentNHibernate.Conventions.Instances;

namespace CoreSharp.NHibernate.Conventions
{
    public class ForeignKeyNameConvention : IReferenceConvention, IHasOneConvention, IHasManyToManyConvention
    {
        private const byte MAX_TABLE_NAME_LENGTH = 18;
        private const byte MAX_PROPERTY_NAME_LENGTH = 18;

        public void Apply(IManyToOneInstance instance)
        {
            var fkName = $"FK_{instance.EntityType.Name}To{instance.Class.Name}_{instance.Name}";

            instance.ForeignKey(fkName);
        }

        public void Apply(IOneToOneInstance instance)
        {
            var oneToOne = instance as IOneToOneInspector;
            var fkName = $"FK_{instance.EntityType.Name}To{oneToOne.Class.Name}_{instance.Name}";

            instance.ForeignKey(fkName);
        }

        public void Apply(IManyToManyCollectionInstance instance)
        {
            var fkName =
                $"FK_{instance.EntityType.Name}{instance.OtherSide.EntityType.Name}_{((ICollectionInspector) instance).Name}";

            instance.Relationship.ForeignKey(fkName);
        }

        private static readonly Regex FkRegex = new Regex(@"(?<!^)(?=[A-Z])", RegexOptions.Compiled);
        private static string GetFkName(string name)
        {
            var split = FkRegex.Split(name);
            var shorten = name;
            var length = 8;

            while (shorten.Length > 63)
            {
                shorten = string.Join("", split.Select(x => x.Length > length ? x.Substring(0, length) : x));
                length--;

                if (length < 3)
                {
                    throw new ApplicationException($"FK name too long: {name}");
                }
            }

            return shorten;
        }
    }
}
