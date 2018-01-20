namespace PeerReviewWeb.Models.FormSchema {
    public class TextForm : AbsFormEntry {
        public override string Type { get; } = "Text";

        public TextForm() {}

        public TextForm(string _id, string _prompt, uint _height)
        : base(_id, _prompt) {
            Height = _height;
        }

        public uint Height { get; set; }
    }
}