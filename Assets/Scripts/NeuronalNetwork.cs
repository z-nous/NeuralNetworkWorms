/* 
 * Written by Bunny83 2016.06.21
 * 
 * License : The MIT License
 * https://opensource.org/licenses/MIT
 * 
 * Original source:
 * https://dl.dropboxusercontent.com/u/7761356/UnityAnswers/Code/NeuronalNetwork.cs
 */

using UnityEngine;
using System.Collections.Generic;
using System.Xml;
using System.IO;

public enum EActivationType
{
    Linear,  // 0
    Sigmoid, // 1
    StepZP,  // 2
    StepNP,  // 3
    COUNT    // 4
}
public enum ENeuronType
{
    Input,
    Hidden,
    Output
}

public struct NeuronLink
{
    public Neuron neuron;
    public float weight;
    public float Value { get { return neuron.Value * weight; } }
}

public class Neuron
{
    float val;
    public float Value
    {
        get { return val; }
        set { val = value; }
    }

    public NeuronLink[] inputs = null;

    public ENeuronType type = ENeuronType.Hidden;
    public EActivationType activationType = EActivationType.Linear;
    public float SigmoidBase = 0f;

    public Neuron(ENeuronType aType)
    {
        type = aType;
    }
    #region serialization
    public Neuron(XmlNode aNode, SerializationContext<Neuron> aContext)
    {
        if (aNode.Name != "Neuron")
            throw new System.Exception("Expected element 'Neuron' but got '" + aNode.Name + "'");
        var id = uint.Parse(aNode.Attributes["id"].Value);
        type = (ENeuronType)System.Enum.Parse(typeof(ENeuronType), aNode.Attributes["type"].Value);
        activationType = (EActivationType)System.Enum.Parse(typeof(EActivationType), aNode.Attributes["activationType"].Value);
        SigmoidBase = float.Parse(aNode.Attributes["SigmoidBase"].Value);
        inputs = new NeuronLink[aNode.ChildNodes.Count];
        for (int i = 0; i < inputs.Length; i++)
        {
            int index = i;
            var link = aNode.ChildNodes.Item(i);
            inputs[i].weight = float.Parse(link.Attributes["weight"].Value);
            var neuronId = uint.Parse(link.Attributes["neuron"].Value);
            aContext.GetObject(neuronId, (n) => inputs[index].neuron = n);
        }
        aContext.RegisterObject(id, this);
    }
    public void Serialize(XmlNode aParent, SerializationContext<Neuron> aContext)
    {
        var doc = aParent.OwnerDocument;
        var neuron = aParent.AppendChild(doc.CreateElement("Neuron"));
        neuron.Attributes.Append(doc.CreateAttribute("id")).Value = aContext.GetId(this).ToString();
        neuron.Attributes.Append(doc.CreateAttribute("type")).Value = type.ToString();
        neuron.Attributes.Append(doc.CreateAttribute("activationType")).Value = activationType.ToString();
        neuron.Attributes.Append(doc.CreateAttribute("SigmoidBase")).Value = SigmoidBase.ToString();
        if (inputs != null)
        {
            for (int i = 0; i < inputs.Length; i++)
            {
                var link = neuron.AppendChild(doc.CreateElement("InputLink"));
                link.Attributes.Append(doc.CreateAttribute("neuron")).Value = aContext.GetId(inputs[i].neuron).ToString();
                link.Attributes.Append(doc.CreateAttribute("weight")).Value = inputs[i].weight.ToString();
            }
        }
    }

    public Neuron(BinaryReader aReader, SerializationContext<Neuron> aContext)
    {
        byte marker = aReader.ReadByte();
        if (marker != 0x3)
            throw new System.Exception("Binarystream: expected neuron (0x3) got " + marker);
        uint id = aReader.ReadUInt32();
        type = (ENeuronType)aReader.ReadByte();
        activationType = (EActivationType)aReader.ReadByte();
        SigmoidBase = aReader.ReadSingle();
        int count = aReader.ReadInt32();
        if (count >= 0)
            inputs = new NeuronLink[count];
        for (int i = 0; i < count; i++)
        {
            int index = i;
            aContext.GetObject(aReader.ReadUInt32(), (n) => inputs[index].neuron = n);
            inputs[i].weight = aReader.ReadSingle();
        }
        aContext.RegisterObject(id, this);
    }
    public void Serialize(BinaryWriter aWriter, SerializationContext<Neuron> aContext)
    {
        aWriter.Write((byte)0x3); // Neuron
        aWriter.Write(aContext.GetId(this));
        aWriter.Write((byte)type);
        aWriter.Write((byte)activationType);
        aWriter.Write(SigmoidBase);
        if (inputs != null)
        {
            aWriter.Write(inputs.Length);
            for (int i = 0; i < inputs.Length; i++)
            {
                aWriter.Write(aContext.GetId(inputs[i].neuron));
                aWriter.Write(inputs[i].weight);
            }
        }
        else
            aWriter.Write((int)-1);
    }

