using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

class uZone
{
    static void Main()
    {
        int listenPort = 12345;
        UdpClient udpServer = new UdpClient(listenPort);

        Console.WriteLine($"[+] UDP Server listening on port {listenPort}");

        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

        while (true)
        {
            //GLOBAL NOTES
            //07-00-00-XX-YY (This is like the start of it)
            //XX = some number (idk yet) [4th byte always]
            //YY = NatStatus [5th byte always]

            //uNode (Connect)
            //1st Stage: 07-00-00-1B-01-05-75-4E-6F-64-65-
            //FC-34-59-0E-47-CB-9E-80-8C-00-D3-A9-D2-9B-D1
            //-EC-D0-57-00-00

            //?? Unknown (ConnectResponse)
            //2nd stage: 07-00-00-29-02-42-50-A8-B9-07-B9-
            //6F-37-32-64-22-1E-92-E9-20-5B-E2-41-00-00-FC
            //-34-59-0E-47-CB-9E-80-8C-00-D3-A9-D2-9B-D1-EC
            //-D0-57-00-00


            byte[] data = udpServer.Receive(ref remoteEndPoint);
            Console.WriteLine($"\n[>] Received {data.Length} bytes from {remoteEndPoint}");

            Console.WriteLine(BitConverter.ToString(data));

            string ascii = Encoding.ASCII.GetString(data);
            Console.WriteLine("\nASCII (filtered): " + new string(ascii.Where(c => c >= 32 && c <= 126).ToArray()));


            //byte[] response = new byte[data.Length];

            PacketType packetType = PacketType.System;
            PacketDeliveryMethod deliveryMethod = PacketDeliveryMethod.ReliableInOrder1;

            byte PacketDetails = GetPacketDetails(packetType, deliveryMethod);


            //GET NAT-STATUS
            //byte NatStatus = data.Length >= 4 ? data[4] : new byte();

            /*byte[] packetBytes = new byte[]
            {
                PacketDetails, 0x00, 0x00, 0x1B, NatStatus, 0x05, 0x75, 0x4E,
                0x6F, 0x64, 0x65, 0xFC, 0x34, 0x59, 0x0E, 0x47,
                0xCB, 0x9E, 0x80, 0x8C, 0x00, 0xD3, 0xA9, 0xD2,
                0x9B, 0xD1, 0xEC, 0xD0, 0x57, 0x00, 0x00
            };*/

            byte[] response = (byte[])data.Clone();

            response[0] = PacketDetails;
            byte natStatus = response[4];



            if (response.Length == 31) //connect
            {
                response[15] = 0x00;
            }
            else if (response.Length == 45) //connectResponse
            {
                //response = new byte[response.Length - 1];
                //response[0] = GetPacketDetails(PacketType.System, PacketDeliveryMethod.S);
                //response[4] = natStatus;
                response[9] = 0x00;
            }

            udpServer.Send(response, response.Length, remoteEndPoint);
            Console.WriteLine("[<] Sent response: " + BitConverter.ToString(response));

            //udpServer.Send(data, data.Length, remoteEndPoint);

            Console.WriteLine($"[<] Sent response with changes: 0x{PacketDetails:X2}");
        }
    }

    static byte GetPacketDetails(PacketType packetType, PacketDeliveryMethod deliveryMethod)
    {
        return (byte)(((int)deliveryMethod << 3) | (int)packetType);
    }
}

internal enum PacketType
{
    None,
    User,
    UserFragmented,
    Acknowledge,
    AckBitVector,
    OutOfBand,
    Unused,
    System
}

internal enum PacketDeliveryMethod
{
    Unreliable,
    UnreliableInOrder1,
    UnreliableInOrder2,
    UnreliableInOrder3,
    UnreliableInOrder4,
    UnreliableInOrder5,
    UnreliableInOrder6,
    UnreliableInOrder7,
    UnreliableInOrder8,
    UnreliableInOrder9,
    UnreliableInOrder10,
    UnreliableInOrder11,
    UnreliableInOrder12,
    UnreliableInOrder13,
    UnreliableInOrder14,
    UnreliableInOrder15,
    ReliableUnordered,
    ReliableInOrder1,
    ReliableInOrder2,
    ReliableInOrder3,
    ReliableInOrder4,
    ReliableInOrder5,
    ReliableInOrder6,
    ReliableInOrder7,
    ReliableInOrder8,
    ReliableInOrder9,
    ReliableInOrder10,
    ReliableInOrder11,
    ReliableInOrder12,
    ReliableInOrder13,
    ReliableInOrder14,
    ReliableInOrder15
}

internal enum NatStatus
{
    Error,
    Connect, //stage 1
    ConnectResponse, //stage 2
    ConnectionEstablished,
    ConnectionRejected,
    Disconnect,
    Discovery = 50,
    DiscoveryResponse,
    NatIntroduction = 75,
    Ping = 100,
    Pong,
    StringTableAck = 200
}