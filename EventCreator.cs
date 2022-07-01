using System.Numerics;
using System.Text;

namespace RtsServer;

public class EventCreator
{
    private List<byte> buffer;
    private byte[] translateBuffer;
    public int readPos;

    public EventCreator()
    {
        readPos = 0;
        buffer = new List<byte>();
    }

    public void SetBytes(byte[] _data)
    {
        Write(_data);
        translateBuffer = buffer.ToArray();
    }

    public byte[] TranslateToByte()
    {
        translateBuffer = buffer.ToArray();
        return translateBuffer;
    }

    public int Length()
    {
        return buffer.Count; // Return the length of buffer
    }

    public void Write(byte _value)
    {
        buffer.Add(_value);
    }

    public void Write(byte[] _value)
    {
        buffer.AddRange(_value);
    }

    public void Write(int _value)
    {
        buffer.AddRange(BitConverter.GetBytes(_value));
    }

    public void Write(float _value)
    {
        buffer.AddRange(BitConverter.GetBytes(_value));
    }

    public void Write(bool _value)
    {
        buffer.AddRange(BitConverter.GetBytes(_value));
    }

    public void Write(string _value)
    {
        Write(_value.Length);
        buffer.AddRange(Encoding.ASCII.GetBytes(_value));
    }

    public void Write(Vector3 _value)
    {
        Write(_value.X);
        Write(_value.Y);
        Write(_value.Z);
    }

    public void Write(Quaternion _value)
    {
        Write(_value.X);
        Write(_value.Y);
        Write(_value.Z);
        Write(_value.W);
    }

    public int ReadIntRange()
    {
        int _value = BitConverter.ToInt32(translateBuffer, readPos);
        readPos += 4;
        return _value;
    }

    public string ReadStringRange()
    {
        int _length = ReadIntRange();
        string _value = Encoding.ASCII.GetString(translateBuffer, readPos, _length);
        readPos += _length;
        return _value;
    }

    public bool ReadBoolRange()
    {
        bool _value = BitConverter.ToBoolean(translateBuffer, readPos);
        readPos += 1;
        return _value;
    }

    public float ReadFloatRange()
    {
        float _value = BitConverter.ToSingle(translateBuffer, readPos);
        readPos += 4;
        return _value;
    }

    public Vector3 ReadVector3Range()
    {
        Vector3 _value = new Vector3(ReadFloatRange(), ReadFloatRange(), ReadFloatRange());
        return _value;
    }

    public Quaternion ReadQuaternionRange()
    {
        Quaternion _value = new Quaternion(ReadFloatRange(), ReadFloatRange(), ReadFloatRange(), ReadFloatRange());
        return _value;
    }

    public void ClearBuffers()
    {
        buffer.Clear();
        translateBuffer = null;
        readPos = 0;
    }
    public void ResetReadPos()
    {
        Console.WriteLine("Readpos: "+readPos);
        readPos = 0;
    }
    public List<byte> ReturnBufferList()
    {
        return buffer;
    }
}