    #endregion serialization

    private float Activate(float sum)
    {
        switch (activationType)
        {
            default:
            case EActivationType.Linear:
                return sum;
            case EActivationType.Sigmoid:
                return 1f / (1f + Mathf.Pow(SigmoidBase, sum));
            case EActivationType.StepZP:
                return (sum > 0f) ? 1f : 0f;
            case EActivationType.StepNP:
                return (sum > 0f) ? 1f : -1f;
        }
    }

    /// <summary>
    /// Calculates the neurons current value based on the input neurons
    /// and the link weights
    /// </summary>
    public void Calculate()
    {
        if (inputs == null || inputs.Length == 0)
            return;
        float sum = 0f;
        for (int i = 0; i < inputs.Length; i++)
        {
            sum += inputs[i].Value;
        }
        if (type == ENeuronType.Output)
            val = sum;
        else
            val = Activate(sum);
    }

    /// <summary>
    /// Mutates the neuron values based on the passed mutation rate
    /// </summary>
    public void Mutate(int aMutationRate)
    {
        if (aMutationRate == -1 || Random.Range(0, aMutationRate) == 1)
            activationType = (EActivationType)Random.Range(0, (int)EActivationType.COUNT);
        for (int i = 0; i < inputs.Length; i++)
        {
            if (aMutationRate == -1 || Random.Range(0, aMutationRate) == 1)
                inputs[i].weight = Random.Range(-1f, 1f);
        }
        if (aMutationRate == -1 || Random.Range(0, aMutationRate) == 1)
            SigmoidBase = Random.Range(0f, 10.0f);

    }
    /// <summary>
    /// Establish links to the previous layer of neurons
    /// </summary>
    /// <param name="aNeurons"> Neuron list of the previous layer</param>
    public void Link(List<Neuron> aNeurons)
    {
        inputs = new NeuronLink[aNeurons.Count];
        for (int i = 0; i < aNeurons.Count; i++)
            inputs[i] = new NeuronLink { neuron = aNeurons[i], weight = 1f };
    }
    /// <summary>
    /// Establish links to the previous layer of neurons, copy link weights from a source
    /// </summary>
    /// <param name="aNeurons">Neuron list of the previous layer</param>
    /// <param name="aSourceWeights">list of neuron links from which we copy the weights</param>
    public void Link(List<Neuron> aNeurons, NeuronLink[] aSourceWeights)
    {
        if (aNeurons == null || aSourceWeights == null || aNeurons.Count != aSourceWeights.Length)
            throw new System.ArgumentException("Duplicate of Neuron failed!");
        inputs = new NeuronLink[aNeurons.Count];
        for (int i = 0; i < aNeurons.Count; i++)
            inputs[i] = new NeuronLink { neuron = aNeurons[i], weight = aSourceWeights[i].weight };
    }
}

public class NeuronLayer
{
    public List<Neuron> neurons;
    public ENeuronType type;
    public NeuronLayer(ENeuronType aType, int aNeuronCount)
    {
        type = aType;
        neurons = new List<Neuron>(aNeuronCount);
        for (int i = 0; i < aNeuronCount; i++)
            neurons.Add(new Neuron(aType));
    }
    public NeuronLayer(NeuronLayer aSource) : this(aSource.type, aSource.neurons.Count) { }

    #region serialization
    public NeuronLayer(XmlNode aNode, SerializationContext<Neuron> aContext)
    {
        if (aNode.Name != "Layer")
            throw new System.Exception("Expected element 'Layer' but got '" + aNode.Name + "'");

        type = (ENeuronType)System.Enum.Parse(typeof(ENeuronType), aNode.Attributes["type"].Value);
        int count = aNode.ChildNodes.Count;
        neurons = new List<Neuron>(count);
        for (int i = 0; i < count; i++)
            neurons.Add(new Neuron(aNode.ChildNodes[i], aContext));
    }
    public void Serialize(XmlNode aParent, SerializationContext<Neuron> aContext)
    {
        var doc = aParent.OwnerDocument;
        var layer = aParent.AppendChild(doc.CreateElement("Layer"));
        layer.Attributes.Append(doc.CreateAttribute("type")).Value = type.ToString();
        for (int i = 0; i < neurons.Count; i++)
        {
            neurons[i].Serialize(layer, aContext);
        }
    }

