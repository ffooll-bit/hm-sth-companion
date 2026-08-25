using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace HmSth.Poc.Tests;

internal sealed class FakePineServer : IDisposable
{
    private readonly TcpListener _listener;
    private Task _exchange;

    public FakePineServer()
    {
        _listener = new TcpListener(IPAddress.Loopback, 0);
        _listener.Start();
        Port = ((IPEndPoint)_listener.LocalEndpoint).Port;
        _exchange = Task.CompletedTask;
    }

    public int Port { get; }

    // New protocol: 4-byte size (u32 LE) + 1-byte result code + payload
    public static byte[] Ok(byte[] payload)
    {
        int bodyLength = 1 + payload.Length; // 1 byte result code + payload
        int packetSize = 5 + bodyLength; // 4 bytes size + bodyLength
        byte[] response = new byte[packetSize];

        BinaryPrimitives.WriteUInt32LittleEndian(response, (uint)packetSize);
        response[4] = 0; // result code 0 = success
        payload.CopyTo(response, 5);
        return response;
    }

    public static byte[] OkString(string value)
    {
        byte[] text = Encoding.UTF8.GetBytes(value);
        byte[] payload = new byte[4 + text.Length + 1];
        BinaryPrimitives.WriteUInt32LittleEndian(payload, (uint)(text.Length + 1));
        text.CopyTo(payload, 4);
        payload[payload.Length - 1] = 0; // null terminator
        return Ok(payload);
    }

    public static byte[] Error(int resultCode)
    {
        int packetSize = 6; // 4 bytes size + 1 result code + 0 payload
        byte[] response = new byte[packetSize];
        BinaryPrimitives.WriteUInt32LittleEndian(response, (uint)packetSize);
        response[4] = (byte)resultCode;
        return response;
    }

    public void ServeOne(Func<byte[], byte[]>? replyFactory)
    {
        _exchange = Task.Run(() =>
        {
            using Socket socket = _listener.AcceptSocket();

            // Read request: 4-byte size + 1 opcode + optional address
            byte[] head = new byte[5];
            ReadExact(socket, head);

            int requestSize = (int)BinaryPrimitives.ReadUInt32LittleEndian(head);
            byte opcode = head[4];

            if (replyFactory is null)
            {
                if (requestSize > 5)
                {
                    ReadExact(socket, new byte[requestSize - 5]);
                }

                socket.Shutdown(SocketShutdown.Both);
                return;
            }

            byte[] request = head;

            if (requestSize > 5)
            {
                request = new byte[requestSize];
                head.CopyTo(request, 0);
                ReadExact(socket, request.AsSpan(5));
            }

            byte[] reply = replyFactory(request);
            socket.Send(reply);
        });
    }

    // Simulates a server that accepts connection but never responds (causes client read timeout)
    public void ServeOneTimeout()
    {
        _exchange = Task.Run(() =>
        {
            using Socket socket = _listener.AcceptSocket();

            byte[] head = new byte[5];
            ReadExact(socket, head);

            int requestSize = (int)BinaryPrimitives.ReadUInt32LittleEndian(head);

            if (requestSize > 5)
            {
                ReadExact(socket, new byte[requestSize - 5]);
            }

            // Never send response, never close - let client timeout
            Thread.Sleep(Timeout.Infinite);
        });
    }

    private static void ReadExact(Socket socket, Span<byte> buffer)
    {
        int offset = 0;

        while (offset < buffer.Length)
        {
            int read = socket.Receive(buffer[offset..]);

            if (read <= 0)
            {
                throw new EndOfStreamException();
            }

            offset += read;
        }
    }

    public void Dispose() => _listener.Stop();
}