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
using System.Collections.Generic;
using VideoFrameAnalyzer;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using Microsoft.Extensions.Configuration;
using CosmosDBLib;

namespace CongestionCameraConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // Get Secret variables.
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<Program>()
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
            FaceServiceClient faceClient = new FaceServiceClient(
                appSettings.Face_API_Subscription_Key, 
                appSettings.Face_API_Endpoint);

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
            FrameGrabber<Face[]> grabber = new FrameGrabber<Face[]>();

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
                return await faceClient.DetectAsync(frame.Image.ToMemoryStream(".jpg"));
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
                    faceCountDB.UpdateFaceCount(e.Analysis.Length, appSettings.PlaceName);
                    Console.WriteLine("New result received for frame acquired at {0}. {1} faces detected", e.Frame.Metadata.Timestamp, e.Analysis.Length);
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
    }
}