    public NeuronLayer(BinaryReader aReader, SerializationContext<Neuron> aContext)
    {
        byte marker = aReader.ReadByte();
        if (marker != 0x2)
            throw new System.Exception("Binarystream: expected layer (0x2) got " + marker);
        type = (ENeuronType)aReader.ReadByte();
        int count = aReader.ReadInt32();
        neurons = new List<Neuron>(count);
        for (int i = 0; i < count; i++)
        {
            neurons.Add(new Neuron(aReader, aContext));
        }
    }
    public void Serialize(BinaryWriter aWriter, SerializationContext<Neuron> aContext)
    {
        aWriter.Write((byte)0x2); // layer
        aWriter.Write((byte)type);
        aWriter.Write(neurons.Count);
        for (int i = 0; i < neurons.Count; i++)
        {
            neurons[i].Serialize(aWriter, aContext);
        }
    }

    #endregion serialization

    public void InitNeurons(NeuronLayer aInputLayer)
    {
        for (int i = 0; i < neurons.Count; i++)
            neurons[i].Link(aInputLayer.neurons);
    }
    public void InitNeurons(NeuronLayer aInputLayer, NeuronLayer aSourceLayer)
    {
        for (int i = 0; i < neurons.Count; i++)
            neurons[i].Link(aInputLayer.neurons, aSourceLayer.neurons[i].inputs);
    }

    public void Calculate()
    {
        for (int i = 0; i < neurons.Count; i++)
            neurons[i].Calculate();
    }
    public void Mutate(int aMutationRate)
    {
        for (int i = 0; i < neurons.Count; i++)
            neurons[i].Mutate(aMutationRate);
    }

}

public class NeuronalNetwork
{
    public NeuronLayer input;
    public NeuronLayer output;
    public List<NeuronLayer> layers = new List<NeuronLayer>();

    public NeuronalNetwork(int aInputCount, int aOutputCount, params int[] aMiddleLayers)
    {
        input = new NeuronLayer(ENeuronType.Input, aInputCount);
        output = new NeuronLayer(ENeuronType.Output, aOutputCount);
        if (aMiddleLayers != null)
            for (int i = 0; i < aMiddleLayers.Length; i++)
                layers.Add(new NeuronLayer(ENeuronType.Hidden, aMiddleLayers[i]));
        var last = input;
        for (int i = 0; i < layers.Count; i++)
        {
            layers[i].InitNeurons(last);
            last = layers[i];
        }
        output.InitNeurons(last);
        // init all neurons with random values
        Mutate(-1);
    }
    public NeuronalNetwork(NeuronalNetwork aSource)
    {
        input = new NeuronLayer(aSource.input);
        output = new NeuronLayer(aSource.output);
        var last = input;
        for (int i = 0; i < aSource.layers.Count; i++)
        {
            var layer = new NeuronLayer(aSource.layers[i]);
            layers.Add(layer);
            layer.InitNeurons(last, aSource.layers[i]);
            last = layer;
        }
        output.InitNeurons(last, aSource.output);
    }

    #region serialization
    public NeuronalNetwork(XmlNode aNode)
    {
        if (aNode.Name != "NeuronalNetwork")
            throw new System.Exception("Expected element 'NeuronalNetwork' but got '" + aNode.Name + "'");

        var context = new SerializationContext<Neuron>();
        input = new NeuronLayer(aNode["Inputs"]["Layer"], context);
        output = new NeuronLayer(aNode["Outputs"]["Layer"], context);
        var nLayers = aNode["Layers"];
        int count = nLayers.ChildNodes.Count;
        layers.Capacity = count;
        for (int i = 0; i < count; i++)
            layers.Add(new NeuronLayer(nLayers.ChildNodes.Item(i), context));
    }
    public void Serialize(XmlNode aParent)
    {
        var doc = aParent.OwnerDocument;
        var context = new SerializationContext<Neuron>();

        var nNetwork = aParent.AppendChild(doc.CreateElement("NeuronalNetwork"));
        input.Serialize(nNetwork.AppendChild(doc.CreateElement("Inputs")), context);
        output.Serialize(nNetwork.AppendChild(doc.CreateElement("Outputs")), context);
        var nLayers = nNetwork.AppendChild(doc.CreateElement("Layers"));
        for (int i = 0; i < layers.Count; i++)
        {
            layers[i].Serialize(nLayers, context);
        }
    }

