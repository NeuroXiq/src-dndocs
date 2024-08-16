using DNDocs.Docs.Web.Model;
using DNDocs.Docs.Web.ValueTypes;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.ObjectPool;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text;
using System.Text.Json;
using Vinca.Exceptions;
using Vinca.Utils;

namespace DNDocs.Docs.Web.Services
{
    public interface IDMetrics
    {
        Task SaveInDbAndClear();
        void CacheMiss();
        void CacheHit();
        void SqlInsert(string methodName, long ellapsedMs, long byteDataLength);
        void SqlOpen(string dbName);
        void CreateProjectEllapsedTime(long elapsedMilliseconds);
        void CreateProjectFilesCount(int length);
        void CreateProjectZipSize(long length);
        void CreateProjectSiteItemUncompressedSize(int length);
        void CreateProjectSiteItemCompressedSize(int compressedCount);
        void SaveHttpLogsCount(int logsCount);
        void SaveAppLogsCount(int count);
        void SqlSelect(string queryName, long elapsedMilliseconds, long byteDataLength);
    }

    public class DMetrics : IDMetrics
    {
        const string NameCreateProjectEllapsedTime = "create-project.ellapsed-ms";
        const string NameCreateProjectFilesCount = "create-project.files-count";
        const string NameCreateProjectZipSize = "create-project.zip-size";
        const string NameCreateProjectSiteItemUncompressedSize = "create-project.uncompressed-file-size";
        const string NameCreateProjectSiteItemCompressedSize = "create-project.compresssed-file-size";
        const string NameSqlSelectDuration = "sql.select-duration";
        const string NameSqlInsertDuration = "sql.insert-duration";

        private readonly Meter mddocs;
        private readonly Counter<int> cSqlOpen;
        private readonly Counter<int> cSqlSelectCount;
        private readonly Counter<int> cSqlInsert;
        private readonly Counter<long> cSqlInsertByteDataLength;
        private readonly Counter<long> cSqlSelectByteDataLength;
        private readonly Counter<int> cCacheHit;
        private readonly Counter<int> cCacheMiss;
        private readonly Histogram<long> hCreateProjectEllapsedTime;
        private readonly Histogram<int> hCreateProjectFilesCount;
        private readonly Histogram<long> hCreateProjectZipSize;
        private readonly Histogram<int> hCreateProjectSiteItemUncompressedSize;
        private readonly Histogram<int> hCreateProjectSiteItemCompressedSize;
        private readonly Counter<int> cSaveHttpLogsCount;
        private readonly Counter<int> cSaveAppLogsCount;
        private readonly Histogram<long> hSqlSelectDuration;
        private readonly Histogram<long> hSqlInsertDuration;
        private readonly IServiceProvider serviceProvider;
        private readonly ObjectPool<StringBuilder> stringBuilderPool;
        private readonly ConcurrentBag<InstrumentState> instrumentStates;
        private readonly MeterListener meterListener;
        private readonly Dictionary<string, double[]> histogramsConfig;
        private MetersInDbCache metersInDbCache;

