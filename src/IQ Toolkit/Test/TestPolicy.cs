// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System;
using System.Collections.Generic;
using System.Reflection;
using IQToolkit;
using IQToolkit.Data;

namespace Test
{
    using IQToolkit.Data.Common;
    using IQToolkit.Data.Mapping;

    class TestPolicy : QueryPolicy
    {
        HashSet<string> included;

        internal TestPolicy(params string[] includedRelationships)
        {
            this.included = new HashSet<string>(includedRelationships);
        }

        public override bool IsIncluded(MemberInfo member)
        {
            return this.included.Contains(member.Name);
        }
    }

    class TestMapping : ImplicitMapping
    {
        public TestMapping(QueryLanguage language)
            : base(language)
        {
        }

        protected override bool IsGenerated(MappingEntity entity, MemberInfo member)
        {
            return member.Name == "OrderID" && member.DeclaringType.Name == "Order";
        }
    }
}
