using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage.Blob;

using WebSocketManager;
using TwilioMediaStreams.Models;

namespace TwilioMediaStreams.Services
{
    public class MediaStreamHandler : WebSocketHandler
    {
        private readonly ProjectSettings _projectSettings;

        //We use a dictionary to separate the buffering of various websocket connections
        private Dictionary<string, List<byte[]>> dictionaryByteList = new Dictionary<string, List<byte[]>>();

        public MediaStreamHandler(WebSocketConnectionManager webSocketConnectionManager, IOptions<ProjectSettings> projectSettings) : base(webSocketConnectionManager)
        {
            _projectSettings = projectSettings.Value;
        }

        public override async Task OnConnected(WebSocket socket)
        {
            await base.OnConnected(socket);
            string socketId = WebSocketConnectionManager.GetId(socket);
            dictionaryByteList.Add(socketId, new List<byte[]>());
        }

        public override async Task ReceiveAsync(WebSocket socket, WebSocketReceiveResult result, byte[] buffer)
        {
            string socketId = WebSocketConnectionManager.GetId(socket);

            using (JsonDocument jsonDocument = JsonDocument.Parse(Encoding.UTF8.GetString(buffer, 0, result.Count)))
            {
                string eventMessage = jsonDocument.RootElement.GetProperty("event").GetString();

                switch (eventMessage)
                {
                    case "connected":
                        break;
                    case "start":
                        break;
                    case "media":
                        string payload = jsonDocument.RootElement.GetProperty("media").GetProperty("payload").GetString();
                        AddPayloadToBuffer(socketId, payload);
                        break;
                    case "stop":
                        await OnConnectionFinishedAsync(socket, socketId);
                        break;
                }
            }
        }



        private void AddPayloadToBuffer(string socketId, string payload)
        {
            //We convert the base64 encoded string into a byte array and append it to the appropriate buffer
            byte[] payloadByteArray = Convert.FromBase64String(payload);
            dictionaryByteList[socketId].Add(payloadByteArray);
        }


        private async Task OnConnectionFinishedAsync(WebSocket socket, string socketId)
        {
            // extract buffer data, create audio file, upload to storage
            await ProcessBufferAsync(socketId);

            // instruct the server to actually close the socket connection
            await OnDisconnected(socket);

            // clean up buffer 
            dictionaryByteList.Remove(socketId);
        }

        private async Task ProcessBufferAsync(string socketId)
        {
            byte[] completeAudioBuffer = CreateCompleteAudioByteArray(socketId);

            CloudBlockBlob blob = await StorageHandler.SetupCloudStorageAsync(_projectSettings);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                AudioHandler.GenerateAudioStream(completeAudioBuffer, memoryStream);

                // make sure the memory stream is returned to its beginning, ready to stream to storage
                memoryStream.Seek(0, SeekOrigin.Begin);

                //upload memory stream to cloud storage
                await blob.UploadFromStreamAsync(memoryStream);
            }
        }

        private byte[] CreateCompleteAudioByteArray(string socketId)
        {
            //get the relevant dictionary entry
            List<byte[]> byteList = dictionaryByteList[socketId];

            //create new byte array that will represent the "flattened" array
            List<byte> completeAudioByteArray = new List<byte>();

            foreach (byte[] byteArray in byteList)
            {
                foreach (byte singleByte in byteArray)
                {
                    completeAudioByteArray.Add(singleByte);
                }
            }

            //collate the List<T> of byte arrays into a single large byte array
            byte[] buffer = completeAudioByteArray.ToArray();
            return buffer;
        }


    }
}
