using Azure;
using Azure.Core.Pipeline;
using Azure.DigitalTwins.Core;
using Azure.Identity;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;

//add new package for eventgrid: dotnet add package Microsoft.Azure.WebJobs.Extensions.EventHubs --version 5.2.0
namespace My.Function
{
    // This class processes telemetry events from IoT Hub, reads temperature of a device
    // and sets the "Temperature" property of the device with the value of the telemetry.
    public class telemetryfunction
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private static string adtServiceUrl = Environment.GetEnvironmentVariable("ADT_SERVICE_URL");

        [FunctionName("telemetryfunction")]
        public async Task Run([EventGridTrigger] EventGridEvent eventGridEvent, ILogger log)
        {
            try
            {
                // After this is deployed, you need to turn the Managed Identity Status to "On",
                // Grab Object Id of the function and assigned "Azure Digital Twins Owner (Preview)" role
                // to this function identity in order for this function to be authorized on ADT APIs.
                //Authenticate with Digital Twins
                var credentials = new DefaultAzureCredential();
                log.LogInformation(credentials.ToString());
                DigitalTwinsClient client = new DigitalTwinsClient(
                    new Uri(adtServiceUrl), credentials, new DigitalTwinsClientOptions
                    { Transport = new HttpClientTransport(httpClient) });
                log.LogInformation($"ADT service client connection created.");
                if (eventGridEvent.Data.ToString().Contains("Alert"))
                {
                    /*JObject alertMessage = (JObject)JsonConvert.DeserializeObject(eventGridEvent.Data.ToString());
                    string deviceId = (string)alertMessage["systemProperties"]["iothub-connection-device-id"];
                    var ID = alertMessage["body"]["TurbineID"];
                    var alert = alertMessage["body"]["Alert"];
                    log.LogInformation($"Device:{deviceId} Device Id is:{ID}");
                    log.LogInformation($"Device:{deviceId} Alert Status is:{alert}");

                    var updateProperty = new JsonPatchDocument();
                    updateProperty.AppendReplace("/Alert", alert.Value<bool>());
                    updateProperty.AppendReplace("/TurbineID", ID.Value<string>());
                    log.LogInformation(updateProperty.ToString());
                    try
                    {
                        await client.UpdateDigitalTwinAsync(deviceId, updateProperty);
                    }
                    catch (Exception e)
                    {
                        log.LogInformation(e.Message);
                    }*/
                }
                else if (eventGridEvent != null && eventGridEvent.Data != null)
                {

                    JObject deviceMessage = (JObject)JsonConvert.DeserializeObject(eventGridEvent.Data.ToString());
                    /*string deviceId = (string)deviceMessage["systemProperties"]["iothub-connection-device-id"];
                    var ID = deviceMessage["body"]["TurbineID"];
                    var TimeInterval = deviceMessage["body"]["TimeInterval"];
                    var Description = deviceMessage["body"]["Description"];
                    var Code = deviceMessage["body"]["Code"];
                    var WindSpeed = deviceMessage["body"]["WindSpeed"];
                    var Ambient = deviceMessage["body"]["Ambient"];
                    var Rotor = deviceMessage["body"]["Rotor"];
                    var Power = deviceMessage["body"]["Power"];*/

                    //var ID = deviceMessage["body"]["SensorID"];
                    var ID = "MangOHBoard";
                    //string deviceId = "hclfrisco123";
                    string deviceId = "MangOHBoard";

                    // var Temperature = deviceMessage["body"]["elems"]["imu"]["temp"];
                    // var Light = deviceMessage["body"]["elems"]["light"];
                    // var Vibration = deviceMessage["body"]["elems"]["vibration"];

                    if (deviceMessage["body"]["elems"]["imu"] != null && deviceMessage["body"]["elems"]["imu"]["temp"] != null){
                        var temperature = deviceMessage["body"]["elems"]["imu"]["temp"];
                        log.LogInformation($"Device:{deviceId} temp is:{temperature}");

                        // <Update_twin_with_device_temperature>
                        //var updateTwinData1 = new JsonPatchDocument();
                        //updateTwinData1.AppendReplace("/Temperature", temperature.Value<double>());
                        //await client.UpdateDigitalTwinAsync(deviceId, updateTwinData1);

                        var turbineTelemetry = new Dictionary<string, Object>(){
                            ["Temperature"] = temperature, //["elems"]["imu"]["temp"] = MangohTemperature,
                        };

                        try
                        {
                            await client.PublishTelemetryAsync(deviceId, Guid.NewGuid().ToString(), JsonConvert.SerializeObject(turbineTelemetry));
                        }
                        catch (Exception e)
                        {
                            log.LogInformation(e.Message);
                        }

                        // </Update_twin_with_device_temperature>
                    } 
                    else if (deviceMessage["body"]["elems"]["imu"] != null && deviceMessage["body"]["elems"]["imu"]["accel"] != null){
                        var x = deviceMessage["body"]["elems"]["imu"]["accel"]["x"];
                        var y = deviceMessage["body"]["elems"]["imu"]["accel"]["y"];
                        var z = deviceMessage["body"]["elems"]["imu"]["accel"]["z"];
                        log.LogInformation($"Device:{deviceId} x is:{x} y is:{y} and z is:{z} ");

                        // var updateTwinData2 = new JsonPatchDocument();
                        // updateTwinData2.AppendReplace("/x", x.Value<double>());
                        // updateTwinData2.AppendReplace("/y", y.Value<double>());
                        // updateTwinData2.AppendReplace("/z", z.Value<double>());
                        // await client.UpdateDigitalTwinAsync(deviceId, updateTwinData2);
                    
                        var turbineTelemetry = new Dictionary<string, Object>(){
                            ["Vibration"] = "YES", //["elems"]["imu"]["temp"] = MangohTemperature,
                        };

                        try
                        {
                            await client.PublishTelemetryAsync(deviceId, Guid.NewGuid().ToString(), JsonConvert.SerializeObject(turbineTelemetry));
                        }
                        catch (Exception e)
                        {
                            log.LogInformation(e.Message);
                        }

                    } 
                    else if(deviceMessage["body"]["elems"]["light"] != null){
                        var light = deviceMessage["body"]["elems"]["light"];
                        log.LogInformation($"Device:{deviceId} light is:{light}");

                        // // <Update_twin_with_device_temperature>
                        // var updateTwinData3 = new JsonPatchDocument();
                        // updateTwinData3.AppendReplace("/Light", light.Value<double>());
                        // await client.UpdateDigitalTwinAsync(deviceId, updateTwinData3);
                        // // </Update_twin_with_device_temperature>

                         var turbineTelemetry = new Dictionary<string, Object>(){
                            ["Light"] = light, //["elems"]["imu"]["temp"] = MangohTemperature,
                        };

                        try
                        {
                            await client.PublishTelemetryAsync(deviceId, Guid.NewGuid().ToString(), JsonConvert.SerializeObject(turbineTelemetry));
                        }
                        catch (Exception e)
                        {
                            log.LogInformation(e.Message);
                        }
                    }

                    // log.LogInformation($"Device:{deviceId} Device Id is:{deviceId}");
                    // log.LogInformation($"Device:{deviceId} Temperature is:{Temperature}");
                    // log.LogInformation($"Device:{deviceId} Light is:{Light}");
                    // log.LogInformation($"Device:{deviceId} Vibration is:{Vibration}");

                    /*log.LogInformation($"Device:{deviceId} Time interval is:{TimeInterval}");
                    log.LogInformation($"Device:{deviceId} Description is:{Description}");
                    log.LogInformation($"Device:{deviceId} CodeNumber is:{Code}");
                    log.LogInformation($"Device:{deviceId} WindSpeed is:{WindSpeed}");
                    log.LogInformation($"Device:{deviceId} Ambient Temperature is:{Ambient}");
                    log.LogInformation($"Device:{deviceId} Rotor RPM is:{Rotor}");
                    log.LogInformation($"Device:{deviceId} Power is:{Power}");*/
                    //var updateProperty = new JsonPatchDocument();
                    // var turbineTelemetry = new Dictionary<string, Object>()
                    // {
                    //     //["TurbineID"] = ID,
                    //     //["SensorID"] = ID,
                    //     ["Temperature"] = Temperature, //["elems"]["imu"]["temp"] = MangohTemperature,
                    //     ["Light"] = Light,
                    //     ["Vibration"] = Vibration
                        
                    //     /*["TimeInterval"] = TimeInterval,
                    //     ["Description"] = Description,
                    //     ["Code"] = Code,
                    //     ["WindSpeed"] = WindSpeed,
                    //     ["Ambient"] = Ambient,
                    //     ["Rotor"] = Rotor,
                    //     ["Power"] = Power*/
                    // };
                    // //updateProperty.AppendAdd("/TurbineID", ID.Value<string>()); 

                    // log.LogInformation(updateProperty.ToString());
                    // try
                    // {
                    //     await client.PublishTelemetryAsync(deviceId, Guid.NewGuid().ToString(), JsonConvert.SerializeObject(turbineTelemetry));
                    // }
                    // catch (Exception e)
                    // {
                    //     log.LogInformation(e.Message);
                    // }
                }
            }
            catch (Exception e)
            {
                log.LogInformation(e.Message);
            }
        }
    }
}