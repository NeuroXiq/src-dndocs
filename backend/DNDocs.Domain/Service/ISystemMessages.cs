using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Domain.Service
{
    public interface ISystemMessages
    {
        public void Trace(object obj, string title, string msg);
        public void Error(object obj, string title, string msg);
        public void Warning(object obj, string title, string msg);
        public void Info(object obj, string title, string msg);
        public void Success(object obj, string title, string msg);
    }
}