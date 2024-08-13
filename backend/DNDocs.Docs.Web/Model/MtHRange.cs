namespace DNDocs.Docs.Web.Model
{
    public class MtHRange
    {
        public int Id { get; set; }
        public int MtInstrumentId { get; set; }
        public double? End { get; set; }

        public MtHRange() { }

        public MtHRange(int mtInstrumentId, double? end)
        {
            MtInstrumentId = mtInstrumentId;
            End = end;
        }
    }
}
