namespace PeerReviewWeb.Models.FormSchema {
    public abstract class AbsFormEntry {

        public AbsFormEntry() {}
        public AbsFormEntry(string _id, string _prompt) {
            Id = _id;
            Prompt = _prompt;
        }

        public string Id { get; set; }
        public string Prompt { get; set; }

        public bool Required { get; set; } = true;

        public abstract string Type { get; }
    }
}