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

    public static byte[] Ok(byte[] payload)
    {
        byte[] response = new byte[6 + payload.Length];
        BinaryPrimitives.WriteUInt16LittleEndian(response, (ushort)response.Length);
        BinaryPrimitives.WriteInt32LittleEndian(response.AsSpan(2), 0);
        payload.CopyTo(response, 6);
        return response;
    }

    public static byte[] OkString(string value)
    {
        byte[] text = Encoding.UTF8.GetBytes(value);
        byte[] payload = new byte[4 + text.Length + 1];
        BinaryPrimitives.WriteUInt32LittleEndian(payload, (uint)(text.Length + 1));
        text.CopyTo(payload, 4);
        return Ok(payload);
    }

    public static byte[] Error(int resultCode)
    {
        byte[] response = new byte[6];
        BinaryPrimitives.WriteUInt16LittleEndian(response, 6);
        BinaryPrimitives.WriteInt32LittleEndian(response.AsSpan(2), resultCode);
        return response;
    }

    public void ServeOne(Func<byte[], byte[]>? replyFactory)
    {
        _exchange = Task.Run(() =>
        {
            using Socket socket = _listener.AcceptSocket();

            byte[] head = new byte[3];
            ReadExact(socket, head);

            int requestSize = head[0] | (head[1] << 8);

            if (replyFactory is null)
            {
                if (requestSize > 3)
                {
                    ReadExact(socket, new byte[requestSize - 3]);
                }

                socket.Shutdown(SocketShutdown.Both);
                return;
            }

            byte[] request = head;

            if (requestSize > 3)
            {
                request = new byte[requestSize];
                head.CopyTo(request, 0);
                ReadExact(socket, request.AsSpan(3));
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

            byte[] head = new byte[3];
            ReadExact(socket, head);

            int requestSize = head[0] | (head[1] << 8);

            if (requestSize > 3)
            {
                ReadExact(socket, new byte[requestSize - 3]);
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