        public DMetrics(IMeterFactory meterFactory, IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            stringBuilderPool = new DefaultObjectPoolProvider().CreateStringBuilderPool();
            instrumentStates = new ConcurrentBag<InstrumentState>();
            meterListener = new MeterListener();
            metersInDbCache = null;

            mddocs = meterFactory.Create("DNDocs.Docs.DMetrics", "");
            cSqlOpen = mddocs.CreateCounter<int>("dndocs.sql.connection-open-count", "number", "open new sql connection");
            cSqlSelectCount = mddocs.CreateCounter<int>("dndocs.sql.select-count", "number", "sql SELECT count");
            cSqlInsert = mddocs.CreateCounter<int>("dndocs.sql.insert-count", "number", "sql insert count");
            cSaveHttpLogsCount = mddocs.CreateCounter<int>("dndocs.save-http-logs-count", "u", "count of http logs");
            cSaveAppLogsCount = mddocs.CreateCounter<int>("dndocs.save-app-logs-count", "u", "count of app logs");
            cCacheHit = mddocs.CreateCounter<int>("dndocs.memory-cache.hit", "number", "memory cache hits");
            cCacheMiss = mddocs.CreateCounter<int>("dndocs.memory-cache.miss", "number", "memory cache miss");
            cSqlInsertByteDataLength = mddocs.CreateCounter<long>("dndocs.sql.insert-byte-data-length", "{bytes_count}", "bytes count inserted to database");
            cSqlSelectByteDataLength = mddocs.CreateCounter<long>("dndocs.sql.select-byte-data-length", "{bytes_count}", "bytes count returned by SQL (if included)");

            hCreateProjectEllapsedTime = mddocs.CreateHistogram<long>(NameCreateProjectEllapsedTime, "ms", "create project duration");
            hCreateProjectFilesCount = mddocs.CreateHistogram<int>(NameCreateProjectFilesCount, "u", "measure count of files in a project");
            hCreateProjectZipSize = mddocs.CreateHistogram<long>(NameCreateProjectZipSize, "u", "measure site zip file size on create project");
            hCreateProjectSiteItemUncompressedSize = mddocs.CreateHistogram<int>(NameCreateProjectSiteItemUncompressedSize, "u", "measure uncompressed size single project content file");
            hCreateProjectSiteItemCompressedSize = mddocs.CreateHistogram<int>(NameCreateProjectSiteItemCompressedSize, "u", "measure compressed size single project content file");
            hSqlSelectDuration = mddocs.CreateHistogram<long>(NameSqlSelectDuration, "ms", "duration of SQL SELECT statement");
            hSqlInsertDuration = mddocs.CreateHistogram<long>(NameSqlInsertDuration, "ms", "duration of SQL INSERT statement");

            histogramsConfig = new Dictionary<string, double[]>()
            {
                { $"{NameCreateProjectEllapsedTime}", Enumerable.Range(1, 100).Select(t => 100 * (double)t).ToArray() },
                { $"{NameCreateProjectFilesCount}", Enumerable.Range(1, 500).Select(t => 20 * (double)t).ToArray() },
                { $"{NameCreateProjectZipSize}", Enumerable.Range(1, 100).Select(t => 1000000 * (double)t).ToArray() },  // 1MB
                { $"{NameCreateProjectSiteItemUncompressedSize}", Enumerable.Range(1, 100).Select(t => 10000 * (double)t).ToArray() },  // 10KB
                { $"{NameCreateProjectSiteItemCompressedSize}", Enumerable.Range(1, 100).Select(t => 1000 * (double)t).ToArray() }, // 1KB
                { $"{NameSqlSelectDuration}", Enumerable.Range(1, 1000).Select(t => (double)t).ToArray() }, //1ms 
                { $"{NameSqlInsertDuration}", Enumerable.Range(1, 1000).Select(t => (double)t).ToArray() },  // 1ms

                // external
                { $"kestrel.connection.duration", new double[] { 0.01, 0.02, 0.05, 0.1, 0.2, 0.5, 1, 2, 5, 10, 30, 60, 120, 300 } },
                { $"kestrel.tls_handshake.duration", new double[] { 0.005, 0.01, 0.025, 0.05, 0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10 } },
                { $"http.server.request.duration", new double[] { 0.005, 0.01, 0.025, 0.05, 0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10  } },
                { $"http.client.request.duration", new double[] { 0.005, 0.01, 0.025, 0.05, 0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10 } },
                { $"http.client.connection.duration", new double[] { 0.01, 0.02, 0.05, 0.1, 0.2, 0.5, 1, 2, 5, 10, 30, 60, 120, 300 } },
                { $"http.client.request.time_in_queue", new double[] { 0.01, 0.02, 0.05, 0.1, 0.2, 0.5, 1, 2, 5, 10, 30, 60, 120, 300 } },
                { $"dns.lookup.duration", new double[] { 0.01, 0.02, 0.05, 0.1, 0.2, 0.5, 1, 2, 5, 10, 30, 60, 120, 300 } },
                { $"http.client.open_connections", new double[] { 0.01, 0.02, 0.05, 0.1, 0.2, 0.5, 1, 2, 5, 10, 30, 60, 120, 300 } },
                { $"http.client.active_requests", new double[] { 0.01, 0.02, 0.05, 0.1, 0.2, 0.5, 1, 2, 5, 10, 30, 60, 120, 300 } },
            };

            meterListener.InstrumentPublished += (Instrument instrument, MeterListener listener) =>
            {
                // if (!(instrument.Meter.Name == "DNDocs.Docs.DMetrics" || histogramsConfig.Any(t => t.Key == instrument.Name))) return;

                InstrumentState state = new InstrumentState(instrument, stringBuilderPool, histogramsConfig.GetValueOrDefault($"{instrument.Name}"));
                listener.EnableMeasurementEvents(instrument, state);

                lock (instrumentStates) instrumentStates.Add(state);
            };

            meterListener.SetMeasurementEventCallback<int>((i, m, t, s) => OnMeasurementRecoreded(i, (long)m, t, s));
            meterListener.SetMeasurementEventCallback<long>(OnMeasurementRecoreded);
            meterListener.SetMeasurementEventCallback<double>(OnMeasurementRecoreded);
            meterListener.Start();
        }

