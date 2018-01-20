using Csla.Core;
using Csla.Serialization;
using Csla.Serialization.Mobile;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<CslaOrleansSerialiser> _logger;

        public CslaOrleansSerialiser(ILogger<CslaOrleansSerialiser> logger)
        {
            _logger = logger;
            _cslaSerializarionFormatter = SerializationFormatterFactory.GetFormatter();
        }

        public object DeepCopy(object source, ICopyContext context)
        {
            _logger.LogInformation("Deep copying: {source}", source.GetType().Name);
            var clone = ObjectCloner.Clone(source);
            context.RecordCopy(source, clone);
            return clone;
        }

        public object Deserialize(Type expectedType, IDeserializationContext context)
        {
            try
            {
                _logger.LogInformation("Deserialising: {name}", expectedType?.Name ?? "{null expetced type}");
                var size = context.StreamReader.ReadInt();

                _logger.LogInformation("Size {size}", size);
                if (size == 0)
                {
                    return null;
                }

                using (var memoryStream = new MemoryStream(size))
                {
                    var bytes = context.StreamReader.ReadBytes(size);

                    memoryStream.Write(bytes, 0, bytes.Length);
                    memoryStream.Position = 0;
                   // memoryStream.Seek(0, SeekOrigin.Begin);
                    var deserialised = _cslaSerializarionFormatter.Deserialize(memoryStream);
                    return deserialised;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not deserialise.");
                throw;
            }

        }

        public bool IsSupportedType(Type itemType)
        {
            //_cslaSerializarionFormatter.
            if (!itemType.IsSerializable)
            {
                _logger.LogDebug("Type {type} is not supported as its not serializable.", itemType.Name);
                return false;
            }

            var hasInterface = itemType.GetInterface(nameof(IMobileObject)) != null;
            if (!hasInterface)
            {
                _logger.LogDebug("Type {type} is not supported.", itemType.Name);
            }

            _logger.LogDebug("Type {type} IS supported.");
            return hasInterface;
        }

        public void Serialize(object item, ISerializationContext context, Type expectedType)
        {

            try
            {
                _logger.LogInformation("Serialising: {name}", expectedType?.Name ?? "{null expetced type}");

                if (context == null)
                {
                    throw new ArgumentNullException(nameof(context));
                }

                if (item == null)
                {
                    _logger.LogDebug("Serialising null with a 0 int");
                    context.StreamWriter.Write(0);
                    return;
                }

                using (var memoryStream = new MemoryStream())
                {
                    _logger.LogDebug("Serialising item with size {size}", memoryStream.Length);
                    _cslaSerializarionFormatter.Serialize(memoryStream, item);
                    context.StreamWriter.Write(memoryStream.Length);

                    // memoryStream.Seek(0, SeekOrigin.Begin); 
                    context.StreamWriter.Write(memoryStream.ToArray());
                }

            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not Serialize.");
                throw;
            }
          
          
        }
    }
}
