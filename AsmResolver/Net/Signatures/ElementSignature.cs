﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    public class ElementSignature : BlobSignature
    {
        public static ElementSignature FromReader(MetadataHeader header, TypeSignature typeSignature, IBinaryStreamReader reader)
        {
            long position = reader.Position;
            return new ElementSignature(ReadValue(header, typeSignature, reader))
            {
                StartOffset = position
            };
        }

        private static object ReadValue(MetadataHeader header, TypeSignature typeSignature, IBinaryStreamReader reader)
        {
            switch (typeSignature.ElementType)
            {
                case ElementType.Boolean:
                    return reader.ReadByte() == 1;
                case ElementType.Char:
                    return (char)reader.ReadUInt16();
                case ElementType.R4:
                    return reader.ReadSingle();
                case ElementType.R8:
                    return reader.ReadDouble();
                case ElementType.I1:
                    return reader.ReadSByte();
                case ElementType.I2:
                    return reader.ReadInt16();
                case ElementType.I4:
                    return reader.ReadInt32();
                case ElementType.I8:
                    return reader.ReadInt64();
                case ElementType.U1:
                    return reader.ReadByte();
                case ElementType.U2:
                    return reader.ReadUInt16();
                case ElementType.U4:
                    return reader.ReadUInt32();
                case ElementType.U8:
                    return reader.ReadUInt64();
                case ElementType.String:
                    return reader.ReadSerString();
                case ElementType.Object:
                    return ReadValue(header, TypeSignature.ReadFieldOrPropType(header, reader), reader);
                case ElementType.Class:
                case ElementType.Enum:
                case ElementType.ValueType:
                    var enumTypeDef = header.MetadataResolver.ResolveType(typeSignature);
                    if (enumTypeDef == null)
                        throw new MemberResolutionException(typeSignature);

                    if (enumTypeDef.IsEnum)
                        return ReadValue(header, enumTypeDef.GetEnumUnderlyingType(), reader);
                    break;
            }
            if (typeSignature.IsTypeOf("System", "Type"))
                return TypeSignature.FromAssemblyQualifiedName(header, reader.ReadSerString());
            throw new NotSupportedException("Unsupported element type " + typeSignature.ElementType);
        }

        public ElementSignature(object value)
        {
            Value = value;
        }

        public object Value
        {
            get;
            set;
        }

        public override uint GetPhysicalLength()
        {
            if (Value == null)
                throw new NotSupportedException();

            switch(Type.GetTypeCode(Value.GetType()))
            {
                case TypeCode.Boolean:
                case TypeCode.Byte:
                case TypeCode.SByte:
                    return sizeof (byte);
                case TypeCode.Char:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                    return sizeof (ushort);
                case TypeCode.Single:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                    return sizeof(uint);
                case TypeCode.Double:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    return sizeof (ulong);
                case TypeCode.String:
                    return ((Value as string).GetSerStringSize());
            }

            var typeSignature = Value as TypeSignature;
            if (typeSignature != null)
                return TypeNameBuilder.GetAssemblyQualifiedName(typeSignature).GetSerStringSize();
            
            throw new NotSupportedException();
        }

        public override void Write(WritingContext context)
        {
            if (Value == null)
                throw new ArgumentNullException();

            var writer = context.Writer;
            switch (Type.GetTypeCode(Value.GetType()))
            {
                case TypeCode.Boolean:
                    writer.WriteByte((byte)((bool)Value ? 1 : 0));
                    break;
                case TypeCode.Byte:
                    writer.WriteByte((byte)Value);
                    break;
                case TypeCode.Char:
                    writer.WriteUInt16((char)Value);
                    break;
                case TypeCode.Double:
                    writer.WriteDouble((double)Value);
                    break;
                case TypeCode.Int16:
                    writer.WriteInt16((short)Value);
                    break;
                case TypeCode.Int32:
                    writer.WriteInt32((int)Value);
                    break;
                case TypeCode.Int64:
                    writer.WriteInt64((long)Value);
                    break;
                case TypeCode.SByte:
                    writer.WriteSByte((sbyte)Value);
                    break;
                case TypeCode.Single:
                    writer.WriteSingle((float)Value);
                    break;
                case TypeCode.String:
                    writer.WriteSerString((string)Value);
                    break;
                case TypeCode.UInt16:
                    writer.WriteUInt16((ushort)Value);
                    break;
                case TypeCode.UInt32:
                    writer.WriteUInt32((uint)Value);
                    break;
                case TypeCode.UInt64:
                    writer.WriteUInt64((ulong)Value);
                    break;
                default:
                    var typeSignature = Value as TypeSignature;
                    if (typeSignature != null)
                        writer.WriteSerString(TypeNameBuilder.GetAssemblyQualifiedName(typeSignature));
                    else
                        throw new NotSupportedException();
                    break;
            }

        }
    }
}
