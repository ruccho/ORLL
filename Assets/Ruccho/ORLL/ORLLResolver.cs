#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168

namespace ORLL
{
    using System;
    using MessagePack;
    using MessagePack.Resolvers;

    public class ORLLResolver : global::MessagePack.IFormatterResolver
    {
        public static readonly global::MessagePack.IFormatterResolver Instance = new ORLLResolver();

        ORLLResolver()
        {

        }

        public global::MessagePack.Formatters.IMessagePackFormatter<T> GetFormatter<T>()
        {
            return FormatterCache<T>.formatter;
        }

        static class FormatterCache<T>
        {
            public static readonly global::MessagePack.Formatters.IMessagePackFormatter<T> formatter;

            static FormatterCache()
            {
                var f = GeneratedResolverGetFormatterHelper.GetFormatter(typeof(T));
                if (f != null)
                {
                    formatter = (global::MessagePack.Formatters.IMessagePackFormatter<T>)f;
                    return;
                }

                f = StandardResolver.Instance.GetFormatter<T>();
                if (f != null)
                {
                    formatter = (global::MessagePack.Formatters.IMessagePackFormatter<T>)f;
                    return;
                }


            }
        }
    }

    internal static class GeneratedResolverGetFormatterHelper
    {
        static readonly global::System.Collections.Generic.Dictionary<Type, int> lookup;

        static GeneratedResolverGetFormatterHelper()
        {
            lookup = new global::System.Collections.Generic.Dictionary<Type, int>(4)
            {
                {typeof(global::System.Collections.Generic.List<string>), 0 },
                {typeof(global::System.Collections.Generic.Dictionary<string, string>), 1 },
                {typeof(global::ORLL.LocalizationPack), 2 },
                {typeof(global::LocalizationUnit), 3 },
            };
        }

        internal static object GetFormatter(Type t)
        {
            int key;
            if (!lookup.TryGetValue(t, out key)) return null;

            switch (key)
            {
                case 0: return new global::MessagePack.Formatters.ListFormatter<string>();
                case 1: return new global::MessagePack.Formatters.DictionaryFormatter<string, string>();
                case 2: return new MessagePack.Formatters.ORLL.LocalizationPackFormatter();
                case 3: return new MessagePack.Formatters.LocalizationUnitFormatter();
                default: return null;
            }
        }
    }
}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612



#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168

namespace MessagePack.Formatters.ORLL
{
    using System;
    using MessagePack;


    public sealed class LocalizationPackFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::ORLL.LocalizationPack>
    {

        public int Serialize(ref byte[] bytes, int offset, global::ORLL.LocalizationPack value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 8);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Guid, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Name, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.DisplayName, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Description, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Author, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.AuthorDescription, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<string>>().Serialize(ref bytes, offset, value.GroupNames, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Version, formatterResolver);
            return offset - startOffset;
        }

        public global::ORLL.LocalizationPack Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Guid__ = default(string);
            var __Name__ = default(string);
            var __Version__ = default(string);
            var __DisplayName__ = default(string);
            var __Description__ = default(string);
            var __Author__ = default(string);
            var __AuthorDescription__ = default(string);
            var __GroupNames__ = default(global::System.Collections.Generic.List<string>);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __Guid__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 1:
                        __Name__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 7:
                        __Version__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 2:
                        __DisplayName__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 3:
                        __Description__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 4:
                        __Author__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 5:
                        __AuthorDescription__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 6:
                        __GroupNames__ = formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<string>>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::ORLL.LocalizationPack(__Guid__, __Name__, __DisplayName__, __Description__, __Author__, __AuthorDescription__);
            ____result.Guid = __Guid__;
            ____result.Name = __Name__;
            ____result.Version = __Version__;
            ____result.DisplayName = __DisplayName__;
            ____result.Description = __Description__;
            ____result.Author = __Author__;
            ____result.AuthorDescription = __AuthorDescription__;
            ____result.GroupNames = __GroupNames__;
            return ____result;
        }
    }

}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612
#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168

namespace MessagePack.Formatters
{
    using System;
    using MessagePack;


    public sealed class LocalizationUnitFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::LocalizationUnit>
    {

        public int Serialize(ref byte[] bytes, int offset, global::LocalizationUnit value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 2);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Name, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.Dictionary<string, string>>().Serialize(ref bytes, offset, value.Values, formatterResolver);
            return offset - startOffset;
        }

        public global::LocalizationUnit Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Name__ = default(string);
            var __Values__ = default(global::System.Collections.Generic.Dictionary<string, string>);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __Name__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 1:
                        __Values__ = formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.Dictionary<string, string>>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::LocalizationUnit();
            ____result.Name = __Name__;
            ____result.Values = __Values__;
            return ____result;
        }
    }

}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612