        private void OnMeasurementRecoreded(
            Instrument instrument,
            long measurement,
            ReadOnlySpan<KeyValuePair<string, object>> tags,
            object state) => ((InstrumentState)state).OnMeasurementDouble(measurement, tags);

        private void OnMeasurementRecoreded(
            Instrument instrument,
            double measurement,
            ReadOnlySpan<KeyValuePair<string, object>> tags,
            object state) => ((InstrumentState)state).OnMeasurementDouble(measurement, tags);

        

        public async Task SaveInDbAndClear()
        {
            meterListener.RecordObservableInstruments();

            // todo: need to do something with txrepository, get rid of serviceprovider
            using var scope = serviceProvider.CreateScope();
            using var txRepository = scope.ServiceProvider.GetRequiredService<ITxRepository>();
            txRepository.BeginTransaction();

            List<CounterState> allCounterStates = new List<CounterState>();

            // create all instruments if not exist
            await InitMetersDbCache(txRepository);

            var nowInstrumentStates = instrumentStates.ToArray();
            foreach (var istate in nowInstrumentStates)
            {
                var counterStatesByTags = istate.CounterStateByTags.ToArray();

                foreach (var stateTagsPair in counterStatesByTags)
                {
                    string tags = stateTagsPair.Key;
                    CounterState counterState = stateTagsPair.Value;
                    allCounterStates.Add(counterState);

                    if (metersInDbCache.Instruments.ContainsKey(counterState.InstanceId)) continue;

                    var newInstrument = new MtInstrument(istate.Instrument.Name, istate.Instrument.Meter.Name, counterState.InstanceId, istate.InstrumentType, tags);
                    await txRepository.InsertMtInstrument(newInstrument);
                    metersInDbCache.Instruments[newInstrument.InstanceId] = newInstrument;


                    List<MtHRange> hranges = null;

                    if (newInstrument.Type == MtInstrumentType.Histogram)
                    {
                        hranges = istate.HistogramRanges.Select(t => new MtHRange(newInstrument.Id, t)).ToList();
                        hranges.Add(new MtHRange(newInstrument.Id, null)); // to store all values greater than last
                    }

                    if (hranges != null)
                    {
                        for (int i = 0; i < hranges.Count; i++)
                        {
                            await txRepository.InsertMtHRange(hranges[i]);
                            metersInDbCache.HRanges[$"{counterState.InstanceId}_{i}"] = hranges[i];
                        }
                    }
                }
            }

            // inserts measurements
            List<MtMeasurement> mtMeasurements = new List<MtMeasurement>();
            foreach (var cs in allCounterStates)
            {
                // question: should save metrics if 0?
                // if yes then there will be saved in db 5000 rows per 1 seconds
                MtInstrument instrument = metersInDbCache.Instruments[cs.InstanceId];
                
                bool isempty = (instrument.Type == MtInstrumentType.Counter && cs.Value == 0);
                isempty |= instrument.Type == MtInstrumentType.Histogram && cs.ValuesH.All(t => t == 0);
                
                if (isempty) continue;
                // if (instrument.Name == "http.server.request.duration") Debugger.Break();
                if (instrument.Type == MtInstrumentType.Counter || instrument.Type == MtInstrumentType.Gauge)
                {
                    mtMeasurements.Add(new MtMeasurement(instrument.Id, Interlocked.Exchange(ref cs.Value, 0), null));
                }
                else
                {
                    double[] replace = new double[cs.ValuesH.Length];
                    var dvals = Interlocked.Exchange(ref cs.ValuesH, replace);
                    for (int i = 0; i < dvals.Length; i++)
                    {
                        var ihr = metersInDbCache.HRanges[$"{cs.InstanceId}_{i}"];
                        mtMeasurements.Add(new MtMeasurement(instrument.Id, dvals[i], ihr.Id));
                    }
                }
            }

            foreach (var m in mtMeasurements) m.CreatedOn = DateTime.UtcNow;
            await txRepository.InsertMtMeasurement(mtMeasurements);
            await txRepository.CommitAsync();
            //throw new NotImplementedException();
        }

