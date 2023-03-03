using System;
using Azure;
using System.Net.Http;
using Azure.Core.Pipeline;
using Azure.DigitalTwins.Core;
using Azure.Identity;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Azure.Messaging.EventGrid;

namespace AllinOneFunApp
{
    public class AllinOneFunApp
    {
        private static readonly string adtInstanceUrl = Environment.GetEnvironmentVariable("ADT_SERVICE_URL");
        private static readonly HttpClient httpClient = new HttpClient();

        [FunctionName("AllinOneFunApp")]
        // While async void should generally be used with caution, it's not uncommon for Azure function apps, since the function app isn't awaiting the task.
#pragma warning disable AZF0001 // Suppress async void error
        public async void Run([EventGridTrigger] EventGridEvent eventGridEvent, ILogger log)
#pragma warning restore AZF0001 // Suppress async void error
        {
            if (adtInstanceUrl == null) log.LogError("Application setting \"ADT_SERVICE_URL\" not set");

            try
            {
                // Authenticate with Digital Twins
                var cred = new DefaultAzureCredential();
                var client = new DigitalTwinsClient(new Uri(adtInstanceUrl), cred);
                log.LogInformation($"ADT service client connection created.");
            
                if (eventGridEvent != null && eventGridEvent.Data != null)
                {
                    log.LogInformation(eventGridEvent.Data.ToString());

                    // <Find_device_ID_and_temperature>
                    JObject deviceMessage = (JObject)JsonConvert.DeserializeObject(eventGridEvent.Data.ToString());
                    //string deviceId = (string)deviceMessage["systemProperties"]["iothub-connection-device-id"];
                    
                    string deviceId = "MangOHBoard";

                    if (deviceMessage["body"]["elems"]["imu"] != null && deviceMessage["body"]["elems"]["imu"]["temp"] != null){
                        var temperature = deviceMessage["body"]["elems"]["imu"]["temp"];
                        log.LogInformation($"Device:{deviceId} temp is:{temperature}");

                        // <Update_twin_with_device_temperature>
                        var updateTwinData1 = new JsonPatchDocument();
                        updateTwinData1.AppendReplace("/Temperature", temperature.Value<double>());
                        await client.UpdateDigitalTwinAsync(deviceId, updateTwinData1);
                        // </Update_twin_with_device_temperature>
                    } 
                    else if (deviceMessage["body"]["elems"]["imu"] != null && deviceMessage["body"]["elems"]["imu"]["accel"] != null){
                        var x = deviceMessage["body"]["elems"]["imu"]["accel"]["x"];
                        var y = deviceMessage["body"]["elems"]["imu"]["accel"]["y"];
                        var z = deviceMessage["body"]["elems"]["imu"]["accel"]["z"];
                        log.LogInformation($"Device:{deviceId} x is:{x} y is:{y} and z is:{z} ");

                        // <Update_twin_with_device_temperature>
                        var updateTwinData2 = new JsonPatchDocument();
                        updateTwinData2.AppendReplace("/x", x.Value<double>());
                        updateTwinData2.AppendReplace("/y", y.Value<double>());
                        updateTwinData2.AppendReplace("/z", z.Value<double>());
                        await client.UpdateDigitalTwinAsync(deviceId, updateTwinData2);
                        // </Update_twin_with_device_temperature>
                    } 
                    else if(deviceMessage["body"]["elems"]["light"] != null){
                        var light = deviceMessage["body"]["elems"]["light"];
                        log.LogInformation($"Device:{deviceId} light is:{light}");

                        // <Update_twin_with_device_temperature>
                        var updateTwinData3 = new JsonPatchDocument();
                        updateTwinData3.AppendReplace("/Light", light.Value<double>());
                        await client.UpdateDigitalTwinAsync(deviceId, updateTwinData3);
                        // </Update_twin_with_device_temperature>
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"Error in ingest function: {ex.Message}");
            }
        }
    }
}