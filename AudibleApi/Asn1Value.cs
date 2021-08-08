using System;
using System.Collections.Generic;
using System.Formats.Asn1;

namespace AudibleApi
{
     /// <summary>
     /// Represents an Asn.1 encoded value
     /// </summary>
    internal class Asn1Value
    {
        public Asn1Tag Type { get; private set; }
        public byte[] Value { get; private set; }
        public List<Asn1Value> Children { get; private set; }

        public override string ToString() => $"{nameof(Asn1Value)} ({Type})";
        public static Asn1Value Parse(byte[] bytes) => ParseInternal(bytes, out _);

        /// <summary>
        /// Recursively parse an Asn.1 object.
        /// </summary>
        private static Asn1Value ParseInternal(byte[] bytes, out int bytesConsumed)
        {
            var type = AsnDecoder.ReadEncodedValue(bytes, AsnEncodingRules.BER, out int contentOffset, out int contentLength, out bytesConsumed);

            var asn1Value = new Asn1Value
            {
                Type = type
            };

            byte[] content = new byte[contentLength];
            Buffer.BlockCopy(bytes, contentOffset, content, 0, contentLength);

            if (type == Asn1Tag.Sequence ||
                type == Asn1Tag.SetOf)
            {
                asn1Value.Children = new List<Asn1Value>();

                int consumedLength = 0;

                while (consumedLength < contentLength)
                {
                    var subVal = ParseInternal(content, out int subLength);
                    consumedLength += subLength;
                    asn1Value.Children.Add(subVal);

                    //Move all subsequent Asn.1 values to the front of the content buffer.
                    Buffer.BlockCopy(content, subLength, content, 0, contentLength - consumedLength);
                }
            }
            else
            {
                asn1Value.Value = content;
            }

            return asn1Value;
        }
    }
}
