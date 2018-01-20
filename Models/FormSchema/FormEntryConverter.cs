/**
 *  Adapted from code by Nicholas Westby\
 *  "Bulletproof Interface Deserialization in Json.NET"
 *  http://skrift.io/articles/archive/bulletproof-interface-deserialization-in-jsonnet/
 */

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PeerReviewWeb.Models.FormSchema {
    public class FormEntryConverter : JsonConverter {
        public override bool CanRead => true;
        public override bool CanWrite => false;
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(AbsFormEntry);
        }

        public override void WriteJson(
            JsonWriter writer,
            object value,
            JsonSerializer serializer)
        {
            throw new InvalidOperationException("Use the default serializer.");
        }

        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            var jobj = JObject.Load(reader);
            var entry = default(AbsFormEntry);
            var variant = jobj["Type"].Value<string>();
            switch(variant) {
                case "Likert":
                    entry = new LikertForm();
                    break;
                case "Text":
                    entry = new TextForm();
                    break;
                case "Section":
                    entry = new Section();
                    break;
                default:
                    throw new ArgumentException($"Invalid FormEntry variant: {variant}");
            }

            serializer.Populate(jobj.CreateReader(), entry);
            return entry;
        }
    }
}