        private async Task InitMetersDbCache(ITxRepository txRepository)
        {
            if (metersInDbCache != null) return;
            metersInDbCache = new MetersInDbCache();
            IEnumerable<MtInstrument> instruments = await txRepository.SelectMtInstrument();
            IEnumerable<MtHRange> allHranges = await txRepository.SelectMtHRange();

            foreach (var inst in instruments)
            {
                MtHRange[] instHranges = allHranges.Where(t => t.MtInstrumentId == inst.Id)
                    .OrderBy(t => t.End.HasValue ? t.End : double.MaxValue)
                    .ToArray();

                metersInDbCache.Instruments[inst.InstanceId] = inst;

                for (int i = 0; i < instHranges.Length; i++)
                {
                    metersInDbCache.HRanges[$"{inst.InstanceId}_{i}"] = instHranges[i];
                }
            }
        
        }
        
        public void SqlInsert(string methodName, long ellapsedMs, long byteDataLength)
        {
            cSqlInsert.Add(1, new KeyValuePair<string, object?>("name", methodName));
            cSqlInsertByteDataLength.Add(byteDataLength);
            hSqlInsertDuration.Record(ellapsedMs, new KeyValuePair<string, object?>("name", methodName));
        }

        public void SqlOpen(string dbName)
        {
            cSqlOpen.Add(1, new KeyValuePair<string, object>("db", dbName));
        }

        public void CacheMiss() => cCacheMiss.Add(1);

        public void CacheHit() => cCacheHit.Add(1);

        public void CreateProjectEllapsedTime(long elapsedMilliseconds) => hCreateProjectEllapsedTime.Record(elapsedMilliseconds);

        public void CreateProjectFilesCount(int count) => hCreateProjectFilesCount.Record(count);

        public void CreateProjectZipSize(long length) => hCreateProjectZipSize.Record(length);

        public void CreateProjectSiteItemUncompressedSize(int length) => hCreateProjectSiteItemUncompressedSize.Record(length);

        public void CreateProjectSiteItemCompressedSize(int length) => hCreateProjectSiteItemCompressedSize.Record(length);

        public void SaveHttpLogsCount(int logsCount) => cSaveHttpLogsCount.Add(logsCount);

        public void SaveAppLogsCount(int count) => cSaveAppLogsCount.Add(count);

        public void SqlSelect(string queryName, long elapsedMilliseconds, long byteDataLength)
        {
            cSqlSelectCount.Add(1, new KeyValuePair<string, object?>("name", queryName));
            cSqlSelectByteDataLength.Add(byteDataLength);
            hSqlSelectDuration.Record(elapsedMilliseconds, new KeyValuePair<string, object?>("name", queryName));
        }

        class InstrumentState
        {
            public Instrument Instrument { get; set; }
            public ConcurrentDictionary<string, CounterState> CounterStateByTags { get; private set; }
            private readonly ObjectPool<StringBuilder> stringBuilderPool;
            public double[] HistogramRanges { get; private set; }
            public MtInstrumentType InstrumentType { get; private set; }