    public NeuronalNetwork(BinaryReader aReader)
    {
        byte marker = aReader.ReadByte();
        if (marker != 0x1)
            throw new System.Exception("Binarystream: expected network (0x1) got " + marker);
        var context = new SerializationContext<Neuron>();
        input = new NeuronLayer(aReader, context);
        output = new NeuronLayer(aReader, context);
        int count = aReader.ReadInt32();
        for (int i = 0; i < count; i++)
        {
            layers.Add(new NeuronLayer(aReader, context));
        }
    }
    public void Serialize(BinaryWriter aWriter)
    {
        var context = new SerializationContext<Neuron>();

        aWriter.Write((byte)0x1); // Network
        input.Serialize(aWriter, context);
        output.Serialize(aWriter, context);
        aWriter.Write(layers.Count);
        for (int i = 0; i < layers.Count; i++)
        {
            layers[i].Serialize(aWriter, context);
        }
    }
    #endregion serialization

    /// <summary>
    /// Calculate all values inside the neuronal network
    /// </summary>
    public void Calculate()
    {
        for (int i = 0; i < layers.Count; i++)
        {
            layers[i].Calculate();
        }
        output.Calculate();
    }
    public void Mutate(int aMutationRate)
    {
        for (int i = 0; i < layers.Count; i++)
        {
            layers[i].Mutate(aMutationRate);
        }
        output.Mutate(aMutationRate);
    }

    public void SetInput(int aIndex, float aValue)
    {
        if (aIndex < 0 || aIndex >= input.neurons.Count)
            throw new System.ArgumentOutOfRangeException("aIndex is out of range");
        input.neurons[aIndex].Value = aValue;
    }
    public float GetOutput(int aIndex)
    {
        if (aIndex < 0 || aIndex >= output.neurons.Count)
            throw new System.ArgumentOutOfRangeException("aIndex is out of range");
        return output.neurons[aIndex].Value;

    }
}

/// <summary>
/// Simple class to handle cross references between classes during serialization
/// During serialization one can use "GetId(obj)" to get a unique ID within this
/// SerializationContext for this object.
/// During deserialization one can use "GetObject" and pass an ID with a callback
/// to perform a "late bind" of the value. Once the actual instance is deserialized
/// you would call "RegisterObject" which would execute all the pending callbacks
/// 
/// This allows even circular references since the reference-binding is deferred
/// until the actual object is created / deserialized.
/// </summary>
public class SerializationContext<TObjectType> where TObjectType : class
{
    private class Instance
    {
        public uint id = 0;
        public TObjectType obj = default(TObjectType);
        public List<System.Action<TObjectType>> callbacks = new List<System.Action<TObjectType>>();
    }

    private Dictionary<uint, Instance> idToObject = new Dictionary<uint, Instance>();
    private Dictionary<TObjectType, Instance> objectToId = new Dictionary<TObjectType, Instance>();
    uint lastID = 0; // ID counter
    public uint GetId(TObjectType aObject)
    {
        Instance obj = null;
        if (!objectToId.TryGetValue(aObject, out obj))
        {
            obj = new Instance();
            obj.id = ++lastID;
            obj.obj = aObject;
            idToObject.Add(obj.id, obj);
            objectToId.Add(aObject, obj);
        }
        return obj.id;
    }
    public void GetObject(uint aID, System.Action<TObjectType> aCallback)
    {
        Instance obj = null;
        if (!idToObject.TryGetValue(aID, out obj))
        {
            obj = new Instance();
            obj.id = aID;
            idToObject.Add(aID, obj);
        }
        if (obj.obj != null)
            aCallback(obj.obj);
        else
            obj.callbacks.Add(aCallback);
    }
    public void RegisterObject(uint aId, TObjectType aObject)
    {
        Instance obj = null;
        if (!idToObject.TryGetValue(aId, out obj))
        {
            obj = new Instance();
            obj.id = aId;
            idToObject.Add(obj.id, obj);
            objectToId.Add(aObject, obj);
        }
        if (obj.obj != null)
            throw new System.Exception("Object already registrated.");

        obj.obj = aObject;
        for (int i = 0; i < obj.callbacks.Count; i++)
        {
            obj.callbacks[i](aObject);
        }
        obj.callbacks.Clear();
    }
}