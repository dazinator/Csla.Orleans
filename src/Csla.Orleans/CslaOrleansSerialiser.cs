﻿using Csla.Core;
using Csla.Serialization;
using Csla.Serialization.Mobile;
using Orleans.Serialization;
using System;
using System.IO;

namespace Csla.Orleans
{
    /// <summary>
    /// An orleans serialiser that uses a CSLA <see cref="ISerializationFormatter"/> to maintain compatibility with CSLA.
    /// </summary>
    /// <remarks>
    /// see  https://dotnet.github.io/orleans/Documentation/Advanced-Concepts/Serialization.html
    /// and https://github.com/MarimerLLC/csla/blob/0041bc665942aa7e3a4a546adde6e5f8ad715aab/Source/Csla.Shared/Serialization/Mobile/MobileFormatter.cs
    /// </remarks>
    public class CslaOrleansSerialiser : IExternalSerializer
    {       

        private readonly ISerializationFormatter _cslaSerializarionFormatter;

        public CslaOrleansSerialiser()
        {
            _cslaSerializarionFormatter = SerializationFormatterFactory.GetFormatter();
        }

        public object DeepCopy(object source, ICopyContext context)
        {
            var clone = ObjectCloner.Clone(source);
            context.RecordCopy(source, clone);
            return clone;
        }

        public object Deserialize(Type expectedType, IDeserializationContext context)
        {
            var size = context.StreamReader.ReadInt();

            if (size == 0)
            {
                return null;
            }

            using (var memoryStream = new MemoryStream(size))
            {
                var bytes = context.StreamReader.ReadBytes(size);

                memoryStream.Write(bytes, 0, bytes.Length);
                memoryStream.Seek(0, SeekOrigin.Begin);
                var deserialised = _cslaSerializarionFormatter.Deserialize(memoryStream);
                return deserialised;
            }
        }

        public bool IsSupportedType(Type itemType)
        {
            //_cslaSerializarionFormatter.
            if(!itemType.IsSerializable)
            {
                return false;
            }

            var hasInterface = itemType.GetInterface(nameof(IMobileObject));
            return hasInterface != null;
        }

        public void Serialize(object item, ISerializationContext context, Type expectedType)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (item == null)
            {
                context.StreamWriter.Write(0);
                return;
            }

            using (var memoryStream = new MemoryStream())
            {
                _cslaSerializarionFormatter.Serialize(memoryStream, item);
                context.StreamWriter.Write(memoryStream.Length);

                // memoryStream.Seek(0, SeekOrigin.Begin); 
                context.StreamWriter.Write(memoryStream.ToArray());
            }
        }
    }
}