using DbUp;
using DbUp.Engine;
using DbUp.Engine.Output;

namespace DNDocs.Infrastructure.Utils
{
    class log : IUpgradeLog
    {
        public void WriteError(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        public void WriteInformation(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        public void WriteWarning(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }
    }

    class TransactionProcessor : IScriptPreprocessor
    {
        public string Process(string contents)
        {
            if (contents.StartsWith("--DBUPINFO:NOTRANSACTION"))
            {
                return contents;
            }

            return "BEGIN TRANSACTION;\r\n" + contents + "\r\nCOMMIT;";
        }
    }
}
