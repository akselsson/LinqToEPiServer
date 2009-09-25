using System;
using System.Collections;
using EPiServer.BaseLibrary;

namespace LinqToEPiServer.Tests.Fakes
{
    public  class NullContext : IContext
    {
        private readonly Hashtable _values = new Hashtable();
        private const IRepository _repository = null;

        #region IContext Members

        public void FreezeTime(DateTime now)
        {
        }

        public void OffsetTime(TimeSpan offset)
        {
        }

        public void NormalTime()
        {
        }

        public IRepository Repository
        {
            get { return _repository; }
        }

        public object this[string name]
        {
            get { return _values[name]; }
            set { _values[name] = value; }
        }

        public DateTime RequestTime
        {
            get { return DateTime.Now; }
        }

        public DateTime Now
        {
            get { return DateTime.Now; }
        }

        #endregion
    }
}