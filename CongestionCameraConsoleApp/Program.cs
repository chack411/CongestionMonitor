// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Cognitive Services: http://www.microsoft.com/cognitive
// 
// Microsoft Cognitive Services Github:
// https://github.com/Microsoft/Cognitive
// 
// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 

using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Web;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Rest;
using Microsoft.Rest.Serialization;
using Newtonsoft.Json;
using VideoFrameAnalyzer;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Microsoft.Extensions.Configuration;
using CosmosDBLib;

namespace CongestionCameraConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Initializing Congestion Camera Console App...");

            // Get Secret variables.
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<Program>()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .AddCommandLine(args, GetCommandLineSwitchMappings())
                .Build();

            var appSettings = configuration.GetSection("Settings").Get<AppSettings>();
            if (appSettings == null)
            {
                Console.WriteLine("Failed App Settings initialization. Need to set variables.");
                return;
            }
            if (!appSettings.IsValid()) return;

            // Create Face API Client.
            var faceClient = new FaceClient(new ApiKeyServiceClientCredentials(appSettings.Face_API_Subscription_Key)) { Endpoint = appSettings.Face_API_Endpoint };
            IList<FaceAttributeType?> faceAttributes = new FaceAttributeType?[]
            {
                FaceAttributeType.Age,
                FaceAttributeType.Gender,
                FaceAttributeType.Smile,
                FaceAttributeType.FacialHair,
                FaceAttributeType.HeadPose,
                FaceAttributeType.Glasses,
                FaceAttributeType.Emotion,
                FaceAttributeType.Accessories
            };

            // Create CosmosDB Client.
            FaceCountDB faceCountDB = new FaceCountDB(
                appSettings.FaceCountDB_Endpoint, 
                appSettings.FaceCountDB_Key,
                appSettings.DatabaseId,
                appSettings.ContainerId);

            if (!faceCountDB.IsInitialized())
            {
                Console.WriteLine("Failed CosmosDB initialization.");
                return;
            }

            // Create grabber. 
            FrameGrabber<IList<DetectedFace>> grabber = new FrameGrabber<IList<DetectedFace>>();

            // Check Cameras.
            // At this time, there is a memory leak issue in Opencv 4.*
            // https://github.com/opencv/opencv/issues/13255
            // If you want to disable the warning messages, you try to set the environment variable of OPENCV_VIDEOIO_PRIORITY_MSMF to 0.
            int numCameras = grabber.GetNumCameras();
            if (numCameras == 0)
            {
                Console.WriteLine("No cameras found.");
                return;
            }

            Console.WriteLine($"Deteced {numCameras} camera(s).");
            if (appSettings.CameraIndex + 1 > numCameras)
            {
                Console.WriteLine("'CameraIndex' is invalid");
                return;
            }

            // Set up a listener for when we acquire a new frame.
            grabber.NewFrameProvided += (s, e) =>
            {
            };

            // Set up Face API call.
            grabber.AnalysisFunction = async frame =>
            {
                // Encode image and submit to Face API.

                // After the "mask' attribute is supported on the latest Face .NET SDK,
                // will replace this method with DetectWithStreamAsync.
                return await CallFaceApiAsync(frame.Image.ToMemoryStream(".jpg"), appSettings);

                //return await faceClient.Face.DetectWithStreamAsync(frame.Image.ToMemoryStream(".jpg"), true, false, faceAttributes);
                //return await faceClient.Face.DetectWithStreamAsync(frame.Image.ToMemoryStream(".jpg"), true, false,
                    // Hope this "mask" attribute will be added to Face .NET SDK soon
                    //returnFaceAttributes: new List<FaceAttributeType?> { FaceAttributeType.Mask }, 
                    // The recognition_04 model (published 2021) is the most accurate model currently available.
                    //recognitionModel: "recognition_04", returnRecognitionModel: true, 
                    // The detection_03 model (published 2021) is the latest model that should be used with recognition_04 model.
                    //detectionModel: "detection_03");
            };

            // Set up a listener for when we receive a new result from an API call. 
            grabber.NewResultAvailable += (s, e) =>
            {
                if (e.TimedOut)
                    Console.WriteLine("API call timed out.");
                else if (e.Exception != null)
                    Console.WriteLine("API call threw an exception.");
                else
                {
                    long maskCount = GetMaskCount2021(e.Analysis);
                    faceCountDB.UpdateFaceCount(e.Analysis.Count, maskCount, appSettings.PlaceName);
                    Console.WriteLine("New result received for frame acquired at {0}. {1} faces, {2} masks detected", e.Frame.Metadata.Timestamp, e.Analysis.Count, maskCount);
                }
            };

            // Tell grabber when to call API.
            // See also TriggerAnalysisOnPredicate
            grabber.TriggerAnalysisOnInterval(TimeSpan.FromSeconds(appSettings.AnalysisIntervalInSeconds));

            // Start running in the background.
            grabber.StartProcessingCameraAsync(appSettings.CameraIndex).Wait();

            // Wait for keypress to stop
            Console.WriteLine("Congestion Camera Console App has been successfully started.");
            Console.WriteLine("Press any key to stop...");
            Console.ReadKey();

            // Stop, blocking until done.
            grabber.StopProcessingAsync().Wait();
        }

        // This method is for the default model (recognition_01 and detection_01)
        private static long GetMaskCount(IList<Microsoft.Azure.CognitiveServices.Vision.Face.Models.DetectedFace> faces)
        {
            long maskCount = 0;
            foreach (var face in faces)
            {
                foreach (var accessories in face.FaceAttributes.Accessories)
                {
                    if (accessories.Type == AccessoryType.Mask)
                        ++maskCount;
                }
            }

            return maskCount;
        }

        // This method is for the default model (recognition_04 and detection_03)
        private static long GetMaskCount2021(IList<DetectedFace> faces)
        {
            long maskCount = 0;
            foreach (var face in faces)
            {
                if (face.FaceAttributes.Mask.Type != "noMask" && 
                    face.FaceAttributes.Mask.NoseAndMouthCovered == true)
                    ++maskCount;
            }

            return maskCount;
        }

        private static IDictionary<string, string> GetCommandLineSwitchMappings()
        {
            var switchMappings = new Dictionary<string, string>()
            {
                { "--CameraIndex", "Settings:CameraIndex" },
                { "-c", "Settings:CameraIndex" },
                { "--PlaceName", "Settings:PlaceName" },
                { "-p", "Settings:PlaceName" },
                { "--AnalysisIntervalInSeconds", "Settings:AnalysisIntervalInSeconds" },
                { "-a", "Settings:AnalysisIntervalInSeconds" }
            };

            return switchMappings;
        }

        private static async Task<IList<DetectedFace>> CallFaceApiAsync(Stream image, AppSettings appSettings)
        {            
            using (var client = new HttpClient())
            {
                var content = new StreamContent(image);

                var queryString = HttpUtility.ParseQueryString(string.Empty);

                // Request headers
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", appSettings.Face_API_Subscription_Key);

                // Request parameters
                queryString["returnFaceId"] = "true";
                queryString["returnFaceLandmarks"] = "false";
                queryString["returnFaceAttributes"] = "mask";
                queryString["recognitionModel"] = "recognition_04";
                queryString["returnRecognitionModel"] = "true";
                queryString["detectionModel"] = "detection_03";

                var url = appSettings.Face_API_Endpoint + "face/v1.0/detect?" + queryString;
                // var uri = "https://westus.api.cognitive.microsoft.com/face/v1.0/detect?" + queryString;

                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                var httpResponse = await client.PostAsync(url, content);

                if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    IList<DetectedFace> _body;

                    var deserializationSettings = new JsonSerializerSettings
                    {
                        DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat,
                        DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc,
                        NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
                        ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Serialize,
                        ContractResolver = new ReadOnlyJsonContractResolver(),
                        Converters = new List<JsonConverter>
                        {
                            new Iso8601TimeSpanConverter()
                        }
                    };

                    var _responseContent = await httpResponse.Content.ReadAsStringAsync();
                    try
                    {
                        _body = SafeJsonConvert.DeserializeObject<IList<DetectedFace>>(_responseContent, deserializationSettings);
                    }
                    catch (System.Text.Json.JsonException ex)
                    {
                        throw new SerializationException("Unable to deserialize the response.", _responseContent, ex);
                    }

                    return _body;
                }

                return null;
            }
        }
    }

    // Mask, FaceAttributes2, and DetectedFace2 are tentative implementations until the Face .NET SDK is updated.
    public class Mask
    {
        public Mask()
        {
        }

        public Mask(string type = default(string), bool noseAndMouthCovered = default(bool))
        {
            Type = type;
            NoseAndMouthCovered = noseAndMouthCovered;
        }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "noseAndMouthCovered")]
        public bool NoseAndMouthCovered { get; set; }
    }

    public class FaceAttributes
    {
        public FaceAttributes()
        {
        }

        public FaceAttributes(Mask mask = default(Mask))
        {
            Mask = mask;
        }

        [JsonProperty(PropertyName = "mask")]
        public Mask Mask { get; set; }
    }

    public class DetectedFace
    {
        public DetectedFace()
        {
        }

        public DetectedFace(FaceRectangle faceRectangle, System.Guid? faceId = default(System.Guid?), string recognitionModel = default(string), FaceLandmarks faceLandmarks = default(FaceLandmarks), FaceAttributes faceAttributes = default(FaceAttributes))
        {
            FaceId = faceId;
            RecognitionModel = recognitionModel;
            FaceRectangle = faceRectangle;
            FaceLandmarks = faceLandmarks;
            FaceAttributes = faceAttributes;
        }

        [JsonProperty(PropertyName = "faceId")]
        public System.Guid? FaceId { get; set; }

        [JsonProperty(PropertyName = "recognitionModel")]
        public string RecognitionModel { get; set; }

        [JsonProperty(PropertyName = "faceRectangle")]
        public FaceRectangle FaceRectangle { get; set; }

        [JsonProperty(PropertyName = "faceLandmarks")]
        public FaceLandmarks FaceLandmarks { get; set; }

        [JsonProperty(PropertyName = "faceAttributes")]
        public FaceAttributes FaceAttributes { get; set; }

        public virtual void Validate()
        {
            if (FaceRectangle == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "FaceRectangle");
            }
            if (FaceRectangle != null)
            {
                FaceRectangle.Validate();
            }
            if (FaceLandmarks != null)
            {
                FaceLandmarks.Validate();
            }
        }
    }
}
