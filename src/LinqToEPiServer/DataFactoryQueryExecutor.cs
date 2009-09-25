using EPiServer;
using EPiServer.Core;
using LinqToEPiServer.Linq;

namespace LinqToEPiServer
{
    public class DataFactoryQueryExecutor : IQueryExecutor
    {
        public PageDataCollection FindPagesWithCriteria(PageReference root, params PropertyCriteria[] criteria)
        {
            PropertyCriteriaCollection criteriaCollection = criteria.ToCriteriaCollection();
            return DataFactory.Instance.FindPagesWithCriteria(root, criteriaCollection);
        }
    }
}