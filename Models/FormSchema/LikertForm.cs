namespace PeerReviewWeb.Models.FormSchema
{
    public class LikertForm : AbsFormEntry
    {
        public override string Type { get; } = "Likert";

        public LikertForm() {}

        public LikertForm(
            string _id,
            string _prompt,
            uint _scale,
            string _maxLabel = "Strongly Agree",
            string _minLabel = "Strongly Disagree")
            : base(_id, _prompt)
        {
            Scale = _scale;
            MinLabel = _minLabel;
            MaxLabel = _maxLabel;
        }

        public uint Scale { get; set; }
        public string MaxLabel { get; set; }
        public string MinLabel { get; set; }
    }
}