            public InstrumentState(Instrument instrument, ObjectPool<StringBuilder> stringBuilderPool, double[] historamConfig)
            {
                Instrument = instrument;
                CounterStateByTags = new ConcurrentDictionary<string, CounterState>();
                this.stringBuilderPool = stringBuilderPool;
                HistogramRanges = historamConfig;

                var instrumentType = instrument.GetType().GetGenericTypeDefinition();

                if (instrumentType == typeof(Histogram<>))
                {
                    InstrumentType = MtInstrumentType.Histogram;
                }
                else if (instrumentType == typeof(ObservableGauge<>))
                {
                    InstrumentType = MtInstrumentType.Gauge;
                }
                else InstrumentType = MtInstrumentType.Counter;
            }

            public void OnMeasurementDouble(
                double measurement,
                ReadOnlySpan<KeyValuePair<string, object>> tags)
            {
                var counter = GetCounterState(tags);

                if (InstrumentType == MtInstrumentType.Counter)
                {
                    ThreadSafeAddDouble(ref counter.Value, measurement);
                }
                else if (InstrumentType == MtInstrumentType.Gauge)
                {
                    counter.Value = measurement;
                }
                else
                {
                    for (int i = 0; i < HistogramRanges.Length + 1; i++)
                    {
                        if (i == HistogramRanges.Length || HistogramRanges[i] > measurement)
                        {
                            ThreadSafeAddDouble(ref counter.ValuesH[i], 1);
                            break;
                        }
                    }
                }
            }

            private void ThreadSafeAddDouble(ref double counter, double valueToAdd)
            {
                while (true)
                {
                    double valueBeforeSet = counter;
                    double newValue = counter + valueToAdd;
                    double valueAfterSet = Interlocked.CompareExchange(ref counter, newValue, valueBeforeSet);
                    if (valueAfterSet == valueBeforeSet)
                        break;
                }
            }

            private CounterState GetCounterState(ReadOnlySpan<KeyValuePair<string, object>> tags)
            {
                string tagsKey = ConvertTagsToString(tags);
                CounterState counterState = null;

                if (!CounterStateByTags.TryGetValue(tagsKey, out counterState))
                {
                    string instanceId = $"{InstrumentType}|{Instrument.Meter.Name}.{Instrument.Name}|{tagsKey}";
                    counterState = new CounterState();

                    if (InstrumentType == MtInstrumentType.Histogram)
                    {
                        counterState.ValuesH = new double[HistogramRanges.Length + 1];
                        instanceId = $"{instanceId}|hvals:{HistogramRanges.StringJoin(",")}";
                    }

                    counterState.InstanceId = instanceId;
                    CounterStateByTags.TryAdd(tagsKey, counterState);
                    counterState = CounterStateByTags[tagsKey];
                }

                return counterState;
            }

            private string ConvertTagsToString(ReadOnlySpan<KeyValuePair<string, object>> tags)
            {
                // todo how to avoid logs of tostring methods (new string creation?)?
                // maybe somehow store somewhere and return reference
                // from this store (dictionary/list or smt) instead of each time creating new string?

                if (tags.Length == 0) return string.Empty;

                var sb = stringBuilderPool.Get();

                sb.AppendFormat("{0}={1}", tags[0].Key, tags[0].Value);
                for (int i = 1; i < tags.Length; i++) sb.AppendFormat(",{0}={1}", tags[i].Key, tags[i].Value);

                var result = sb.ToString();
                stringBuilderPool.Return(sb);

                return result;
            }
        }

        class CounterState
        {
            public string InstanceId;
            public double Value;
            public double[] ValuesH;
        }

        class MetersInDbCache
        {
            public Dictionary<string, MtInstrument> Instruments { get; private set; }
            public Dictionary<string, MtHRange> HRanges { get; private set; }

            public MetersInDbCache()
            {
                Instruments = new Dictionary<string, MtInstrument>();
                HRanges = new Dictionary<string, MtHRange>();
            }
        }
    }
}
