using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using EPiServer.Core;
using LinqToEPiServer.Implementation.Expressions;

namespace LinqToEPiServer.Implementation.Visitors.PropertyReferenceExtractors
{
    public class PageDataMemberPropertyReferenceExtractor : MemberPropertyReferenceExtractorBase
    {
      
        protected override PropertyReference GetPropertyReferencFromMember(MemberExpression e)
        {
            string memberName = e.Member.Name;
            if (!memberName.StartsWith("Page"))
                memberName = "Page" + memberName;
            var reference = new PropertyReference(memberName, e.Type);
            
            return reference;
        }

        protected override bool AppliesToMember(MemberExpression e)
        {
            return e.Member.DeclaringType == typeof (PageData);
        }
    }
}