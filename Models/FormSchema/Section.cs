namespace PeerReviewWeb.Models.FormSchema {
    public class Section : AbsFormEntry {
        public override string Type { get; } = "Section";

        public Section() {}
        
        public Section(string _id, string _prompt, Schema _inner)
        : base(_id, _prompt) {
            InnerSchema = _inner;
        }

        public Schema InnerSchema { get; set; }
    }
}